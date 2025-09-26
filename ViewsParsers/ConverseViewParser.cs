using BepInEx.Logging;
using GameViews.Converse;
using NeuroSdk.Actions;
using NeuroValet.Actions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using static NeuroValet.ActionManager;

namespace NeuroValet.ViewsParsers
{
    internal class ConverseViewParser : IViewParser
    {
        // Implement a singleton pattern for the ConverseViewParser
        private static readonly Lazy<ConverseViewParser> _instance = new Lazy<ConverseViewParser>(() => new ConverseViewParser());
        public static ConverseViewParser Instance => _instance.Value;

        FieldInfo optionsListField = typeof(ConverseOptionsView)
            .GetField("options", BindingFlags.NonPublic | BindingFlags.Instance);
        FieldInfo conversationField = typeof(ConversationView)
            .GetField("conversation", BindingFlags.NonPublic | BindingFlags.Instance);

        private ConversationView _conversationView;

        private ConverseViewParser()
        {
            _conversationView = (ConversationView)GameViews.Static.converseView;
        }

        public PossibleActions GetPossibleActions(ManualLogSource logger)
        {
            StringBuilder context = new StringBuilder();
            PossibleActions possibleActions = new PossibleActions();
            possibleActions.Actions = new List<INeuroAction>();

            var conversation = (Conversation)conversationField.GetValue(_conversationView);
            context.AppendLine($"You are in a conversation with {conversation._character.address}.");
            context.AppendLine($"Your Previous Message: {conversation.playerDialog}");
            context.AppendLine($"{conversation._character.address} Response: {conversation.characterDialog}");

            context.AppendLine($"{_conversationView.optionsView.optionsHeader.text}");
            context.AppendLine($"{_conversationView.optionsView.infoText.text}");

            // Find all possible conversation options (=buttons). Each will be one of the following types of options
            // - Ask about a possible destination city (to get further questions about routes from there)
            // - Ask about route from X to Y
            // - Use item to keep conversation going and allow more questions
            // - End conversation
            var options = (List<ConversationOptionButton>)optionsListField.GetValue(_conversationView.optionsView);
            foreach(var option in options)
            {
                possibleActions.Actions.Add(new ConverseAction(option));
            }

            possibleActions.Context = context.ToString();
            possibleActions.IsContextSilent = false;
            return possibleActions;
        }

        // Is there a story going on right now?
        public bool IsViewRelevant()
        {
            if (_conversationView == null)
            {
                // Attempt to reinitialize the StoryView if it is null
                _conversationView = (ConversationView)GameViews.Static.converseView;
                return _conversationView != null && _conversationView.isVisible;
            }
            else
            {
                return _conversationView.isVisible; // Return the visibility status of the conversation view
            }
        }
    }
}
