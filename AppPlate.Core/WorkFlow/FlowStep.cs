using System;
using System.Windows.Input;

namespace AppPlate.Core.WorkFlow
{    
    public class FlowStep
    {
        private FlowStepRoute _next;
        private FlowStepRoute _back;
        public Type ViewModelType { get; set; }

        public bool IsInitial { get; set; }
        public bool IsFinal { get; set; }

        public FlowStepRoute Next
        {
            get { return _next ?? FlowStepRoute.Automatic; }
            set { _next = value; }
        }

        public FlowStepRoute Back
        {
            get { return _back ?? FlowStepRoute.Automatic; }
            set { _back = value; }
        }
    }

    public class TemporaryFlowStep : FlowStep
    {
        
    }

    public class FlowStep<TViewModel> : FlowStep
        where TViewModel : ViewModelBase
    {
        public Action<TViewModel> OnEnter { get; set; }
        public Action<TViewModel> OnExit { get; set; }
        public Action<TViewModel> OnReturn { get; set; }
        public Action<TViewModel> OnCancel { get; set; }
        public Action<TViewModel, ICommand, object> OnDo { get; set; }

        public FlowStep(
            FlowStepRoute next = null, 
            FlowStepRoute back = null, 
            bool isInitial = false, 
            bool isFinal = false, 
            Action<TViewModel> onEnter = null,
            Action<TViewModel> onExit = null,
            Action<TViewModel> onReturn = null,
            Action<TViewModel> onCancel = null,            
            Action<TViewModel, ICommand, object> onDo = null
            )
        {
            OnEnter = onEnter;
            OnExit = onExit;
            OnReturn = onReturn;
            OnCancel = onCancel;
            OnDo = onDo;

            Next = next ?? FlowStepRoute.Automatic;
            Back = back ?? FlowStepRoute.Automatic;
            IsInitial = isInitial;
            IsFinal = isFinal;
            ViewModelType = typeof(TViewModel);
        }
    }
}