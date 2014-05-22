using System;

namespace AppPlate.Core.WorkFlow
{
    public class ViewModelContainerLocator : IViewModelContainerLocator
    {
        private readonly IContainer _container;

        public ViewModelContainerLocator(IContainer container)
        {
            _container = container;
        }

        public TViewModel GetViewModelInstance<TViewModel>()
            where TViewModel : ViewModelBase
        {
            return _container.GetInstance<TViewModel>();
        }

        public TViewModel GetViewModelInstance<TViewModel>(Type viewModelType) where TViewModel : ViewModelBase
        {
            return _container.GetInstance(viewModelType) as TViewModel;
        }
    }
}