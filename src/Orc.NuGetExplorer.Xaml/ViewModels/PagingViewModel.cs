// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PagingViewModel.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.NuGetExplorer.ViewModels
{
    using Catel;
    using Catel.Fody;
    using Catel.MVVM;

    internal class PagingViewModel : ViewModelBase
    {
        #region Fields
        private readonly IPagingService _pagingService;
        #endregion

        #region Constructors
        public PagingViewModel(IPagingService pagingService)
        {
            Argument.IsNotNull(() => pagingService);

            _pagingService = pagingService;

            Pager = new Pager();

            MoveToPage = new Command<PagingItemInfo>(OnMoveToPageExecute, OnMoveToPageCanExecute);
            MoveToFirst = new Command(OnMoveToFirstExecute, OnMoveToFirstCanExecute);
            MoveBack = new Command(OnMoveBackExecute, OnMoveBackCanExecute);
            MoveForward = new Command(OnMoveForwardExecute, OnMoveForwardCanExecute);
            MoveToLast = new Command(OnMoveToLastExcute, OnMoveToLastCanExecute);
        }
        #endregion

        #region Properties
        [Model(SupportIEditableObject = true)]
        [Expose(nameof(NuGetExplorer.Pager.LeftPages))]
        [Expose(nameof(NuGetExplorer.Pager.RightPages))]
        [Expose(nameof(NuGetExplorer.Pager.CurrentPage))]
        public Pager Pager { get; private set; }

        [ViewModelToModel(nameof(Pager))]
        public int ItemIndex { get; set; }

        [ViewModelToModel(nameof(Pager))]
        public int VisiblePages { get; set; }

        [ViewModelToModel(nameof(Pager))]
        public bool CanContinue { get; set; }

        [ViewModelToModel(nameof(Pager))]
        public int ItemsPerPage { get; set; }
        #endregion

        #region Methods
        private void OnVisiblePagesChanged()
        {
       //     _pagingService.UpdatePagingItems(Pager);
        }

        private void OnItemsCountChanged()
        {
            Pager.CanContinue = CanContinue; // Note: doesn't refresh automatically
        //    _pagingService.UpdatePagingItems(Pager);
        }

        private void OnItemsPerPageChanged()
        {
       //     _pagingService.UpdatePagingItems(Pager);
        }

        private void OnItemIndexChanged()
        {
        //    _pagingService.UpdatePagingItems(Pager);
        }

        private bool CanMoveForward()
        {
            var tooFewItems = !CanContinue;
            var lastPage = _pagingService.IsLastPage(Pager);
            return !lastPage && !tooFewItems;
        }
        #endregion

        #region Commands
        public Command MoveToLast { get; private set; }

        private void OnMoveToLastExcute()
        {
         //   _pagingService.MoveToLast(Pager);
        }

        private bool OnMoveToLastCanExecute()
        {
            return CanMoveForward();
        }

        public Command MoveForward { get; private set; }

        private void OnMoveForwardExecute()
        {
            _pagingService.Step(Pager, 1);
        }

        private bool OnMoveForwardCanExecute()
        {
            return CanMoveForward();
        }

        public Command MoveBack { get; private set; }

        private void OnMoveBackExecute()
        {
            if (ItemIndex == 0)
            {
                return;
            }

            _pagingService.Step(Pager, -1);
        }

        private bool OnMoveBackCanExecute()
        {
            return ItemIndex > 0;
        }

        public Command MoveToFirst { get; private set; }

        public void OnMoveToFirstExecute()
        {
            _pagingService.MoveToFirst(Pager);
        }

        private bool OnMoveToFirstCanExecute()
        {
            return ItemIndex > 0;
        }

        public Command<PagingItemInfo> MoveToPage { get; private set; }

        private void OnMoveToPageExecute(PagingItemInfo pagingItem)
        {
         //   _pagingService.StepTo(Pager, pagingItem);
        }

        private bool OnMoveToPageCanExecute(PagingItemInfo pagingItem)
        {
            return true;
        }
        #endregion
    }
}
