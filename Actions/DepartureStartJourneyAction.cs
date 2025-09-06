using NeuroSdk;
using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;
using NeuroValet.StateData;
using NeuroValet.ViewsParsers;

namespace NeuroValet.Actions
{
    internal class DepartureStartJourneyAction : NeuroSdk.Actions.NeuroAction
    {
        public override string Name => "depart_on_journey";

        protected override string Description => $"Start trip to {_journey.DestinationCity.displayName}, {_journey.ArrivalTime}, Will cost {_journey.Cost}, and will do {_healthCost} health damage";

        private readonly Journey _journey;
        private readonly int _healthCost;

        protected override JsonSchema Schema => new()
        {
        };

        public DepartureStartJourneyAction(Journey journey, int healthCost)
        {
            _journey = journey;
            _healthCost = healthCost;
        }

        protected override void Execute()
        {
            DepartureViewParser.Instance.ClickMainButton();
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
