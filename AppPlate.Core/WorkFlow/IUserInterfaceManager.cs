using System;
using System.Collections.Generic;

namespace AppPlate.Core.WorkFlow
{
    public interface IUserInterfaceManager
    {
        void Recover();

        void Start();
        void Stop();
        void Pause();
        void Unpause();

        void Navigate(Uri uri);
        void Back(Uri uri);
        void Navigate(Type flowType = null, string stepName = null, bool forward = true, string notificationKey = null, IDictionary<string, string> parameters = null);
        void Navigate<TFlow>(string stepName = null, bool forward = true, string notificationKey = null, IDictionary<string, string> parameters = null) where TFlow : IUserInterfaceFlow;
        void Exit();
    }
}