using Game.Luggage;
using GameViews.Item;
using GameViews.Luggage;
using NeuroSdk;
using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;
using NeuroValet.ViewsParsers;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace NeuroValet.Actions
{
    internal class LuggageBuyItemAction : NeuroSdk.Actions.NeuroAction<MarketAndLuggageViewParser.MoveToData>
    {
        private readonly List<SuitcaseView> suitcaseViews;
        private readonly ItemSlot itemSlot;
        private readonly string name;
        private readonly string description;

        public override string Name => name;

        protected override string Description => description;

        protected override JsonSchema Schema => new()
        {
            Type = JsonSchemaType.Object,
            Required = new List<string> { "put_in_suitcase_number", "put_in_row_number", "put_in_slot_number" },
            Properties = new Dictionary<string, JsonSchema>
            {
                ["put_in_suitcase_number"] = QJS.Enum(GetSuitcaseNumbers()),
                ["put_in_row_number"] = QJS.Enum([0, 1]),
                ["put_in_slot_number"] = QJS.Enum([0, 1, 2, 3]),
            }
        };

        private IEnumerable<string> GetSuitcaseNumbers()
        {
            for (int i = 0; i < suitcaseViews.Count; i++)
            {
                yield return i.ToString();
            }
        }

        public LuggageBuyItemAction(ItemSlot itemSlot, List<SuitcaseView> suitcaseViews)
        {
            this.suitcaseViews = suitcaseViews;
            this.itemSlot = itemSlot;

            this.name = $"buy_{itemSlot.item.item.displayName}";
            this.description = $"Buy {itemSlot.item.item.displayName} and put it in one of your suitcases";
        }

        protected override ExecutionResult Validate(ActionJData actionData, out MarketAndLuggageViewParser.MoveToData parsedData)
        {
            if (MarketAndLuggageViewParser.Instance.IsViewRelevant())
            {
                uint? suitcase = actionData.Data?["put_in_suitcase_number"]?.Value<uint>();
                if (suitcase == null)
                {
                    parsedData = new MarketAndLuggageViewParser.MoveToData();
                    return ExecutionResult.Failure(NeuroSdkStrings.ActionFailedMissingRequiredParameter.Format("put_in_suitcase_number"));
                }

                uint? row = actionData.Data?["put_in_row_number"]?.Value<uint>();
                if (row == null)
                {
                    parsedData = new MarketAndLuggageViewParser.MoveToData();
                    return ExecutionResult.Failure(NeuroSdkStrings.ActionFailedMissingRequiredParameter.Format("put_in_row_number"));
                }

                uint? slot = actionData.Data?["put_in_slot_number"]?.Value<uint>();
                if (slot == null)
                {
                    parsedData = new MarketAndLuggageViewParser.MoveToData();
                    return ExecutionResult.Failure(NeuroSdkStrings.ActionFailedMissingRequiredParameter.Format("put_in_slot_number"));
                }

                parsedData = new MarketAndLuggageViewParser.MoveToData()
                {
                    moveToSuitcaseNumber = (int)suitcase.Value,
                    moveToPosition = new SuitcasePosition((int)slot.Value, (int)row.Value),
                };

                // could item fit
                if (suitcaseViews.Count <= parsedData.moveToSuitcaseNumber)
                {
                    return ExecutionResult.Failure(
                        NeuroSdkStrings.ActionFailedInvalidParameter.Format("put_in_suitcase_number") +
                        $" Can only put items in suitcases 0 to {suitcaseViews.Count - 1}");
                }
                // Is row valid?
                if (1 < parsedData.moveToPosition.y)
                {
                    return ExecutionResult.Failure(
                        NeuroSdkStrings.ActionFailedInvalidParameter.Format("put_in_row_number") +
                        $" Can only put items in row 0 or 1");
                }
                // Is slot valid - Could item fit there?
                if (4 < parsedData.moveToPosition.x + itemSlot.item.item.stats.size)
                {
                    return ExecutionResult.Failure(
                        NeuroSdkStrings.ActionFailedInvalidParameter.Format("put_in_slot_number") +
                        $" Item can only fit in slots 0 to {4 - itemSlot.item.item.stats.size}");
                }

                var targetSuitcase = suitcaseViews[(int)parsedData.moveToSuitcaseNumber];
                if (targetSuitcase.suitcase.CanFitItem(itemSlot.item.item, parsedData.moveToPosition))
                {
                    return ExecutionResult.Success();
                }
                else
                {
                    return ExecutionResult.Failure(
                        NeuroSdkStrings.ActionFailedInvalidParameter.Format("put_in_row_number / put_in_slot_number") +
                        $" Item '{itemSlot.item.item.displayName}' cannot fit within suitcase {parsedData.moveToSuitcaseNumber}" +
                        $" at row {parsedData.moveToPosition.y}, slot {parsedData.moveToPosition.x}");
                }
            }
            else
            {
                // This shouldn't happen. Maybe had some timed stuff and action hasn't unregistered in time before neuro decided to do it
                // Or user did some input outside neuro's control?
                parsedData = new MarketAndLuggageViewParser.MoveToData();
                return ExecutionResult.Failure(NeuroSdkStrings.ActionFailedUnregistered);
            }
        }

        protected override void Execute(MarketAndLuggageViewParser.MoveToData parsedData)
        {
            MarketAndLuggageViewParser.Instance.DragItem(itemSlot, parsedData);
        }
    }
}
