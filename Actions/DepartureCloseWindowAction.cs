using NeuroSdk;
using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;
using NeuroValet.ViewsParsers;

namespace NeuroValet.Actions
{
    internal class DepartureCloseWindowAction : NeuroSdk.Actions.NeuroAction
    {
        public override string Name => "close_departure_window";

        protected override string Description => $"Close departure window and go back to viewing the available routes and current location";

        protected override JsonSchema Schema => new()
        {
        };

        public DepartureCloseWindowAction()
        {
        }

        protected override void Execute()
        {
            DepartureViewParser.Instance.LeaveDepartureMenu();
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
