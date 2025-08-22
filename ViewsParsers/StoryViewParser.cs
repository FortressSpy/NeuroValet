using BepInEx.Logging;
using GameViews.Story;
using NeuroSdk.Actions;
using NeuroValet.Actions;
using System;
using System.Collections.Generic;
using System.Reflection;
using static NeuroValet.StateData.StoryData;

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

        public List<INeuroAction> GetPossibleActions(ManualLogSource logger)
        {
            List<INeuroAction> actions = new List<INeuroAction>();
            if (IsStoryVisible())
            {
                var choices = GetAvailableChoices();

                foreach (var choice in choices)
                {
                    actions.Add(new StoryAction(choice, logger));
                }
            }
            return actions;
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
            // Check if there is a 
            if (IsStoryVisible())
            {
                StoryViewContents contents = (StoryViewContents)storyViewContents.GetValue(_storyView);
                IList<StoryChoiceElement> choices = (IList<StoryChoiceElement>)storyChoices.GetValue(contents);

                // Is possible to select this choice?
                if (choices.Count > 0 && choices.Count > actionIndex)
                {
                    return true;
                }
                // Is 'continue' choice?
                else if (choices.Count == 0 && actionIndex == 1)
                {
                    return true;
                }
                return choices != null && actionIndex >= 0 && actionIndex < choices.Count;
            }

            return false; // no story data for now
        }

        public bool IsStoryVisible()
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

        public string GetStoryName()
        {
            if (IsStoryVisible())
            {
                return _storyView.story?.name ?? ""; // Return the story name if available
            }
            else
            {
                return ""; // no story data for now
            }
        }

        public string GetStoryText()
        {
            if (IsStoryVisible())
            {
                StoryViewContents contents = (StoryViewContents)storyViewContents.GetValue(_storyView);
                return contents?.currentFlowText ?? "";
            }
            else
            {
                return ""; // no story data for now
            }
        }

        public List<ChoiceData> GetAvailableChoices()
        {
            if (IsStoryVisible())
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
            else
            {
                return null; // no story data for now
            }
        }
    }
}
