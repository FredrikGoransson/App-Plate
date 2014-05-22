using System;

namespace AppPlate.Core.Concurrency
{
    /// <summary>
    /// Manages dispatching of actions to the UI thread
    /// </summary>
    public interface IDispatcher
    {
        /// <summary>
        /// Dispatches the action to the UI thread
        /// </summary>
        /// <param name="action"></param>
        void DispatchAction(Action action);

#if DEBUG
        void AssertUIThread();
#endif

    }
}
