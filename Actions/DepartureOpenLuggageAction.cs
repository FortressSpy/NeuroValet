using NeuroSdk;
using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;
using NeuroValet.ViewsParsers;

namespace NeuroValet.Actions
{
    internal class DepartureOpenLuggageAction : NeuroSdk.Actions.NeuroAction
    {
        public override string Name => "prepare_luggage_for_departure";

        protected override string Description => $"View current luggage and perhaps throw away some of it so you'll need less suitcases";

        protected override JsonSchema Schema => new()
        {
        };

        public DepartureOpenLuggageAction()
        {
        }

        protected override void Execute()
        {
            DepartureViewParser.Instance.OpenLuggageWindow();
        }

        protected override ExecutionResult Validate(ActionJData actionData)
        {
            if (DepartureViewParser.Instance.IsViewRelevant())
            {
                return ExecutionResult.Success();
            }
            else
            {
                // This shouldn't happen. Maybe had some timed stuff and action hasn't unregistered in time before neuro decided to do it
                // Or user did some input outside neuro's control?
                return ExecutionResult.Failure(NeuroSdkStrings.ActionFailedUnregistered);
            }
        }
    }
}
