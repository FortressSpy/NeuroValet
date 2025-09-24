using BepInEx.Logging;
using NeuroSdk;
using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;
using NeuroValet.ViewsParsers;
using System.Text.RegularExpressions;

namespace NeuroValet.Actions
{
    internal class StoryAction : NeuroAction
    {
        internal struct ChoiceData
        {
            public string ChoiceText { get; set; }
            public int ChoiceIndex { get; set; }
            public bool IsContinueChoice { get; set; }
        }

        private readonly ChoiceData _choiceData;

        private ManualLogSource logger;

        public StoryAction(ChoiceData choiceData, ManualLogSource logger)
        {
            _choiceData = choiceData;
            this.logger = logger;
        }

        public override string Name => "story_decision_" + _choiceData.ChoiceIndex;
        protected override string Description => Regex.Replace(_choiceData.ChoiceText, "<.*?>", string.Empty);

        protected override JsonSchema Schema => new()
        {
        };

        protected override void Execute()
        {
            StoryViewParser.Instance.ExecuteAction(_choiceData);
        }

        protected override ExecutionResult Validate(ActionJData actionData)
        {
            logger.Log(LogLevel.Info, $"Validating StoryAction with index: {_choiceData.ChoiceIndex}. [{_choiceData.ChoiceText}].");

            if (StoryViewParser.Instance.HasAction(_choiceData.ChoiceIndex))
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
