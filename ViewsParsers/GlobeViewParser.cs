using BepInEx.Logging;
using GameResources.MapData;
using GameViews;
using NeuroValet.Actions;
using System;
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
            if (IsViewRelevant())
            {
                if (StateReporter.Instance.CurrentStateData.City.IsInCity)
                {
                    possibleActions.Actions.Add(new EnterCityAction(StateReporter.Instance.CurrentStateData.City.CityName));

                    foreach (var journey in StateReporter.Instance.CurrentStateData.Journey.RoutesFromCurrentCity)
                    {
                        possibleActions.Actions.Add(new SelectJourneyAction(journey));
                    }
                }
            }

            possibleActions.Context = "You are looking at the globe. You can choose your current city to view more actions there, or the destinations cities you can get to from here to plan and depart to them";
            possibleActions.IsContextSilent = false;

            return possibleActions;
        }

        public void FocusOnPlayer()
        {
            // Focus on the player looks at the current 'thing' where the player is. this is usually either a city or a journey
            // However we don't let Neuro look outside when on a current journey, so this should mostly be for looking into the current city
            Game.Static.game.globeControls.FocusOnPlayer();
        }

        public void FocusOnCity(ICityInfo city)
        {
            Game.Static.game.globeController.SelectCity(city);
        }

        // Is the globe visible and player can be focused on?
        public bool IsViewRelevant()
        {
            IGameViews gameViews = GameViews.Static.gameViews;

            if (gameViews == null) return false;

            // Check if the globe is viewable, and there isn't anything that takes8 control
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
