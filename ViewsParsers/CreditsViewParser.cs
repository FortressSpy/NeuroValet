using BepInEx.Logging;
using GameViews.Converse;
using GameViews.Credits;
using NeuroSdk.Actions;
using NeuroValet.Actions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using static NeuroValet.ActionManager;

namespace NeuroValet.ViewsParsers
{
    internal class CreditsViewParser : IViewParser
    {
        // Implement a singleton pattern for the ConverseViewParser
        private static readonly Lazy<CreditsViewParser> _instance = new Lazy<CreditsViewParser>(() => new CreditsViewParser());
        public static CreditsViewParser Instance => _instance.Value;

        private CreditsView _creditsView;

        private CreditsViewParser()
        {
            _creditsView = (CreditsView)GameViews.Static.creditsView;
        }

        public PossibleActions GetPossibleActions(ManualLogSource logger)
        {
            PossibleActions possibleActions = new PossibleActions();
            possibleActions.Actions = new List<INeuroAction>();

            possibleActions.Actions.Add(new CreditsCloseAction());

            possibleActions.Context = "You are viewing the credits";
            possibleActions.IsContextSilent = true;
            return possibleActions;
        }

        public void CloseTheCredits()
        {
            _creditsView.ClickCloseButton();
        }

        // Is there a story going on right now?
        public bool IsViewRelevant()
        {
            if (_creditsView == null)
            {
                // Attempt to reinitialize the StoryView if it is null
                _creditsView = (CreditsView)GameViews.Static.converseView;
                return _creditsView != null && _creditsView.isVisible;
            }
            else
            {
                return _creditsView.isVisible; // Return the visibility status of the credits view
            }
        }
    }
}
