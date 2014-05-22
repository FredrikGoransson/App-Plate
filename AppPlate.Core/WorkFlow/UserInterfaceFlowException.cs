using System;

namespace AppPlate.Core.WorkFlow
{
    public class UserInterfaceFlowException : Exception
    {
        private UserInterfaceFlowException(string message) : base(message)
        {
            
        }

        public static UserInterfaceFlowException NextStepNotFound()
        {
            return new UserInterfaceFlowException("The next step in the user interface flow could not be found.");
        }

        public static UserInterfaceFlowException BackStepNotFound()
        {
            return new UserInterfaceFlowException("The back step in the user interface flow could not be found.");
        }

        public static UserInterfaceFlowException OutOfSyncWithNavigationService(Type flowViewType, Type navigationViewType)
        {
            return
                new UserInterfaceFlowException(
                    string.Format("The current step in the current flow is '{0}' but Navigation reports it as '{1}'",
                        flowViewType.Name, navigationViewType.Name));
        }

        public static UserInterfaceFlowException InitialStepNotFound()
        {
            return new UserInterfaceFlowException("The initial (or any) step in the user interface flow could not be found.");
        }

        public static UserInterfaceFlowException StepNotFound<TViewModel>()
            where TViewModel : ViewModelBase
        {
            return new UserInterfaceFlowException(string.Format("The step for ViewModel {0} in the user interface flow could not be found.", typeof(TViewModel).FullName));
        }

        public static UserInterfaceFlowException StepNotFound(Type viewModelType)
        {
            return new UserInterfaceFlowException(string.Format("The step for ViewModel {0} in the user interface flow could not be found.", viewModelType.FullName));
        }

        public static UserInterfaceFlowException StepNotFound(string viewName)
        {
            return new UserInterfaceFlowException(string.Format("The step with name '{0}View.xaml' or '{0}.xaml' in the user interface flow could not be found.", viewName));
        }

        public static Exception CannotGoBackWhenCurrentStepIsNull()
        {
            return new UserInterfaceFlowException("The current flowstep is not set so back action is not allowed.");
        }
    }
}