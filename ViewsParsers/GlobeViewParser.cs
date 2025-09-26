using BepInEx.Logging;
using GameResources.MapData;
using GameViews;
using GameViews.BottomNav;
using GameViews.Departure;
using GameViews.InfoCard;
using HarmonyLib;
using NeuroValet.Actions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using static NeuroValet.ActionManager;

namespace NeuroValet.ViewsParsers
{
    internal class GlobeViewParser : IViewParser
    {
        // Implement a singleton pattern for the GlobeViewParser
        private static readonly Lazy<GlobeViewParser> _instance = new Lazy<GlobeViewParser>(() => new GlobeViewParser());
        public static GlobeViewParser Instance => _instance.Value;

        private static readonly FieldInfo CurrentSpeechBubbleViewField = AccessTools.Field(typeof(FoggPanelView), "currentSpeechBubbleView");
        private static readonly FieldInfo BribeStateField = AccessTools.Field(typeof(FoggPanelView), "bribeState");
        private static readonly FieldInfo ButtonsField = AccessTools.Field(typeof(FoggSpeechBubbleView), "buttons");

        private GlobeViewParser() { }

        public PossibleActions GetPossibleActions(ManualLogSource logger)
        {
            var selectedCity = Game.Static.game.globeController.selectedCity;
            PossibleActions possibleActions = new PossibleActions();
            
            StringBuilder context = new StringBuilder();
            context.AppendLine("You are looking at the globe view, where you can see your current city and other cities you can travel to.");

            if (StateReporter.Instance.CurrentStateData.City.IsInCity)
            {
                string currentCityName = StateReporter.Instance.CurrentStateData.City.CityName;
                context.Append($"You are currently in {currentCityName}");
                possibleActions.Actions.Add(new EnterCityAction(currentCityName));

                foreach (var journey in StateReporter.Instance.CurrentStateData.Journey.RoutesFromCurrentCity)
                {
                    // Is this city selected? if so, we have different actions about it
                    if (selectedCity?.cityInfo.name == journey.DestinationCity.name)
                    {
                        context.AppendLine($" and viewing possible journey to {journey.DestinationCity.displayName}.");
                        if (journey.CanDepartRightNow)
                        {
                            possibleActions.Actions.Add(new EmbarkJourneyAction(journey));
                        }
                        else
                        {
                            // Can negotiate for earlier departure?
                            var foggPanelView = (FoggPanelView)GameViews.Static.bottomNavView?.foggPanelView;
                            if (foggPanelView != null && foggPanelView.isActiveAndEnabled && foggPanelView.showingSpeechBubble)
                            {
                                var speechBubbleView = (FoggSpeechBubbleView)CurrentSpeechBubbleViewField.GetValue(foggPanelView);
                                var bribeState = (int)BribeStateField.GetValue(foggPanelView);

                                ReportNegotiationStatusInContext(context, speechBubbleView, bribeState);

                                if (speechBubbleView.hasButtons)
                                {
                                    var buttons = (List<FoggSpeechBubbleButtonView>)ButtonsField.GetValue(speechBubbleView);
                                    for (int i = 0; i < buttons.Count; i++)
                                    {
                                        possibleActions.Actions.Add(new NegotiateScheduleAction(bribeState, buttons[i], i));
                                    }
                                }
                            }
                        }

                        context.Append($"Full journey information:\n{journey.FullContext}");
                    }
                    else
                    {
                        possibleActions.Actions.Add(new SelectJourneyAction(journey));
                    }
                }
            }
            else
            {
                logger.LogWarning("Trying to get actions from GlobeViewParser outside a city. This is unexpected.");
                logger.LogDebug("Current City (probably empty):" + StateReporter.Instance.CurrentStateData.City.CityName);
                logger.LogDebug("Current Journey:" + StateReporter.Instance.CurrentStateData.Journey.ActiveJourney.DebugText);
            }

            possibleActions.Context = context.ToString();
            possibleActions.IsContextSilent = false;

            return possibleActions;
        }
        
        // Report speech bubble text without any HTML tags
        private static void ReportNegotiationStatusInContext(StringBuilder context, FoggSpeechBubbleView speechBubbleView, int bribeState)
        {
            // Add some extra context when items affect negotiation
            if (bribeState == 2)
            {
                context.AppendLine("Negotiation status: " + Regex.Replace(speechBubbleView.speechText.text, "<.*?>", string.Empty));
            }
            // If can bribe, the final decision text is useless
            // however if can't bribe, it explains why not
            else if (bribeState != 3 || !speechBubbleView.hasButtons)
            {
                context.AppendLine(Regex.Replace(speechBubbleView.speechText.text, "<.*?>", string.Empty));
            }
            else if (bribeState == 3)
            {
                // explain that in addition to the negotiation cost there is the ticket cost
                context.AppendLine("Can pay for an earlier departure, but remember, " 
                    + speechBubbleView.additionalFundsText.text);
            }
        }

        public void FocusOnPlayer()
        {
            // Focus on the player looks at the current 'thing' where the player is. this is usually either a city or a journey
            // However we don't let Neuro look outside when on a current journey, so this should mostly be for looking into the current city
            Game.Static.game.globeControls.FocusOnPlayer();
        }

        public void FocusOnCity(ICityInfo city)
        {
            GameViews.Static.bottomNavView.foggPanelView?.ClearFoggBubbles(); // clear any fogg bubbles - belonged to previous focused city
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
