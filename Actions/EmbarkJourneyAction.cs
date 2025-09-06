using NeuroSdk;
using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;
using NeuroValet.StateData;
using NeuroValet.ViewsParsers;

namespace NeuroValet.Actions
{
    internal class EmbarkJourneyAction : NeuroSdk.Actions.NeuroAction
    {
        public override string Name => "embark_journey_to_" + m_journey.DestinationCity.displayName.ToLower();

        protected override string Description => $"View departure window for journey to {m_journey.DestinationCity.displayName} that is departing soon";

        private readonly Journey m_journey;

        protected override JsonSchema Schema => new()
        {
        };

        public EmbarkJourneyAction(Journey journey)
        {
            m_journey = journey;
        }

        protected override void Execute()
        {
            GlobeViewParser.Instance.OpenDepartureWindow();
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
