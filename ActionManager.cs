using BepInEx.Logging;
using NeuroSdk.Actions;
using NeuroValet.ViewsParsers;
using System.Collections.Generic;

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
        private List<IViewParser> viewParsers;

        public ActionManager(ManualLogSource logger)
        {
            this.logger = logger;

            // Order of view parsers is important, as the first valid one will be used.
            // So add parsers in order of priority
            viewParsers = new List<IViewParser>
            {
                CreditsViewParser.Instance,
                ConverseViewParser.Instance,
                StoryViewParser.Instance,
                MarketAndLuggageViewParser.Instance,
                DepartureViewParser.Instance,
                CloudViewParser.Instance,
                GlobeViewParser.Instance,
            };
        }

        public PossibleActions GetPossibleActions() 
        {
            PossibleActions actions = actions = new PossibleActions()
            {
                Actions = new List<INeuroAction>(),
                Context = "No actions are currently available.",
                IsContextSilent = true
            };

            bool foundActions = false;
            int i = 0;
            while (!foundActions && viewParsers.Count > i)
            {
                if (viewParsers[i].IsViewRelevant())
                {
                    actions = viewParsers[i].GetPossibleActions(logger);
                    foundActions = true;
                }
                i++;
            }

            return actions;
        }
    }
}
