using BepInEx.Logging;
using GameViews.Story;
using NeuroSdk.Actions;
using NeuroValet.Actions;
using NeuroValet.StateData;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using static NeuroValet.ActionManager;
using static NeuroValet.Actions.StoryAction;

namespace NeuroValet.ViewsParsers
{
    internal class StoryViewParser : IViewParser
    {
        // Implement a singleton pattern for the StoryViewParser
        private static readonly Lazy<StoryViewParser> _instance = new Lazy<StoryViewParser>(() => new StoryViewParser());
        public static StoryViewParser Instance => _instance.Value;

        // Get the private field info
        FieldInfo storyViewContents = typeof(StoryView)
            .GetField("_contents", BindingFlags.NonPublic | BindingFlags.Instance);
        FieldInfo storyChoices = typeof(StoryViewContents)
            .GetField("_contentChoiceElements", BindingFlags.NonPublic | BindingFlags.Instance);

        private StoryView _storyView;

        private StoryViewParser()
        {
            _storyView = (StoryView)GameViews.Static.storyView;
        }

        public PossibleActions GetPossibleActions(ManualLogSource logger)
        {
            PossibleActions possibleActions = new PossibleActions();
            possibleActions.Actions = new List<INeuroAction>();
            if (IsViewRelevant()) // make sure the story view is still relevant, even though this was probably called before
            {
                var choices = GetAvailableChoices();

                foreach (var choice in choices)
                {
                    possibleActions.Actions.Add(new StoryAction(choice, logger));
                }
            }

            possibleActions.Context = StoryViewParser.Instance.GetStoryText() + " (You have to choose how to respond this)"; // TODO - will this part be silent? does Neuro even need this prompt?
            possibleActions.IsContextSilent = false;
            return possibleActions;
        }

        public void ExecuteAction(ChoiceData choice)
        {
            if (HasAction(choice.ChoiceIndex))
            {
                StoryViewContents contents = (StoryViewContents)storyViewContents.GetValue(_storyView);

                if (choice.IsContinueChoice)
                {
                    contents.OnContinueSelected();
                }
                else
                {
                    contents.OnChoiceSelected(choice.ChoiceIndex);
                }
            }
        }

        public bool HasAction(int actionIndex)
        {
            // Check if there is even a story going on right now
            if (IsViewRelevant())
            {
                StoryViewContents contents = (StoryViewContents)storyViewContents.GetValue(_storyView);
                IList<StoryChoiceElement> choices = (IList<StoryChoiceElement>)storyChoices.GetValue(contents);

                // Is it possible to select this choice?
                if (choices.Count > 0 && choices.Count > actionIndex)
                {
                    return true;
                }
                // Is this a 'continue' choice?
                else if (choices.Count == 0 && actionIndex == 1)
                {
                    return true;
                }
                return choices != null && actionIndex >= 0 && actionIndex < choices.Count;
            }

            return false; // no story data for now
        }

        // Is there a story going on right now?
        public bool IsViewRelevant()
        {
            if (_storyView == null)
            {
                // Attempt to reinitialize the StoryView if it is null
                _storyView = (StoryView)GameViews.Static.storyView;
                return _storyView != null && _storyView.isVisible;
            }
            else
            {
                return _storyView.isVisible; // Return the visibility status of the story view
            }
        }

        private string GetStoryName()
        {
            return _storyView.story?.name ?? ""; // Return the story name if available
        }

        private string GetStoryText()
        {
            StoryViewContents contents = (StoryViewContents)storyViewContents.GetValue(_storyView);
            string storyText = contents?.currentFlowText ?? "";
            return Regex.Replace(storyText, "<.*?>", string.Empty); // Remove any HTML tags (game uses it to format the text)
        }

        private List<ChoiceData> GetAvailableChoices()
        {
            StoryViewContents contents = (StoryViewContents)storyViewContents.GetValue(_storyView);
            IList<StoryChoiceElement> choices = (IList<StoryChoiceElement>)storyChoices.GetValue(contents);
            List<ChoiceData> choicesResult = new List<ChoiceData>();

            // If there are no choices but the story is still going, we can assume the "Continue" (=finish story?) choice is available
            if (choices.Count == 0)
            {
                // only continue element is available probabaly, add it as the single available choice
                choicesResult.Add(new ChoiceData { ChoiceIndex = 1, ChoiceText = "(Continue)", IsContinueChoice = true }); // TODO - will neuro say something? this should be silent...
            }
            else
            {
                foreach (var choice in choices)
                {
                    choicesResult.Add(new ChoiceData { ChoiceIndex = choice.choiceIndex, ChoiceText = choice.sentenceElement.text, IsContinueChoice = false });
                }
            }

            return choicesResult;
        }
    }
}
