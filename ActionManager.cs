using BepInEx.Logging;
using GameViews;
using NeuroSdk.Actions;
using NeuroValet.StateData;
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

        public ActionManager(ManualLogSource logger)
        {
            this.logger = logger;
        }

        public PossibleActions GetPossibleActions(GameStateData stateData) 
        {
            PossibleActions possibleActions = new PossibleActions();

            // Go over the various View Parsers in order of priority to get the possible actions
            // Note some views are mutually exclusive, while others can be active at the same time, allowing multiple actions to be available
            // TODO - need to figure out how to handle this priority. Maybe the views themselves?
            if (StoryViewParser.Instance.IsStoryVisible())
            {
                var actions = StoryViewParser.Instance.GetPossibleActions(stateData, logger);
                possibleActions.Actions = actions;
                possibleActions.Context = StoryViewParser.Instance.GetStoryText() + " (You have to choose how to respond this)"; // TODO - will this part be silent? does Neuro even need this prompt?
                possibleActions.IsContextSilent = false;
            }
            // TODO - globe view should have two actions - focus on player. Look back at the globe. 
            // TODO - maybe even more - looking at globe allows looking at possible journeys
            else if (GlobeViewParser.Instance.CanFocusOnPosition())
            {
                var actions = GlobeViewParser.Instance.GetPossibleActions(stateData, logger);
                possibleActions.Actions = actions;
                possibleActions.Context = "You are looking at the globe. You can choose to focus on your current location to do more actions";
                possibleActions.IsContextSilent = false;
            }
            else
            {
                // No known view is active, so no actions available
                possibleActions.Context = "No actions are currently available.";
                possibleActions.IsContextSilent = true;
            }

            return possibleActions;
        }
    }
}
