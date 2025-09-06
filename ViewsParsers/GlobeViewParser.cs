using BepInEx.Logging;
using GameResources.MapData;
using GameViews;
using GameViews.InfoCard;
using HarmonyLib;
using NeuroValet.Actions;
using System;
using System.Text;
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
            var selectedCity = Game.Static.game.globeController.selectedCity;
            PossibleActions possibleActions = new PossibleActions();
            
            StringBuilder context = new StringBuilder();
            context.AppendLine("You are looking at the globe view, where you can see your current city and other cities you can travel to.");

            // TODO is there anything in the globe view parser that should happen outside of being in a city?
            if (StateReporter.Instance.CurrentStateData.City.IsInCity)
            {
                possibleActions.Actions.Add(new EnterCityAction(StateReporter.Instance.CurrentStateData.City.CityName));

                foreach (var journey in StateReporter.Instance.CurrentStateData.Journey.RoutesFromCurrentCity)
                {
                    // Is this city selected? if so, we have different actions about it
                    if (selectedCity?.cityInfo.name == journey.DestinationCity.name)
                    {
                        context.AppendLine($"You are viewing possible journey to {journey.DestinationCity.displayName}. Full journey information to it: {journey.FullContext}");
                        if (journey.CanDepartRightNow)
                        {
                            possibleActions.Actions.Add(new EmbarkJourneyAction(journey));
                        }
                        else
                        {
                            // TODO - negotiate if possible
                        }
                    }
                    else
                    {
                        possibleActions.Actions.Add(new SelectJourneyAction(journey));
                    }
                }
            }

            possibleActions.Context = context.ToString();
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
            // TODO This way to focus on a city doesn't hide the Negotiate action when switching between cities properly...
            Game.Static.game.globeController?.SelectCity(city);
            Game.Static.gameAudio?.sfxController?.PlayClickCitySFX();
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

        internal void OpenDepartureWindow()
        {
            // Is a target city selected?
            // this should only have happened if we've focused on a destination city,
            // as neuro doesn't have actions to focus on other cities (except current city, but that enters other modes)
            if (Game.Static.game.globeController.selectedCity == null)
            {
                return;
            }

            var infoCardsView = (InfoCardsView)GameViews.Static.infoCardView;

            // if can leave on this current journey, then there should be a single embark card - find and click on it
            var getBestTargetCard = AccessTools.Method(typeof(InfoCardsView), "GetBestTargetCard");
            var infoCard = (InfoCardView)getBestTargetCard.Invoke(infoCardsView, null);
            if (infoCard is EmbarkCardView embarkCard)
            {
                embarkCard.OnClickedEmbark();
                return;
            }
        }
    }
}
