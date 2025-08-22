using NeuroSdk;
using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;
using NeuroValet.ViewsParsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroValet.Actions
{
    internal class EnterCityAction : NeuroSdk.Actions.NeuroAction
    {
        public override string Name => "focus_on_city";

        protected override string Description => "Focus on the current player position and get the actions there. Current position is " + _position;

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
            GlobeViewParser.Instance.ExecuteAction();
        }

        protected override ExecutionResult Validate(ActionJData actionData)
        {
            if (GlobeViewParser.Instance.CanFocusOnPosition())
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
