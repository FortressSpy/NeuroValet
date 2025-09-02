using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;

namespace NeuroValet.Actions
{
    internal class TravelAction : NeuroAction
    {
        public override string Name => _actionName;

        protected override string Description => _description;

        protected override JsonSchema Schema => new()
        {
        };

        private int _actionIndex;
        private GameViews.Cloud.Icon _icon;
        private string _description;
        private string _actionName;

        public TravelAction(int actionIndex, GameViews.Cloud.Icon icon, string description)
        {
            _actionIndex = actionIndex;
            _icon = icon;
            _description = description;

            // Have to save the action name separately, cause by the time action unregisters, _icon will be destroyed
            _actionName = $"travel_{_actionIndex}_{_icon.name.ToLower()}";
        }

        protected override void Execute()
        {
            _icon.OnClicked();
        }

        protected override ExecutionResult Validate(ActionJData actionData)
        {
            if (_icon.available && !_icon.ignoreClicks && _icon.fadeAlpha > 0f && _icon.iconData.clickEventName != null)
            {
                return ExecutionResult.Success();
            }
            else
            {
                return ExecutionResult.Failure("This action is no longer useable. " +
                    "Most likely that you've run out of time to do this, and/or it was recently unregistered");
            }
        }
    }
}
