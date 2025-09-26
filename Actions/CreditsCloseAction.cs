using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;
using NeuroValet.ViewsParsers;

namespace NeuroValet.Actions
{
    internal class CreditsCloseAction : NeuroSdk.Actions.NeuroAction
    {
        public override string Name => "close_credits";

        protected override string Description => "Close the credits";

        protected override JsonSchema Schema => new()
        {
        };

        protected override void Execute()
        {
            CreditsViewParser.Instance.CloseTheCredits();
        }

        protected override ExecutionResult Validate(ActionJData actionData)
        {
            if (CreditsViewParser.Instance.IsViewRelevant())
            {
                return ExecutionResult.Success();
            }
            else
            {
                return ExecutionResult.Failure("This action is no longer useable. " +
                    "Most likely that the credits just finished rolling on their own, or it was otherwise recently unregistered");
            }
        }
    }
}