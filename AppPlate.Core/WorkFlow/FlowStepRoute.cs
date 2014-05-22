using System;

namespace AppPlate.Core.WorkFlow
{
    public class FlowStepRoute
    {
        public bool CancelFlow { get; private set; }
        public Func<FlowStep> StepTo { get; private set; }
        public bool IsAutomatic { get; private set; }

        private FlowStepRoute()
        {
            IsAutomatic = false;
            CancelFlow = false;
            StepTo = null;
        }

        public static FlowStepRoute Automatic
        {
            get { return new FlowStepRoute { IsAutomatic = true }; }
        }

        public static FlowStepRoute To(Func<FlowStep> flowStep)
        {
            return new FlowStepRoute { StepTo = flowStep};
        }

        public static FlowStepRoute CancelCurrentFlow()
        {
            return new FlowStepRoute { CancelFlow = true };
        }
    }
}