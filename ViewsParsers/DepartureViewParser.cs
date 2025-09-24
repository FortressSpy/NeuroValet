using BepInEx.Logging;
using GameData;
using GameViews.Departure;
using HarmonyLib;
using NeuroSdk.Actions;
using NeuroValet.Actions;
using NeuroValet.StateData;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using static NeuroValet.ActionManager;

namespace NeuroValet.ViewsParsers
{
    internal class DepartureViewParser : IViewParser
    {
        // Implement a singleton pattern for the StoryViewParser
        private static readonly Lazy<DepartureViewParser> _instance = new Lazy<DepartureViewParser>(() => new DepartureViewParser());
        public static DepartureViewParser Instance => _instance.Value;

        static readonly MethodInfo canAffordTicketGetter = AccessTools.PropertyGetter(typeof(DepartureView), "canAffordTicket");
        static readonly MethodInfo healthReportGetter = AccessTools.PropertyGetter(typeof(DepartureView), "healthReport");
        static readonly MethodInfo luggageReportGetter = AccessTools.PropertyGetter(typeof(DepartureView), "luggageReport");

        private DepartureViewParser()
        {
        }

        public void ClickMainButton()
        {
            var departureView = GameViews.Static.departureView;
            departureView.PressEnterKey();
        }

        public void OpenLuggageWindow()
        {
            DynamicEvent.Raise("DepartureViewControllerRequestedClose");
            DynamicEvent.Raise("DepartureViewControllerRequestedOpenLuggage");
        }

        public void LeaveDepartureMenu()
        {
            var departureView = GameViews.Static.departureView;
            departureView.PressBackKey();
        }

        public void PayForExtraStorage()
        {
            var departureView = GameViews.Static.departureView;
            var buyExtraLuggageMethod = AccessTools.Method(typeof(DepartureView), "BuyExtraLuggageSlot");
            buyExtraLuggageMethod.Invoke(departureView, null);
        }
        
        public ActionManager.PossibleActions GetPossibleActions(ManualLogSource logger)
        {
            PossibleActions possibleActions = new PossibleActions();
            possibleActions.Actions = new List<INeuroAction>();

            var departureView = GameViews.Static.departureView;
            var journeyData = new Journey(departureView.journey);
            var playerData = StateReporter.Instance.CurrentStateData.Player;

            StringBuilder context = new StringBuilder();
            context.AppendLine($"You are viewing departure screen for trip to {journeyData.DestinationCity.displayName}.");

            // Determine possible actions
            if (journeyData.CanDepartRightNow)
            {
                context.AppendLine($"You can choose to depart right now for this journey, or go back and do something else.");

                // Can afford ticket?
                if ((bool)canAffordTicketGetter.Invoke(departureView, null))
                {
                    var healthReport = healthReportGetter.Invoke(departureView, null);
                    var dontTravelField = AccessTools.Field(healthReport.GetType(), "dontTravel");
                    var dontTravel = (bool)dontTravelField.GetValue(healthReport);
                    var itemBenefitField = AccessTools.Field(healthReport.GetType(), "itemBenefit");
                    var itemBenefit = (int)itemBenefitField.GetValue(healthReport);
                    var healthCostField = AccessTools.Field(healthReport.GetType(), "cost");
                    var healthCost = (int)healthCostField.GetValue(healthReport);

                    // Report health damage (will also report if can't travel due to health)
                    var healthRiskReport = TextGen.RigoursBodyTextFor(departureView.journey, dontTravel);
                    healthRiskReport = Regex.Replace(healthRiskReport, "<.*?>", string.Empty); // remove any HTML tags
                    context.AppendLine($"Health risk report: {healthRiskReport}. Costs {healthCost} health but items protect {itemBenefit} of that");

                    // Can travel safely?
                    if (!dontTravel)
                    {
                        var luggageReport = luggageReportGetter.Invoke(departureView, null);
                        var tooMuchLuggageField = AccessTools.Field(luggageReport.GetType(), "tooMuchLuggage");
                        bool tooMuchLuggage = (bool)tooMuchLuggageField.GetValue(luggageReport);
                        var canBuyExtraSlotsField = AccessTools.Field(luggageReport.GetType(), "canBuyExtraSlots");
                        bool canBuyExtraSlots = (bool)canBuyExtraSlotsField.GetValue(luggageReport);
                        var canAffordExtraSlotsField = AccessTools.Field(luggageReport.GetType(), "canAffordExtraSlots");
                        bool canAffordExtraSlots = (bool)canAffordExtraSlotsField.GetValue(luggageReport);
                        var extraSlotsCostField = AccessTools.Field(luggageReport.GetType(), "extraSlotsCost");
                        var extraSlotsCost = (MoneyValue)extraSlotsCostField.GetValue(luggageReport);

                        // Has room for luggage?
                        if (!tooMuchLuggage)
                        {
                            // All good, can depart! Note that we need to report journey cost
                            possibleActions.Actions.Add(new DepartureStartJourneyAction(journeyData, itemBenefit + healthCost));
                        }
                        else
                        {
                            var luggageCapacityReport = TextGen.Luggage.LuggageExtraCapacityFor(departureView.journey);
                            context.AppendLine($"However you cannot travel due to luggage. (Game is saying {luggageCapacityReport})");

                            // Can either buy more luggage space, or remove some luggage
                            possibleActions.Actions.Add(new DepartureOpenLuggageAction());

                            if (canBuyExtraSlots)
                            {
                                string text = TextGen.Luggage.LuggageExtraCapacityFor(departureView.journey);
                                string text2 = TextGen.Money.PriceInPounds(extraSlotsCost);
                                string offer = text.ToUpper() + " - " + text2;

                                // Can afford to buy more luggage space
                                if (canAffordExtraSlots)
                                {
                                    possibleActions.Actions.Add(new DepartureBuyStorageAction(offer));
                                }
                                else
                                {
                                    context.AppendLine($"There's an offer to buy more storage, but you don't have enough money for that. Offer: {offer}");
                                }
                            }
                        }
                    }
                    else
                    {
                        context.AppendLine($"You cannot travel this route due to health risk");
                    }
                }
                else
                {
                    context.AppendLine($"However, The ticket price is £{journeyData.Cost} more than {playerData.PlayerName}(YOU) can pay" +
                        $"{(!playerData.IsWithFogg ? " without Fogg" : "")}");
                }
            }
            else
            {
                context.AppendLine($"You cannot depart right now for this journey.");
            }

            // Can always leave the menu
            possibleActions.Actions.Add(new DepartureCloseWindowAction());

            context.AppendLine("Full journey data:\n" + journeyData.FullContext);
            possibleActions.Context = context.ToString();
            possibleActions.IsContextSilent = false;

            return possibleActions;
        }

        public bool IsViewRelevant()
        {
            var departureView = GameViews.Static.departureView;
            return departureView?.isVisible ?? false;
        }
    }
}
