﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageDetailsService.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer
{
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Documents;
    using System.Windows.Media;
    using Catel;
    using Catel.Services;
    using NuGet.Protocol.Core.Types;

    internal class PackageDetailsService : IPackageDetailsService
    {
        private readonly IRepositoryNavigatorService _repositoryNavigatorService;

        private readonly ILanguageService _languageService;

        public PackageDetailsService(IRepositoryNavigatorService repositoryNavigatorService, ILanguageService languageService)
        {
            Argument.IsNotNull(() => repositoryNavigatorService);
            Argument.IsNotNull(() => languageService);

            _repositoryNavigatorService = repositoryNavigatorService;
            _languageService = languageService;
        }

        #region Methods
        public async Task<FlowDocument> PackageToFlowDocumentAsync(IPackageSearchMetadata package)
        {
            Argument.IsNotNull(() => package);

            var result = new FlowDocument();

            var paragraph = new Paragraph
            {
                FontSize = 12
            };

            var autors = GetDetailsRecord(_languageService.GetString("NuGetExplorer_PackageDetailsService_PackageToFlowDocument_GetDetailsRecord_CreatedBy"), package.Authors);
            paragraph.Inlines.AddIfNotNull(autors);

            var id = GetDetailsRecord(_languageService.GetString("NuGetExplorer_PackageDetailsService_PackageToFlowDocument_GetDetailsRecord_Id"), package.Identity.Id);
            paragraph.Inlines.AddIfNotNull(id);

            var version = GetDetailsRecord(_languageService.GetString("NuGetExplorer_PackageDetailsService_PackageToFlowDocument_GetDetailsRecord_Version"), GetVersion(package));
            paragraph.Inlines.AddIfNotNull(version);

            var published = package.Published;

            if (published != null && _repositoryNavigatorService.Navigator.SelectedRepository.OperationType != PackageOperationType.Uninstall)
            {
                paragraph.Inlines.AddIfNotNull(GetDetailsRecord(_languageService.GetString("NuGetExplorer_PackageDetailsService_PackageToFlowDocument_GetDetailsRecord_Published"), published.Value.LocalDateTime.ToString(CultureInfo.CurrentCulture.DateTimeFormat)));
            }

            if (published != null && _repositoryNavigatorService.Navigator.SelectedRepository.OperationType == PackageOperationType.Uninstall)
            {
                paragraph.Inlines.AddIfNotNull(GetDetailsRecord(_languageService.GetString("NuGetExplorer_PackageDetailsService_PackageToFlowDocument_GetDetailsRecord_Installed"), published.Value.LocalDateTime.ToString(CultureInfo.CurrentCulture.DateTimeFormat)));
            }

            var downloads = GetDetailsRecord(_languageService.GetString("NuGetExplorer_PackageDetailsService_PackageToFlowDocument_GetDetailsRecord_Downloads"), package.DownloadCount.ToString());
            paragraph.Inlines.AddIfNotNull(downloads);

            //var dependencyInfos = await package.Dependencies;
            //if (dependencyInfos.Count == 0)
            //{
            //    var dependencies = GetDetailsRecord(_languageService.GetString("NuGetExplorer_PackageDetailsService_PackageToFlowDocument_GetDetailsRecord_Dependencies"), dependencyInfos.Select(x => x.Id).ToArray());
            //    paragraph.Inlines.AddIfNotNull(dependencies);
            //}

            //if (package.ValidationContext.GetErrorCount(ValidationTags.Api) > 0)
            //{
            //    var validations = GetAlertRecords(_languageService.GetString("NuGetExplorer_PackageDetailsService_PackageToFlowDocument_GetAlertRecords_Errors"), package.ValidationContext.GetErrors(ValidationTags.Api).Select(s => " - " + s.Message).ToArray());
            //    paragraph.Inlines.AddIfNotNull(validations);
            //    paragraph.Inlines.Add(new LineBreak());

            //    paragraph.Inlines.Add(_languageService.GetString("NuGetExplorer_PackageDetailsService_PackageToFlowDocument_Update_The_Shell_Error_Message").ToInline(Brushes.Red));
            //    paragraph.Inlines.Add(new LineBreak());
            //}

            result.Blocks.Add(paragraph);
            return result;
        }

        private static string GetVersion(IPackageSearchMetadata package)
        {
            if (!package.Identity.Version.IsPrerelease)
            {
                return package.Identity.Version.ToString();
            }

            return package.Identity.Version.OriginalVersion;
        }

        private Inline GetAlertRecords(string title, params string[] stringLines)
        {
            Argument.IsNotNullOrWhitespace(() => title);

            if (stringLines == null)
            {
                return null;
            }

            var valuableLines = stringLines.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            if (!valuableLines.Any())
            {
                return null;
            }

            var inlines = valuableLines.Select(line => line.ToInline(Brushes.Red).Append(new LineBreak())).ToList();
            var inline = title.ToInline().Append(new LineBreak());
            var resultInline = inline.Bold().AppendRange(inlines);
            return resultInline;
        }

        private Inline GetDetailsRecord(string title, params string[] stringLines)
        {
            Argument.IsNotNullOrWhitespace(() => title);

            if (stringLines == null)
            {
                return null;
            }

            var valuableLines = stringLines.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            if (!valuableLines.Any())
            {
                return null;
            }

            var inlines = valuableLines.Select(line => line.ToInline().Append(new LineBreak())).ToList();

            var inline = title.ToInline();
            var resultInline = inline.Bold().AppendRange(inlines);
            return resultInline;
        }
        #endregion
    }
}
