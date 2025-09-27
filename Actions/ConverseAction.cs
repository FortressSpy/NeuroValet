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

        private static int actionNumber = 1;

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
                else if (topicButton.conversationTopic.categoryType == Game.Conversation.ConversationTopic.CategoryType.City)
                {
                    name = $"converse_topic_{topicButton.conversationTopic.city.displayName.ToLower()}";
                    description = topicButton.infoText;
                }
                else if (topicButton.conversationTopic.categoryType == Game.Conversation.ConversationTopic.CategoryType.NPCResponse)
                {
                    // usually some generic question by the NPC with a YES/NO style response, so will have multiple buttons and need to differentiate them
                    // so just using a generic counter for that
                    // TODO - this causes a minor bug - because everytime we call this it will increment the counter, and we query for actions every second~
                    // TODO - so this means every second we will say we have no actions (even though we don't)?
                    name = $"converse_topic_response_{actionNumber++}";
                    description = topicButton.text.text;
                }
                else
                {
                    // Just in case other category type conversations do happen and have buttons related to them (maybe FoggConversation does, idk)
                    name = $"converse_topic_{actionNumber++}";
                    description = topicButton.text.text;
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
