using GameViews.Converse;
using NeuroSdk;
using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;
using NeuroValet.ViewsParsers;

namespace NeuroValet.Actions
{
    internal class ConverseAction : NeuroSdk.Actions.NeuroAction
    {
        private readonly string name;
        private readonly string description;
        public override string Name => name;

        protected override string Description => description;

        ConversationOptionButton _button;

        protected override JsonSchema Schema => new()
        {
        };

        public ConverseAction(ConversationOptionButton button)
        {
            _button = button;
            if (button is LinkConversationOptionButton)
            {
                var linkButton = button as LinkConversationOptionButton;
                name = $"converse_link_{linkButton.textTop.text.ToLower()}_{linkButton.textBottom.text.ToLower()}";
                description = $"Query about possible route from {linkButton.textTop.text} to {linkButton.textBottom.text}";
            }
            else if (button is ItemConversationOptionButton)
            {
                var itemButton = button as ItemConversationOptionButton;
                name = $"converse_item_{itemButton.item.displayName.ToLower()}";
                description = $"Use {itemButton.item.displayName} to keep the conversation going (allow asking more questions)";
            }
            else if (button is TopicStarterConversationOptionButton)
            {
                var topicButton = button as TopicStarterConversationOptionButton;
                if (topicButton.conversationTopic.categoryType == Game.Conversation.ConversationTopic.CategoryType.Goodbye)
                {
                    name = $"converse_close_topic";
                    description = $"Close the conversation";
                }
                else
                {
                    name = $"converse_topic_{topicButton.conversationTopic.city.displayName.ToLower()}";
                    description = topicButton.infoText;
                }
            }
            else
            {
                // Note only one close button is expected, so no need to differentiate names
                name = $"converse_close";
                description = $"Close the conversation";
            }
        }

        protected override void Execute()
        {
            _button?.Click();
        }

        protected override ExecutionResult Validate(ActionJData actionData)
        {
            if (ConverseViewParser.Instance.IsViewRelevant())
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
