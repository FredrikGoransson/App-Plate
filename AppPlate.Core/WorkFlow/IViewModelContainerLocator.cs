using System;

namespace AppPlate.Core.WorkFlow
{
    public interface IViewModelContainerLocator
    {
        TViewModel GetViewModelInstance<TViewModel>() where TViewModel : ViewModelBase;
        TViewModel GetViewModelInstance<TViewModel>(Type viewModelType) where TViewModel : ViewModelBase;
    }
}