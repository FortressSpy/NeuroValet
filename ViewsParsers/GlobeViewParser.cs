using BepInEx.Logging;
using GameViews;
using NeuroSdk.Actions;
using NeuroValet.Actions;
using NeuroValet.StateData;
using System;
using System.Collections.Generic;
using static NeuroValet.ActionManager;

namespace NeuroValet.ViewsParsers
{
    internal class GlobeViewParser : IViewParser
    {
        // Implement a singleton pattern for the GlobeViewParser
        private static readonly Lazy<GlobeViewParser> _instance = new Lazy<GlobeViewParser>(() => new GlobeViewParser());
        public static GlobeViewParser Instance => _instance.Value;

        private GlobeViewParser() { }

        public PossibleActions GetPossibleActions(ManualLogSource logger)
        {
            PossibleActions possibleActions = new PossibleActions();
            // TODO - other globe related actions -  Focusing on various available journeys. maybe focusing on other cities to see clues on them, especially if have any quests there?
            if (IsViewRelevant())
            {
                possibleActions.Actions.Add(new EnterCityAction(""));
            }

            possibleActions.Context = "You are looking at the globe. You can choose to focus on your current location to do more actions";
            possibleActions.IsContextSilent = false;

            return possibleActions;
        }

        public void FocusOnPlayer()
        {
            // Focus on the player looks at the current 'thing' where the player is. this is usually either a city or a journey
            // However we don't let Neuro look outside when on a current journey, so this should mostly be for looking into the current city
            Game.Static.game.globeControls.FocusOnPlayer();
        }

        // Is the globe visible and player can be focused on?
        public bool IsViewRelevant()
        {
            // Check if the globe is viewable
            IGameViews gameViews = GameViews.Static.gameViews;

            if (gameViews == null) return false;

            return
                (!StateReporter.Instance.CurrentStateData.GeneralData.IsPrologueActive) &&
                (!gameViews.settingsView?.isShown ?? true) &&
                (!gameViews.converseView?.isShown ?? true) && 
                (!gameViews.marketAndLuggageView?.isShown ?? true) && 
                (!gameViews.cloudView?.isShown ?? true) && 
                (!gameViews.storyView?.isShown ?? true);
        }
    }
}
