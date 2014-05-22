using System;

namespace AppPlate.Core.Concurrency
{
    /// <summary>
    /// Dummy dispatcher for use in DesignTime ViewModels
    /// </summary>
    public class DesignTimeDispatcher : IDispatcher
    {
        public void DispatchAction(Action action)
        {
            action();
        }

#if DEBUG
        public void AssertUIThread()
        {
           
        }
#endif

    }
}