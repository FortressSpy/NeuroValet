using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;
using NeuroValet.ViewsParsers;

namespace NeuroValet.Actions
{
    internal class ClockSkipTimeAction : NeuroSdk.Actions.NeuroAction
    {
        public override string Name => "skip_time";

        protected override string Description => "Skip time by one hour";

        protected override JsonSchema Schema => new()
        {
        };

        public ClockSkipTimeAction()
        {
        }

        protected override void Execute()
        {
            CloudViewParser.Instance.SkipTime();
        }

        protected override ExecutionResult Validate(ActionJData actionData)
        {
            if (CloudViewParser.Instance.IsViewRelevant())
            {
                return ExecutionResult.Success();
            }
            else
            {
                return ExecutionResult.Failure("This action is no longer useable. " +
                    "Most likely that current time of day is now too late for this, or it was otherwise recently unregistered");
            }
        }
    }
}
