using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using AppPlate.Core.Concurrency;
using AppPlate.Core.Extensions;
using AppPlate.Core.Messaging;

namespace AppPlate.Core.WorkFlow
{
    public abstract class UserInterfaceFlowBase : UserInterfaceFlowBase<UserInterfaceFlowBase.EmptyState>
    {
        protected UserInterfaceFlowBase(
            IUserInterfaceManager manager, 
            IViewModelContainerLocator viewModelContainerLocator, 
            IMessengerHub messengerHub, 
            IDispatcher uiDispatcher) : 
            base(manager, viewModelContainerLocator, messengerHub, uiDispatcher)
        {
        }

        protected override EmptyState GetCurrentState()
        {
            return new EmptyState();
        }

        protected override void SetCurrentState(EmptyState stateObject)
        {            
        }

        public class EmptyState
        {

        }
    }

    public abstract class UserInterfaceFlowBase<TStateObject> : IUserInterfaceFlow
        where TStateObject : class
    {
        private readonly IViewModelContainerLocator _viewModelContainerLocator;

        protected readonly IDispatcher UIDispatcher;
        protected readonly IUserInterfaceManager Manager;
        protected readonly IMessengerHub MessengerHub;

        private bool _isActive;

        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    if (_isActive) Activated();
                    else Deactivated();
                }
            }
        }

        protected bool SuppressGeneralErrorHandling { get; set; }

        public IUserInterfaceFlow PreviousFlow { get; private set; }

        public IList<FlowStep> Steps { get; protected set; }

        protected FlowStep CurrentStep { get; set; }

        public UserInterfaceFlowBase(
            IUserInterfaceManager manager, 
            IViewModelContainerLocator viewModelContainerLocator,
            IMessengerHub messengerHub, 
            IDispatcher uiDispatcher)
        {
            UIDispatcher = uiDispatcher;
            Manager = manager;
            _viewModelContainerLocator = viewModelContainerLocator;
            MessengerHub = messengerHub;

            SuppressGeneralErrorHandling = false;
        }

        protected abstract TStateObject GetCurrentState();

        protected abstract void SetCurrentState(TStateObject stateObject);

        public TViewModel CurrentViewModel<TViewModel>() 
            where TViewModel : ViewModelBase
        {
            if (CurrentStep == null) return null;
            if (CurrentStep.ViewModelType == null) return null;
            return _viewModelContainerLocator.GetViewModelInstance<TViewModel>(CurrentStep.ViewModelType);
        }

        public TViewModel FindViewModel<TViewModel>()
            where TViewModel : ViewModelBase
        {
            return _viewModelContainerLocator.GetViewModelInstance<TViewModel>();
        }

        private FlowStep GetInitalFlowStep()
        {
            var firstStep = Steps.FirstOrDefault(step => step.IsInitial);
            if (firstStep == null) firstStep = Steps.FirstOrDefault();
            if (firstStep == null) throw UserInterfaceFlowException.InitialStepNotFound();

            return firstStep;
        }

        public virtual string GetInitalStep()
        {
            var firstStep = GetInitalFlowStep();
            return firstStep.GetStepName();
        }

        public string GetCurrentStep()
        {
            return CurrentStep != null ? CurrentStep.GetStepName() : "";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stepName"></param>
        public void BeforeBack(string stepName)
        {
            var step = Steps.GetStepForName(stepName);

            // Check for cancel current step
            if (step.Back.CancelFlow && PreviousFlow != null)
            {
                Manager.Navigate(PreviousFlow.GetType(), forward:false);
                return;
            }

            // Check for step back to another point in the flow
            if (step.Back.StepTo != null)
            {
                var flowStep = step.Back.StepTo();
                if (flowStep != null)
                {
                    NavigateToStep(flowStep, forward:false);
                    return;
                }
            }            
        }

        /// <summary>
        /// Never called.
        /// </summary>
        /// <param name="stepName"></param>
        public void BeforeGoto(string stepName)
        {
            
        }

        /// <summary>
        /// Called when the flow is either entered (<see cref="Enter"/>) or returned to (<see cref="ReturnTo"/>
        /// Useful for hooking up messagehub subscriptions that should be active while this flow is active
        /// </summary>
        protected virtual void Activated()
        {
            if (!SuppressGeneralErrorHandling)
            {
                MessengerHub.Subscribe<CommunicationErrorOccured>(this, OnCommunicationError, UIDispatcher);
                MessengerHub.Subscribe<NetworkErrorOccured>(this, OnNetworkError, UIDispatcher);
                MessengerHub.Subscribe<Failure>(this, OnFailure, UIDispatcher);
            }
        }

        /// <summary>
        /// Called when the flow is exited (<see cref="Exit"/>) or cancelled (<see cref="Cancel"/>)
        /// Useful for unhooking messagehub subscriptions
        /// </summary>
        protected virtual void Deactivated()
        {
            if (!SuppressGeneralErrorHandling)
            {
                MessengerHub.Unsubscribe<Failure>(this);
                MessengerHub.Unsubscribe<CommunicationErrorOccured>(this);
                MessengerHub.Unsubscribe<NetworkErrorOccured>(this);
            }
        }

        /// <summary>
        /// Called when the flow is started from a fresh point, i.e. not returned to or resumed
        /// </summary>
        /// <param name="fromFlow"></param>
        /// <param name="parameters">Any parameters passed to this flow from the navigation</param>
        public virtual void Enter(IUserInterfaceFlow fromFlow = null, IDictionary<string, string> parameters = null)
        {
            CurrentStep = null;
            PreviousFlow = fromFlow;
            IsActive = true;
        }

        /// <summary>
        /// Called when control is returned to this flow from another flow, e.g. when the user
        /// goes back from the initial point of another flow
        /// </summary>
        /// <param name="fromFlow">The flow that will become the new current flow after navigation</param>
        /// <param name="parameters">Any parameters passed to this flow from the navigation</param>
        public virtual void ReturnTo(IUserInterfaceFlow fromFlow = null, IDictionary<string, string> parameters = null)
        {
            CurrentStep = null;
            IsActive = true;
        }

        /// <summary>
        /// Called when the user backs out of this flow by pressing back on any step that is an entry point
        /// </summary>
        /// <param name="toFlow">The flow that will become the new current flow after navigation</param>
        public virtual void Cancel(IUserInterfaceFlow toFlow = null)
        {
            IsActive = false;
            OnBack(null);
            CurrentStep = null;
        }

        /// <summary>
        /// Called when the user navigates away from this flow to another flow. This can be done by returning a
        /// <see cref="Navigator"/> that sets another flow in the <see cref="FlowStep{TViewModel}.OnBack" /> method of the <see cref="FlowStep"/>
        /// or the <see cref="FlowStep{TViewModel}.OnForward"/> 
        /// </summary>
        /// <param name="toFlow">The flow that will become the new current flow after navigation</param>
        public virtual void Exit(IUserInterfaceFlow toFlow = null)
        {
            IsActive = false;
            OnGoto(null);
            CurrentStep = null;
        }

        /// <summary>
        /// Performs navigation to the next logical step in the <see cref="Steps"/> list. The method
        /// evaluates <see cref="FlowStep.Next"/> for the current step and tries to determine what the next flowstep is
        /// </summary>
        public virtual void Next()
        {
            // Figure out next step
            FlowStep nextStep = null;
            if (CurrentStep == null)
            {
                nextStep = GetInitalFlowStep();
            }
            else if (CurrentStep.IsFinal)
            {
                // Taking a 'next' step from a final step is not possible. Let's just stop here.
                return;
            }
            else if (CurrentStep.Next.IsAutomatic)
            {
                nextStep = Steps.NextItem(CurrentStep);
            }
            else if (CurrentStep.Next.StepTo != null)
            {
                nextStep = CurrentStep.Next.StepTo();
            }
            if (nextStep == null)
                throw UserInterfaceFlowException.NextStepNotFound();

            NavigateToStep(nextStep);
        }

        /// <summary>
        /// Called after the Manager navigates to a step in this flow
        /// </summary>
        /// <param name="stepName">The name of the step that has been navigated to</param>
        /// <param name="parameters">Any parameters passed to this step from the navigation</param>
        public virtual void OnGoto(string stepName, IDictionary<string, string> parameters = null)
        {
            // Unhook last page
            if (CurrentStep != null)
            {
                ExitStep(CurrentStep);
            }
            
            // Move to the next step
            if (IsActive)
            {
                // Hook new page
                var step = Steps.GetStepForName(stepName);

                // Create a Temporary step so that we can return to the flow when going back again
                if (step == null)
                {
                    var backStep = CurrentStep;
                    step = new TemporaryFlowStep { Back = FlowStepRoute.To(() => backStep) };
                }

                EnterStep(step);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stepName">The name of the step that has been navigated to</param>
        /// <param name="parameters">Any parameters passed to this step from the navigation</param>
        public virtual void OnBack(string stepName, IDictionary<string, string> parameters = null)
        {
            // Unhook last page
            if (CurrentStep != null)
            {
                CancelStep(CurrentStep);
            }

            // Return to the back step
            if (IsActive)
            {
                ReturnToStep(Steps.GetStepForName(stepName));
            }
        }

        /// <summary>
        /// Called whenever a<see cref="Failure"/> message is published.
        /// The subscription to that message can be suppressed by setting
        /// <see cref="SuppressGeneralErrorHandling"/> to false.
        /// </summary>
        /// <param name="message">The Failure message with details. The message 
        /// can be a sub type of Failuer and contain additional details</param>
        protected virtual void OnFailure(Failure message)
        {
            NavigateToStep(CurrentStep, notificationKey: "Failure", message: message.Message);
        }

        /// <summary>
        /// Called whenever a<see cref="CommunicationErrorOccured"/> message is published.
        /// The subscription to that message can be suppressed by setting
        /// <see cref="SuppressGeneralErrorHandling"/> to false.
        /// </summary>
        /// <param name="message">The CommunicationErrorOccured message with details. The message 
        /// can be a sub type of CommunicationErrorOccured and contain additional details</param>
        protected virtual void OnCommunicationError(CommunicationErrorOccured message)
        {
            NavigateToStep(CurrentStep, notificationKey: "CommunicationProblem", message: message.Message, title: message.Title);
        }

        /// <summary>
        /// Called whenever a<see cref="CommunicationErrorOccured"/> message is published.
        /// The subscription to that message can be suppressed by setting
        /// <see cref="SuppressGeneralErrorHandling"/> to false.
        /// </summary>
        /// <param name="message">The CommunicationErrorOccured message with details. The message 
        /// can be a sub type of CommunicationErrorOccured and contain additional details</param>
        protected virtual void OnNetworkError(NetworkErrorOccured message)
        {
            NavigateToStep(CurrentStep, notificationKey: "NetworkProblem", message: message.Message, title: message.Title);
        }

        /// <summary>
        /// Called by a command in a viewmodel and results in the onDo for the current
        /// step is invoked with the command as argument
        /// </summary>
        /// <param name="command">The calling command</param>
        /// <param name="parameter">The command parameter (optional)</param>
        public virtual void Do(ICommand command, object parameter)
        {
            var step = CurrentStep;
            var viewModel = CurrentViewModel<ViewModelBase>();
            viewModel.Flow = this;
            ExecuteOnDo(step, viewModel, command, parameter);
        }

        /// <summary>
        /// Serializes the state of the Flow and returns it as a json formated
        /// string
        /// </summary>
        /// <returns></returns>
        public string Serialize()
        {
            var currentState = GetCurrentState();
            var serializedObject = Newtonsoft.Json.JsonConvert.SerializeObject(currentState);
            return serializedObject;
        }

        /// <summary>
        /// Builds up the state of the Flow from a serialized json formated string
        /// </summary>
        /// <param name="serializedState"></param>
        public void Deserialize(string serializedState)
        {
            var stateObject = Newtonsoft.Json.JsonConvert.DeserializeObject<TStateObject>(serializedState);
            SetCurrentState(stateObject);
        }

        /// <summary>
        /// Call this in any of a step's onXXX methods or in other parts of the flow. This
        /// forces navigation to a new step within this flow
        /// </summary>
        /// <param name="step">The FlowStep to navigate to</param>
        /// <param name="forward">Tru if navigating to a new view, false to navigate backwards in the backstack</param>
        /// <param name="notificationKey">Null of the key of a notification view to display</param>
        /// <param name="title">The title to display in a notification view</param>
        /// <param name="message">The message to display in a notification view</param>
        /// <param name="script">The name of a javascript function to call on the page after navigation</param>
        protected virtual void NavigateToStep(FlowStep step, bool forward = true, string notificationKey = null, string title = null, string message = null, string script = null)
        {
            var viewModel = CurrentViewModel<ViewModelBase>();
            if (viewModel == null) return;

            viewModel.NotificationTitle = title;
            viewModel.NotificationMessage = message;

            var stepName = step.GetStepName();
            // This ensures the initial step in the uri is the same as 
            // just pointing to the flow without specifying the step
            if (stepName == GetInitalStep()) stepName = null;
            Dictionary<string, string> parameters = null;
            if (script != null)
            {
                // TOD: Clean up this (move 'script' to Manager)
                parameters = new Dictionary<string, string>();
                parameters.Add("script", script);
            }

            Manager.Navigate(this.GetType(), stepName, forward, notificationKey, parameters);
        }

        /// <summary>
        /// Returns the type of the ViewModel associated with the step
        /// </summary>
        /// <param name="stepName"></param>
        /// <returns></returns>
        public Type GetViewModelForStepName(string stepName)
        {
            return Steps.GetStepForName(stepName).ViewModelType;
        }
        
        protected virtual void ExitStep(FlowStep step)
        {
#if DEBUG
            Debug.WriteLine("UIFLOW: {0} Exit step {1}", this.TypeName(), DebugStepName(step));
#endif
            if (step == null) return;
            var viewModel = CurrentViewModel<ViewModelBase>();
            if (viewModel == null) return;

            viewModel.SetActive(false);
            viewModel.Flow = null;
            ExecuteOnExit(step, viewModel);
        }

        protected virtual void EnterStep(FlowStep step)
        {
#if DEBUG
            Debug.WriteLine("UIFLOW: {0} Enter step {1}", this.TypeName(), DebugStepName(step));
#endif
            if (step == null)
            {
                CurrentStep = null;
                return;
            }
            CurrentStep = step;
            var viewModel = CurrentViewModel<ViewModelBase>();
            if( viewModel == null) return;

            viewModel.Flow = this;
            viewModel.SetActive(IsActive);
            ExecuteOnEnter(step, viewModel);
            viewModel.ClearValidation();
        }

        protected virtual void CancelStep(FlowStep step)
        {
#if DEBUG
            Debug.WriteLine("UIFLOW: {0} Cancel step {1}", this.TypeName(), DebugStepName(step));
#endif
            if (step == null) return;
            var viewModel = CurrentViewModel<ViewModelBase>();
            if( viewModel == null) return;

            viewModel.SetActive(false);
            ExecuteOnCancel(step, viewModel);
            viewModel.Flow = null;
        }

        protected virtual void ReturnToStep(FlowStep step)
        {
#if DEBUG
            Debug.WriteLine("UIFLOW: {0} Return to step {1}", this.TypeName(), DebugStepName(step));
#endif
            if (step == null) return;            
            CurrentStep = step;
            var viewModel = CurrentViewModel<ViewModelBase>();
            if (viewModel == null) return;

            viewModel.Flow = this;
            viewModel.SetActive(IsActive);
            ExecuteOnReturn(step, viewModel);
        }

        protected void IsBusy(bool isBusy, string message = null)
        {
            var currentViewModel = CurrentViewModel<ViewModelBase>();
            if (currentViewModel == null) return;

            currentViewModel.SetBusy(isBusy, message);
        }

#if DEBUG

        private string DebugStepName(FlowStep step)
        {
            return step == null
                ? "???"
                : (step is TemporaryFlowStep ? "Temporary Step" : BuildRelativeNamespace(step.ViewModelType));
        }

        private static string BuildRelativeNamespace(Type type)
        {
            if (type == null) return null;
            var assemblyNamespace = type.GetTypeInfo().Assembly.GetName().Name;
            if (assemblyNamespace.EndsWith(".WP8"))
                assemblyNamespace = assemblyNamespace.Remove(assemblyNamespace.Length - 4, 4);
            var navigateToViewNamespace = type.Namespace;
            if (navigateToViewNamespace == null) return string.Format("{0}", type.Name);
            var relativeNamespace = navigateToViewNamespace.Substring(assemblyNamespace.Length);
            var path = string.Format("{0}.{1}", relativeNamespace, type.Name);

            return path;
        }

#endif

        private void ExecuteActionOnViewModel(FlowStep step, string methodName, object[] parameters)
        {
            var flowStepGenericType = typeof(FlowStep<ViewModelBase>).GetGenericTypeDefinition().MakeGenericType(step.ViewModelType);

            var onEnterProperty = flowStepGenericType.GetTypeInfo().GetDeclaredProperty(methodName);

            var onEnterAction = onEnterProperty.GetValue(step, null);
            if (onEnterAction == null) return;

            var onEnterActionType = onEnterAction.GetType();
            var invokeMethod = onEnterActionType.GetTypeInfo().GetDeclaredMethods("Invoke").Single();

            invokeMethod.Invoke(onEnterAction, parameters);
        }

        protected virtual void ExecuteOnEnter(FlowStep step, ViewModelBase viewModel)
        {
            ExecuteActionOnViewModel(step, "OnEnter", new object[] { viewModel });
        }

        protected virtual void ExecuteOnExit(FlowStep step, ViewModelBase viewModel)
        {
            ExecuteActionOnViewModel(step, "OnExit", new object[] { viewModel });
        }

        protected virtual void ExecuteOnCancel(FlowStep step, ViewModelBase viewModel)
        {
            ExecuteActionOnViewModel(step, "OnCancel", new object[] { viewModel });
        }

        protected virtual void ExecuteOnReturn(FlowStep step, ViewModelBase viewModel)
        {
            ExecuteActionOnViewModel(step, "OnReturn", new object[] { viewModel });
        }

        protected virtual void ExecuteOnDo(FlowStep step, ViewModelBase viewModel, ICommand command, object parameter)
        {
            ExecuteActionOnViewModel(step, "OnDo", new[] { viewModel, command, parameter });
        }
    }

    public static class FlowExtensions
    {
        public static bool IsStartupPoint(this IUserInterfaceFlow flow)
        {
            if (flow == null) return false;
            return flow.GetType().HasAttribute<FlowAttribute>(a => a.IsStartupPoint);
        }

        public static bool IsDefault(this IUserInterfaceFlow flow)
        {
            if (flow == null) return false;
            return flow.GetType().HasAttribute<FlowAttribute>(a => a.IsDefault);
        }

        public static string GetStepName(this FlowStep flowStep)
        {
            var viewModelNamespace = flowStep.ViewModelType.Namespace;
            var viewModelSubfolderName = viewModelNamespace.Substring(
                flowStep.ViewModelType.Namespace.LastIndexOf(".",System.StringComparison.Ordinal) + 1);
            return viewModelSubfolderName;
        }

        public static FlowStep GetStepForName(this IEnumerable<FlowStep> steps, string name)
        {
            var stepWithName = steps.FirstOrDefault(step => step.ViewModelType.Namespace.EndsWith(name));
            if( stepWithName == null)
                throw UserInterfaceFlowException.StepNotFound(name);
            return stepWithName;
        }

        public static FlowStep StepWithViewModel<TViewModel>(this IEnumerable<FlowStep> steps)
            where TViewModel : ViewModelBase
        {
            return steps.StepWithViewModel(typeof (TViewModel));
        }

        public static FlowStep StepWithViewModel(this IEnumerable<FlowStep> steps, Type viewModelType)
        {
            return steps.FirstOrDefault(step => step.ViewModelType == viewModelType);
        }
    }
}