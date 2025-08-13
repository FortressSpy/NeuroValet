using GameViews.Story;
using NeuroSdk.Actions;
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

        public void ExecuteAction(INeuroAction action)
        {
            throw new NotImplementedException();
        }

        public List<INeuroAction> GetPossibleActions()
        {
            throw new NotImplementedException();
        }

        public bool IsStoryVisible()
        {
            if (_storyView == null)
            {
                // Attempt to reinitialize the StoryView if it is null
                _storyView = (StoryView)GameViews.Static.storyView;
                return false; // no data yet, maybe next time
            }
            else
            {
                return _storyView.isVisible; // Return the visibility status of the story view
            }
        }

        public string GetStoryName()
        {
            if (_storyView == null)
            {
                // Attempt to reinitialize the StoryView if it is null
                _storyView = (StoryView)GameViews.Static.storyView;
                return ""; // no data yet, maybe next time
            }
            else if (_storyView.isVisible)
            {
                return _storyView.story.name ?? ""; // Return the story name if available
            }
            else
            {
                return ""; // no story data for now
            }
        }

        public string GetStoryText()
        {
            if (_storyView == null)
            {
                // Attempt to reinitialize the StoryView if it is null
                _storyView = (StoryView)GameViews.Static.storyView;
                return ""; // no data yet, maybe next time
            }
            else if (_storyView.isVisible)
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
            if (_storyView == null)
            {
                // Attempt to reinitialize the StoryView if it is null
                _storyView = (StoryView)GameViews.Static.storyView;
                return null; // no data yet, maybe next time
            }
            else if (_storyView.isVisible)
            {
                StoryViewContents contents = (StoryViewContents)storyViewContents.GetValue(_storyView);
                IList<StoryChoiceElement> choices = (IList<StoryChoiceElement>)storyChoices.GetValue(contents);
                List<ChoiceData> choicesResult = new List<ChoiceData>();

                foreach (var choice in choices)
                {
                    choicesResult.Add(new ChoiceData { ChoiceIndex = choice.choiceIndex, ChoiceText = choice.sentenceElement.text });
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
