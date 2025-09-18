using NeuroSdk;
using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;
using NeuroValet.ViewsParsers;

namespace NeuroValet.Actions
{
    internal class LuggageCloseWindowAction : NeuroSdk.Actions.NeuroAction
    {
        public override string Name => "close_luggage_window";

        protected override string Description => $"Close the luggage window";

        protected override JsonSchema Schema => new()
        {
        };

        public LuggageCloseWindowAction()
        {
        }

        protected override void Execute()
        {
            MarketAndLuggageViewParser.Instance.CloseMarketAndLuggageView();
        }

        protected override ExecutionResult Validate(ActionJData actionData)
        {
            if (MarketAndLuggageViewParser.Instance.IsViewRelevant())
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
