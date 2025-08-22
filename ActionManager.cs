using BepInEx.Logging;
using GameViews;
using NeuroSdk.Actions;
using NeuroValet.ViewsParsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroValet
{
    /// <summary>
    /// This class is responsible for identifying possible actions in the game, and executing them.
    /// </summary>
    internal class ActionManager
    {
        internal struct PossibleActions
        {
            public List<INeuroAction> Actions;
            public string Context;
            public bool IsContextSilent;
            public bool IsForcedAction;

            public PossibleActions()
            {
                Actions = new List<INeuroAction>();
            }
        }

        private readonly ManualLogSource logger;
        private readonly StoryViewParser storyView;

        public ActionManager(ManualLogSource logger)
        {
            this.logger = logger;
            this.storyView = StoryViewParser.Instance;
        }

        public PossibleActions GetPossibleActions() 
        {
            PossibleActions possibleActions = new PossibleActions();

            // Go over the various View Parsers in order of priority to get the possible actions
            if (storyView.IsStoryVisible())
            {
                var actions = storyView.GetPossibleActions(logger);
                possibleActions.Actions = actions;
                possibleActions.Context = storyView.GetStoryText() + " (You have to choose how to respond this)"; // TODO - will this part be silent? does Neuro even need this prompt?
                possibleActions.IsContextSilent = false;
                possibleActions.IsForcedAction = false;
            }

            return possibleActions;
        }
    }
}
