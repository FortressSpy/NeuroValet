using BepInEx.Logging;
using GameViews.Cloud;
using GameViews.Story;
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
        FieldInfo iconsField = typeof(CloudViewIcons)
            .GetField("_icons", BindingFlags.NonPublic | BindingFlags.Instance);
        FieldInfo iconsListField = typeof(IList<Icon>)
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
                // TODO - implement after gathering Journey data
                return GetPossibleActionsDuringJourney(cloudView, currentStateData, logger);
            }
            else
            {
                // Cloud view was somehow prioritized, but it's in an unknown state (or hidden?) and therefore shouldn't have been prioritized
                // This should never happen, but leaving it here in case there's some weird edge cases
                // TODO report journey data too (current journey name, next city)
                logger.LogError($"Somehow reached unknown cloud view state, with no actions allowed yet cloud view was still prioritized for actions. \n" +
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
            //possibleActions.Context = $"You are on a trip to X that will take until Y." +
                //$"During a trip you may do some optional actions, as well as some story ";
            possibleActions.IsContextSilent = true;
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
                "You usually will want to explore first, then go to market to buy & sell items, then plan your next journey, then sleep or skip time until you can go on that journey.\n" +
                "However each action spends some time, so you might to skip some or do them in a different order to avoid missing out on some, or worse, missing your next trip.\n" +
                "Note that available actions depend on time of day, and day of week, as well as some events.");

            CloudViewIcons icons = (CloudViewIcons)iconsField.GetValue(cloudView);
            IList<Icon> iconsList = (IList<Icon>)iconsListField.GetValue(icons);

            for (int i = 0; i< iconsList.Count; i++)
            {
                var icon = iconsList[i];
                string iconName = icon.name.ToLower();
                if (iconName == "pack" || iconName == "market")
                {
                    // Add market action
                    if (icon.available)
                    {

                    }
                    else
                    {
                        // add context explaining why it's not open
                    }
                }
                else if (iconName == "bank" || iconName == "beg")
                {
                    // Add Bank action
                }
                else if (iconName == "return")
                {
                    // TODO - end game action
                }
                else if (iconName == "begin")
                {
                    // TODO start game action. equal to plan?
                }
                else if (iconName == "hotel" || iconName == "sleep")
                {
                    // TODO sleep action
                }
                else if (iconName == "depart" || iconName == "plan")
                { 
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
    }
}
