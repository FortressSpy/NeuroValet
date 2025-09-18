
using BepInEx.Logging;
using Game.Luggage;
using GameViews.Departure;
using GameViews.Item;
using GameViews.Market;
using HarmonyLib;
using NeuroValet.Actions;
using NeuroValet.Overrides;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;
using static NeuroValet.ActionManager;

namespace NeuroValet.ViewsParsers
{
    internal class MarketAndLuggageViewParser : IViewParser
    {
        // Implement a singleton pattern for the GlobeViewParser
        private static readonly Lazy<MarketAndLuggageViewParser> _instance = new Lazy<MarketAndLuggageViewParser>(() => new MarketAndLuggageViewParser());
        public static MarketAndLuggageViewParser Instance => _instance.Value;

        private MarketAndLuggageViewParser() { }

        static readonly MethodInfo itemViewHold = AccessTools.Method(typeof(ItemView), "Hold");

        public ActionManager.PossibleActions GetPossibleActions(ManualLogSource logger)
        {
            var view = (MarketAndLuggageView)GameViews.Static.marketAndLuggageView;
            PossibleActions possibleActions = new PossibleActions();

            // Is dragging item right now?
            if (MouseSimulator.Instance.OverrideMouse)
            {
                // Dragging item takes a little bit of time, and while doing it, the item slots are in an inconsistent state
                // which will cause failures if we try to parse them, and anyway neuro shouldn't be trying to do anything else
                possibleActions.Context = "Currently dragging an item, please wait...";
                possibleActions.IsContextSilent = true;
                return possibleActions;
            }

            StringBuilder context = new StringBuilder();
            context.AppendLine("You are looking at your current luggage and items");
            context.AppendLine("(Each suitcase has 2 rows and 4 slots in each row. Items can take 1-4 horizontal slots)");
            context.AppendLine("(You can buy new suitcases in most cities (but not all), but each one costs money and will take extra space on journeys)");
            context.AppendLine("You can rearrange existing items or sell them to free space, preferably so you'll need as few suitcases as possible (empty suitcases will be removed)");

            if (view.mode == GameViews.Market.MarketAndLuggageViewMode.MarketAndLuggage)
            {
                context.AppendLine("You are also looking at the market, which lets you buy new items");
            }

            GetContextAndActionsOnLuggage(possibleActions, view, context, logger);

            // TODO Get items available to buy in market (if in market view), and add buy action for each

            // TODO - add action to close the market/luggage view and return to previous view (probably call OnLuggageButtonClicked())
            possibleActions.Actions.Add(new LuggageCloseWindowAction());

            possibleActions.Context = context.ToString();
            possibleActions.IsContextSilent = false;

            return possibleActions;
        }

        private static void GetContextAndActionsOnLuggage(PossibleActions possibleActions, MarketAndLuggageView view, StringBuilder context, ManualLogSource logger)
        {
            context.AppendLine("Your current luggage (remember that non-reported slots within these suitcases are empty):");
            // Get current items in luggage, and add sell action for each
            for (int i = 0; i < view.luggageView.suitcases.Count; i++)
            {
                var suitcase = view.luggageView.suitcases[i];
                context.AppendLine($"Suitcase {i}:");
                if (suitcase.ghost)
                {
                    context.AppendLine("You can purchase this suitcase by putting an item in here.");
                    continue;
                }
                if (suitcase.suitcase.isEmpty)
                {
                    context.AppendLine("This suitcase is empty.");
                    continue;
                }

                foreach (var itemSlot in suitcase.itemSlots)
                {
                    if (itemSlot.item == null || itemSlot.IsEmpty)
                    {
                        logger.LogError($"Couldn't find full data for an item. Is user manually holding an item?");
                        continue;
                    }

                    // Game code's is annoying, and saves item data in two places that don't properly reference each other (ItemSlot and InventoryItem)
                    // Suitcase level holds both in different arrays. Go over both and match them up
                    Game.Luggage.InventoryItem item = suitcase.suitcase.contents.FirstOrDefault(item => itemSlot.item.item == item.item);
                    if (item == null || item.isEmpty)
                    {
                        logger.LogError($"Couldn't find full data for {itemSlot.item.item.displayName}. Is user manually holding an item?");
                        continue;
                    }

                    context.Append($"Row {item.position.y} {(item.item.stats.size == 1
                        ? $"Slot {item.position.x}"
                        : $"Slots {item.position.x} to {item.position.x + item.item.stats.size}")}: ");
                    context.Append($"{(item.quantity <= 1 ? "" : $"{item.quantity} ")}{item.item.displayName}");
                    context.AppendLine($" - {TextGen.DetailTextForItem(item.item)}");

                    // TODO add action for selling this item
                    possibleActions.Actions.Add(new LuggageMoveItemAction(item, view.luggageView.suitcases, i, itemSlot));
                }
            }
        }

        public void DragItem(ItemSlot itemCurrentSpot, MoveToData moveToData)
        {
            var view = (MarketAndLuggageView)GameViews.Static.marketAndLuggageView;
            var targetSuitcaseView = view.luggageView.suitcases[moveToData.moveToSuitcaseNumber];

            var itemCurrentPosition = itemCurrentSpot.uiCamera.WorldToScreenPoint(itemCurrentSpot.rectTransform.position);

            // Calculate target position
            // (most itemContainer suitcase position calculations are a bit broken,
            // so have to go through local position, then properly transform to world, then to camera)
            UnityEngine.Vector2 itemTargetLocalPosition = targetSuitcaseView.itemContainer.SuitcasePositionToLocalPosition(moveToData.moveToPosition);
            itemTargetLocalPosition += new UnityEngine.Vector2(30, 0); // offset a bit to the right to make sure we're in the slot, not on the border
            UnityEngine.Vector3 itemTargetWorldPosition = targetSuitcaseView.itemContainer.rectTransform.TransformPoint(itemTargetLocalPosition);
            UnityEngine.Vector3 itemTargetPosition = itemCurrentSpot.uiCamera.WorldToScreenPoint(itemTargetWorldPosition);

            // Drag item over time to new position
            MouseSimulator.Instance.DragItem(itemCurrentPosition, itemTargetPosition, 
                (Action)itemViewHold.CreateDelegate(typeof(Action), itemCurrentSpot.item), itemCurrentSpot.item.Release);
        }

        public void CloseMarketAndLuggageView()
        {
            var view = (MarketAndLuggageView)GameViews.Static.marketAndLuggageView;
            view.OnLuggageButtonClicked();
        }

        public bool IsViewRelevant()
        {
            var view = GameViews.Static.marketAndLuggageView;
            return view?.isVisible ?? false;
        }
    }
}
