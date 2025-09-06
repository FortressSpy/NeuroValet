using GameViews.BottomNav;
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
    internal class NegotiateScheduleAction : NeuroSdk.Actions.NeuroAction
    {
        public override string Name => actionName;

        protected override string Description => description;

        protected override JsonSchema Schema => new()
        {
        };

        private readonly string actionName;
        private readonly string description;
        private readonly FoggSpeechBubbleButtonView buttonView;

        public NegotiateScheduleAction(int bribeState, FoggSpeechBubbleButtonView buttonView, int index)
        {
            this.buttonView = buttonView;
            actionName = $"negotiate_{bribeState}_{index}";

            switch (bribeState)
            {
                case 1:
                    description = "Negotiate an earlier departure date for this journey";
                    break;
                case 2:
                    description = "Your items are providing a bonus to the negotiation. Keep negotiating";
                    break;
                case 3:
                    description = $"Set next departure {(buttonView as FoggSpeechBubbleBribeButtonView)?.departureText.text} for {buttonView.text.text}";
                    break;
                case 0:
                default:
                    description = "(this action is likely invalid, please ignore it)";
                    break;
            }
        }

        protected override void Execute()
        {
            buttonView.OnClickButton();
        }

        protected override ExecutionResult Validate(ActionJData actionData)
        {
            if (GlobeViewParser.Instance.IsViewRelevant() && buttonView != null && buttonView.isActiveAndEnabled)
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
