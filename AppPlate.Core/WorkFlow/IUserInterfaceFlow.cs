using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace AppPlate.Core.WorkFlow
{
    public interface IUserInterfaceFlow
    {
        string GetInitalStep();
        string GetCurrentStep();

        bool IsActive { get; set; }

        Type GetViewModelForStepName(string stepName);

        void Enter(IUserInterfaceFlow fromFlow = null, IDictionary<string, string> parameters = null);
        void Exit(IUserInterfaceFlow toFlow = null);
        void ReturnTo(IUserInterfaceFlow fromFlow = null, IDictionary<string, string> parameters = null);
        void Cancel(IUserInterfaceFlow toFlow = null);

        void Next();

        void OnBack(string stepName, IDictionary<string, string> parameters = null);
        void OnGoto(string stepName, IDictionary<string, string> parameters = null);

        void BeforeBack(string stepName);
        void BeforeGoto(string stepName);

        void Do(ICommand command, object parameter = null);

        string Serialize();
        void Deserialize(string serializedState);
    }

    public enum FlowNavigationAction
    {
        InitialStepNotFound,
        NextStepNotFound,
        BackStepNotFound,
        BackToPreviousFlow,
        BackToPreviousStep,
        GoToNextFlow,
        GoToInitialStep,
        GoToNextStep,
        EndOfFlow,
        NavigationOutOfSync,
    }
}