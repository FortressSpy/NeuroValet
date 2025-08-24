using NeuroSdk;
using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;
using NeuroValet.ViewsParsers;

namespace NeuroValet.Actions
{
    internal class EnterCityAction : NeuroSdk.Actions.NeuroAction
    {
        public override string Name => "focus_on_city";

        protected override string Description => "Focus on the city you are in at the moment and check available actions there. Current city is " + _position;

        private readonly string _position;

        protected override JsonSchema Schema => new()
        {
        };

        public EnterCityAction(string positionName)
        {
            _position = positionName;
        }

        protected override void Execute()
        {
            GlobeViewParser.Instance.FocusOnPlayer();
        }

        protected override ExecutionResult Validate(ActionJData actionData)
        {
            if (GlobeViewParser.Instance.IsViewRelevant())
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
