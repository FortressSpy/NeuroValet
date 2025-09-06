using GameResources.MapData;
using NeuroSdk;
using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;
using NeuroValet.StateData;
using NeuroValet.ViewsParsers;

namespace NeuroValet.Actions
{
    internal class DepartureBuyStorageAction : NeuroSdk.Actions.NeuroAction
    {
        public override string Name => "buy_extra_storage_on_trip";

        protected override string Description => $"Pay extra to allow carrying more suitcases for this trip: {_offer}";

        protected override JsonSchema Schema => new()
        {
        };

        private readonly string _offer;

        public DepartureBuyStorageAction(string offer)
        {
            _offer = offer;
        }

        protected override void Execute()
        {
            DepartureViewParser.Instance.PayForExtraStorage();
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
