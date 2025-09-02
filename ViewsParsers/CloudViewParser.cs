using BepInEx.Logging;
using GameViews.Cloud;
using NeuroValet.Actions;
using NeuroValet.StateData;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using static NeuroValet.ActionManager;
namespace NeuroValet.ViewsParsers
{
    /// <summary>
    /// One of the core action parsers - handles actions related to the Cloud view
    /// Which include actions within a city, and during travel
    /// </summary>
    internal class CloudViewParser : IViewParser
    {
        // Implement a singleton pattern for the CloudViewParser
        private static readonly Lazy<CloudViewParser> _instance = new Lazy<CloudViewParser>(() => new CloudViewParser());
        public static CloudViewParser Instance => _instance.Value;

        // Get the private field info
        FieldInfo iconsField = typeof(CloudView)
            .GetField("_icons", BindingFlags.NonPublic | BindingFlags.Instance);
        FieldInfo iconsListField = typeof(CloudViewIcons)
            .GetField("_icons", BindingFlags.NonPublic | BindingFlags.Instance);

        public ActionManager.PossibleActions GetPossibleActions(ManualLogSource logger)
        {
            GameStateData currentStateData = StateReporter.Instance.CurrentStateData;
            CloudView cloudView = (CloudView)GameViews.Static.cloudView;

            if (cloudView?.mode == MenuMode.City && currentStateData.City.IsInCity)
            {
                return GetPossibleActionsInCity(cloudView, currentStateData, logger);
            }
            else if (cloudView?.mode == MenuMode.Travel)
            {
                return GetPossibleActionsDuringJourney(cloudView, currentStateData, logger);
            }
            else
            {
                // Cloud view was somehow prioritized, but it's in an unknown state (or hidden?) and therefore shouldn't have been prioritized
                // like hidden mode is when story/market(maybe converse too) are in use, so should've been prioritized over this parser.
                // Therefor this should never happen, but leaving it here in case there's some weird edge cases
                logger.LogWarning($"Somehow reached unknown cloud view state, with no actions allowed yet cloud view was still prioritized for actions. \n" +
                    $"This leaves no possible actions for Neuro and she might get stuck if this doesn't auto resolve.\n" +
                    $"CloudView not null {cloudView == null}. Mode: {cloudView.mode} Is Visible: {cloudView.isVisible}\n" +
                    $"Day {currentStateData.GeneralData.CurrentDayNumber}. City: {currentStateData.City.CityName}.");

                ActionManager.PossibleActions possibleActions = new ActionManager.PossibleActions();
                possibleActions.Context = "(Unknown context with no known actions. This is probably not your fault. " +
                    "If this keeps up for more than 10 seconds, blame mod integration and ask Vedal for help)";
                return possibleActions;
            }
        }

        private PossibleActions GetPossibleActionsDuringJourney(CloudView cloudView, GameStateData currentStateData, ManualLogSource logger)
        {
            ActionManager.PossibleActions possibleActions = new ActionManager.PossibleActions();
            possibleActions.Context = $"You are on a journey to {currentStateData.Journey.ActiveJourney.DestinationCity}." +
                $"During a journey you may do some optional actions, but have a limited time to do them.";
            possibleActions.IsContextSilent = true;

            CloudViewIcons icons = (CloudViewIcons)iconsField.GetValue(cloudView);
            IList<Icon> iconsList = (IList<Icon>)iconsListField.GetValue(icons);

            for (int i = 0; i < iconsList.Count; i++)
            {
                var icon = iconsList[i];
                string iconName = icon.name.ToLower();
                if (iconName == "converse")
                {
                    possibleActions.Actions.Add(new TravelAction(
                        i, icon,
                        "Talk to fellow travellers on your journey to learn about possible routes from your destination and other rumours"));
                }
                if (iconName == "fogg")
                {
                    possibleActions.Actions.Add(new TravelAction(
                        i, icon,
                        "Attend and take care of Fogg, healing him a little"));
                }
                if (iconName == "wait")
                {
                    possibleActions.Actions.Add(new TravelAction(
                        i, icon,
                        "Pass some time reading the newspaper"));
                }
            }

            return possibleActions;
        }

        public bool IsViewRelevant()
        {
            // Check if the cloud view is viewable
            var cloudView = GameViews.Static.cloudView;
            return (cloudView != null && cloudView.isVisible && cloudView.mode != GameViews.Cloud.MenuMode.Hidden);
        }

        private PossibleActions GetPossibleActionsInCity(CloudView cloudView, GameStateData currentStateData, ManualLogSource logger)
        {
            ActionManager.PossibleActions possibleActions = new ActionManager.PossibleActions();
            StringBuilder context = new StringBuilder();
            context.AppendLine($"You are within a city [{currentStateData.City.CityName}].\n" +
                "You usually want to explore first, then go to market to buy & sell items, then plan your next journey, then sleep or skip time until you can go on that journey.\n" +
                "However each action spends some time, so you might to skip some or do them in a different order to avoid missing out on some, or worse, missing your next trip.\n" +
                "Note that available actions depend on time of day, and day of week, as well as some events.");

            CloudViewIcons icons = (CloudViewIcons)iconsField.GetValue(cloudView);
            IList<Icon> iconsList = (IList<Icon>)iconsListField.GetValue(icons);

            for (int i = 0; i < iconsList.Count; i++)
            {
                var icon = iconsList[i];
                string iconName = icon.name.ToLower();
                if (iconName == "pack" || iconName == "market")
                {
                    if (IsActionPossible(icon))
                    {
                        possibleActions.Actions.Add(new CityAction(
                            i, icon,
                            (iconName == "pack") ? "Pack initial items for your trip" : "Go buy and sell items on the market"));
                    }
                    else
                    {
                        context.AppendLine($"This city has a market, but it is not open right now. {icon.subtitle}");
                    }
                }
                else if (iconName == "bank")
                {
                    if (IsActionPossible(icon))
                    {
                        possibleActions.Actions.Add(new CityAction(
                            i, icon,
                            (currentStateData.City.BankFundsReady ?
                            "Go to the bank to collect the money you've requested." : 
                            "Go to a bank to get additional money, but will have to wait a few days in the city before it arrives.\n") +                            
                            $"Visiting the bank will take 2 hours, and can only be done during work hours."));
                    }
                    else
                    {
                        context.AppendLine($"This city has a bank, but it is not open right now. {icon.subtitle}");
                    }
                }
                else if (iconName == "beg")
                {
                    if (IsActionPossible(icon))
                    {
                        possibleActions.Actions.Add(new CityAction(
                            i, icon,
                            "Go beg for money on the streets. Will gain additional money but will take 8 hours"));
                    }
                }
                else if (iconName == "return")
                {
                    if (IsActionPossible(icon))
                    {
                        possibleActions.Actions.Add(new CityAction(
                            i, icon, "You've arrived back in London. Finish your journey."));
                    }
                }
                else if (iconName == "begin")
                {
                    if (IsActionPossible(icon))
                    {
                        possibleActions.Actions.Add(new CityAction(
                            i, icon, "Begin the first step on your trip around the world."));
                    }
                }
                else if (iconName == "hotel" || iconName == "sleep")
                {
                    if (IsActionPossible(icon))
                    {
                        StringBuilder sleepActionText = new StringBuilder();
                        sleepActionText.Append((iconName == "hotel")
                            ? "Spend the night in an hotel. +1 day and heal Fogg a bit"
                            : "Spend the night. +1 day.");

                        if (currentStateData.City.BankTransferInProgress)
                        {
                            sleepActionText.Append(" [You've requested funds from the bank. You should wait for them]");
                        }

                        possibleActions.Actions.Add(new CityAction(
                            i, icon, sleepActionText.ToString()));
                    }
                }
                else if (iconName == "depart" || iconName == "plan")
                {
                    if (IsActionPossible(icon))
                    {
                        possibleActions.Actions.Add(new CityAction(
                            i, icon, "View the various routes you can take from here, and possibly depart on one of them"));
                    }
                }
                else if (iconName == "explore")
                {
                    if (IsActionPossible(icon))
                    {
                        possibleActions.Actions.Add(new CityAction(
                            i, icon,
                            "Explore the city in search of new information, journeys, items, quests and secrets.\n" +
                            $"Exploring the city will take 4 hours."));
                    }
                    else
                    {
                        context.AppendLine($"Can explore this city, but not right now. ({icon.subtitle})");
                    }
                }
                else
                {
                    // Unknown icon. Log an error
                    logger.LogError($"Found unknown icon {iconName} in {currentStateData.City.CityName}. " +
                        $"This action is not implemented and Neuro will not know about it");
                }
            }

            // TODO - add action 'clicking on the clock' which will skip 1 hour ahead.
            // TODO - this might be done through a clock view parser, which will be run in conjunction with this?

            possibleActions.Context = context.ToString();
            possibleActions.IsContextSilent = true;

            return possibleActions;
        }

        private bool IsActionPossible(GameViews.Cloud.Icon icon)
        {
            return (icon.available && !icon.ignoreClicks && icon.fadeAlpha > 0f && icon.iconData.clickEventName != null);
        }
    }
}
