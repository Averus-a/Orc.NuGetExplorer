// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MultiSourcePackageFeed.cs" company="WildGums">
//   Copyright (c) 2008 - 2019 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Orc.NuGetExplorer
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Catel;
    using Catel.Services;
    using NuGet.Common;
    using NuGet.Indexing;
    using NuGet.ProjectManagement;
    using NuGet.Protocol.Core.Types;
    using Telemetry;

    public sealed class MultiSourcePackageFeed : IPackageFeed
    {
        #region Fields
        private const int PageSize = 25;
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);
        private readonly IDispatcherService _dispatcherService;
        private readonly INuGetUILogger _logger;

        private readonly SourceRepository[] _sourceRepositories;
        private readonly INuGetTelemetryService _telemetryService;
        #endregion

        #region Constructors
        public MultiSourcePackageFeed(
            IEnumerable<SourceRepository> sourceRepositories,
            INuGetUILogger logger,
            INuGetTelemetryService telemetryService,
            IDispatcherService dispatcherService)
        {
            Argument.IsNotNull(() => sourceRepositories);
            Argument.IsNotNull(() => dispatcherService);

            if (!sourceRepositories.Any())
            {
                throw new ArgumentException("Collection of source repositories cannot be empty", nameof(sourceRepositories));
            }

            _sourceRepositories = sourceRepositories.ToArray();
            _telemetryService = telemetryService;
            _dispatcherService = dispatcherService;
            _logger = logger;
        }
        #endregion

        #region Properties
        public bool IsMultiSource => _sourceRepositories.Length > 1;
        #endregion

        #region Methods
        public async Task<SearchResult<IPackageSearchMetadata>> SearchAsync(string searchText, SearchFilter filter, CancellationToken cancellationToken)
        {
            var searchOperationId = Guid.NewGuid();
            if (_telemetryService != null)
            {
                _telemetryService.EmitTelemetryEvent(new SearchTelemetryEvent(
                    searchOperationId,
                    searchText,
                    filter.IncludePrerelease));

                _telemetryService.EmitTelemetryEvent(SourceTelemetry.GetSearchSourceSummaryEvent(
                    searchOperationId,
                    _sourceRepositories.Select(x => x.PackageSource)));
            }

            var searchTasks = TaskCombinators.ObserveErrorsAsync(
                _sourceRepositories,
                r => r.PackageSource.Name,
                (r, t) => r.SearchAsync(searchText, filter, PageSize, t),
                LogError,
                cancellationToken);

            return await WaitForCompletionOrBailOutAsync(
                searchText,
                searchTasks,
                new TelemetryState(searchOperationId, 0),
                cancellationToken);
        }

        public async Task<SearchResult<IPackageSearchMetadata>> ContinueSearchAsync(SearchCursor continuationToken, CancellationToken cancellationToken)
        {
            var searchToken = continuationToken as AggregatedSearchCursor;

            if (searchToken?.SourceSearchCursors == null)
            {
                throw new InvalidOperationException("Invalid token");
            }

            var searchTokens = _sourceRepositories
                .Join(searchToken.SourceSearchCursors,
                    r => r.PackageSource.Name,
                    c => c.Key,
                    (r, c) => new {Repository = r, NextToken = c.Value});

            var searchTasks = TaskCombinators.ObserveErrorsAsync(
                searchTokens,
                j => j.Repository.PackageSource.Name,
                (j, t) => j.Repository.SearchAsync(j.NextToken, PageSize, t),
                LogError,
                cancellationToken);

            return await WaitForCompletionOrBailOutAsync(
                searchToken.SearchString,
                searchTasks,
                searchToken.TelemetryState?.NextPage(),
                cancellationToken);
        }

        public async Task<SearchResult<IPackageSearchMetadata>> RefreshSearchAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
        {
            var searchToken = refreshToken as AggregatedRefreshToken;

            if (searchToken == null)
            {
                throw new InvalidOperationException("Invalid token");
            }

            return await WaitForCompletionOrBailOutAsync(
                searchToken.SearchString,
                searchToken.SearchTasks,
                searchToken.TelemetryState,
                cancellationToken);
        }

        private async Task<SearchResult<IPackageSearchMetadata>> WaitForCompletionOrBailOutAsync(
            string searchText,
            IDictionary<string, Task<SearchResult<IPackageSearchMetadata>>> searchTasks,
            TelemetryState telemetryState,
            CancellationToken cancellationToken)
        {
            if (searchTasks.Count == 0)
            {
                return SearchResult.Empty();
            }

            var aggregatedTask = Task.WhenAll(searchTasks.Values);

            RefreshToken refreshToken = null;
            if (aggregatedTask != await Task.WhenAny(aggregatedTask, Task.Delay(DefaultTimeout)))
            {
                refreshToken = new AggregatedRefreshToken
                {
                    TelemetryState = telemetryState,
                    SearchString = searchText,
                    SearchTasks = searchTasks,
                    RetryAfter = DefaultTimeout
                };
            }

            var partitionedTasks = searchTasks
                .ToLookup(t => t.Value.Status == TaskStatus.RanToCompletion);

            var completedOnly = partitionedTasks[true];

            SearchResult<IPackageSearchMetadata> aggregated;

            if (completedOnly.Any())
            {
                var results = await Task.WhenAll(completedOnly.Select(kv => kv.Value));
                aggregated = await AggregateSearchResultsAsync(searchText, results, telemetryState);
            }
            else
            {
                aggregated = SearchResult.Empty();
            }

            aggregated.OperationId = telemetryState?.OperationId;
            aggregated.RefreshToken = refreshToken;

            var notCompleted = partitionedTasks[false];

            if (notCompleted.Any())
            {
                var statuses = notCompleted.ToDictionary(
                    kv => kv.Key,
                    kv => GetLoadingStatus(kv.Value.Status));

                foreach (var item in statuses)
                {
                    aggregated.SearchStatusBySource.Add(item);
                }

                var exceptions = notCompleted
                    .Where(kv => kv.Value.Exception != null)
                    .ToDictionary(
                        kv => kv.Key,
                        kv => (Exception)kv.Value.Exception);

                foreach (var item in exceptions)
                {
                    aggregated.SearchExceptionBySource.Add(item);
                }
            }

            if (_telemetryService != null &&
                aggregated.SearchStatusBySource?.Values != null &&
                telemetryState != null)
            {
                var loadingStatus = aggregated.SearchStatusBySource.Values.Aggregate();
                if (loadingStatus != SearchStatus.Loading
                    && telemetryState.ShouldEmit)
                {
                    telemetryState.Duration.Stop();
                    _telemetryService.EmitTelemetryEvent(new SearchPageTelemetryEvent(
                        telemetryState.OperationId,
                        telemetryState.PageIndex,
                        aggregated.Items?.Count ?? 0,
                        telemetryState.Duration.Elapsed,
                        loadingStatus));
                }
            }

            return aggregated;
        }

        private static SearchStatus GetLoadingStatus(TaskStatus taskStatus)
        {
            switch (taskStatus)
            {
                case TaskStatus.Canceled:
                    return SearchStatus.Cancelled;
                case TaskStatus.Created:
                case TaskStatus.RanToCompletion:
                case TaskStatus.Running:
                case TaskStatus.WaitingForActivation:
                case TaskStatus.WaitingForChildrenToComplete:
                case TaskStatus.WaitingToRun:
                    return SearchStatus.Loading;
                case TaskStatus.Faulted:
                    return SearchStatus.Failed;
                default:
                    return SearchStatus.Unknown;
            }
        }

        private async Task<SearchResult<IPackageSearchMetadata>> AggregateSearchResultsAsync(
            string searchText,
            IEnumerable<SearchResult<IPackageSearchMetadata>> results,
            TelemetryState telemetryState)
        {
            SearchResult<IPackageSearchMetadata> result;

            var nonEmptyResults = results.Where(r => r.Any()).ToArray();
            if (nonEmptyResults.Length == 0)
            {
                result = SearchResult.Empty();
            }
            else if (nonEmptyResults.Length == 1)
            {
                result = SearchResult.FromItems(nonEmptyResults[0].Items);
            }
            else
            {
                var items = nonEmptyResults.Select(r => r.Items).ToArray();

                var indexer = new RelevanceSearchResultsIndexer();
                var aggregator = new SearchResultsAggregator(indexer, new PackageSearchMetadataSplicer());
                var aggregatedItems = await aggregator.AggregateAsync(
                    searchText, items);

                result = SearchResult.FromItems(aggregatedItems.ToArray());
                // set correct count of unmerged items
                result.TotalItemsCount = items.Aggregate(0, (r, next) => r + next.Count);
            }

            result.SearchStatusBySource = results
                .SelectMany(r => r.SearchStatusBySource)
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            var cursors = results
                .Where(r => r.Cursor != null)
                .ToDictionary(r => r.SearchStatusBySource.Single().Key, r => r.Cursor);

            if (cursors.Keys.Any())
            {
                result.Cursor = new AggregatedSearchCursor
                {
                    TelemetryState = telemetryState,
                    SearchString = searchText,
                    SourceSearchCursors = cursors
                };
            }

            return result;
        }

        private void LogError(Task task, object state)
        {
            if (_logger == null)
            {
                // observe the task exception when no UI logger provided.
                Trace.WriteLine(ExceptionUtilities.DisplayMessage(task.Exception));
                return;
            }

            _dispatcherService.Invoke(() =>
            {
                var errorMessage = ExceptionUtilities.DisplayMessage(task.Exception);
                _logger.Log(
                    MessageLevel.Error,
                    $"[{state}] {errorMessage}");
            });
        }
        #endregion

        private class TelemetryState
        {
            #region Fields
            private int _emittedFlag;
            #endregion

            #region Constructors
            public TelemetryState(Guid parentId, int pageIndex)
            {
                OperationId = parentId;
                PageIndex = pageIndex;
                Duration = Stopwatch.StartNew();
            }
            #endregion

            #region Properties
            public Guid OperationId { get; }
            public int PageIndex { get; }
            public Stopwatch Duration { get; }

            /// <summary>
            ///     This telemetry state should be emitted exactly once. This property will return true the first time it
            ///     is called, then false for every subsequent call.
            /// </summary>
            public bool ShouldEmit
            {
                get
                {
                    var value = Interlocked.CompareExchange(ref _emittedFlag, 1, 0);
                    return value == 0;
                }
            }
            #endregion

            #region Methods
            public TelemetryState NextPage()
            {
                return new TelemetryState(OperationId, PageIndex + 1);
            }
            #endregion
        }

        private class AggregatedSearchCursor : SearchCursor
        {
            #region Properties
            public TelemetryState TelemetryState { get; set; }
            public IDictionary<string, SearchCursor> SourceSearchCursors { get; set; } = new Dictionary<string, SearchCursor>();
            #endregion
        }

        private class AggregatedRefreshToken : RefreshToken
        {
            #region Properties
            public TelemetryState TelemetryState { get; set; }
            public string SearchString { get; set; }
            public IDictionary<string, Task<SearchResult<IPackageSearchMetadata>>> SearchTasks { get; set; }
            public IDictionary<string, SearchStatus> SourceSearchStatus { get; set; }
            #endregion
        }
    }
}
