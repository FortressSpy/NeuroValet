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
        }

        private readonly ManualLogSource logger;
        private readonly StoryViewParser storyView;

        public ActionManager(ManualLogSource logger)
        {
            this.logger = logger;
            this.storyView = StoryViewParser.Instance;
        }

        // TODO - this should get state data as a parameter.
        public PossibleActions GetPossibleActions() 
        {
            PossibleActions possibleActions = new PossibleActions();

            // Go over the various View Parsers in order of priority to get the possible actions
            if (storyView.IsStoryVisible())
            {
                var actions = storyView.GetPossibleActions(); 
                if (actions.Count > 0)
                {
                    possibleActions.Actions = actions;
                    possibleActions.Context = storyView.GetStoryText() + "(You have to choose a response to this)"; // TODO - do I need this prompt to explain to neuro that she is choosing a response?
                    possibleActions.IsContextSilent = false;
                }
            }
        }
        public void ExecuteAction(INeuroAction action)
        {
            throw new NotImplementedException();
        }
    }
}
