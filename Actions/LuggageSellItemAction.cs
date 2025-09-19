using GameViews.Item;
using NeuroSdk;
using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;
using NeuroValet.ViewsParsers;

namespace NeuroValet.Actions
{
    internal class LuggageSellItemAction : NeuroSdk.Actions.NeuroAction
    {
        private readonly ItemSlot itemSlot;
        private readonly string name;
        private readonly string description;

        public override string Name => name;

        protected override string Description => description;

        protected override JsonSchema Schema => new()
        {
        };

        public LuggageSellItemAction(ItemSlot itemSlot)
        {
            this.itemSlot = itemSlot;

            this.name = $"sell_{itemSlot.item.item.displayName}";
            var itemPriceHere = GameData.Static.markets.SalePriceOfItemInCity(itemSlot.item.item, Game.Static.player?.currentCity).pounds;
            this.description = $"Sell {itemSlot.item.item.displayName} for £{itemPriceHere}";
        }

        protected override ExecutionResult Validate(ActionJData actionData)
        {
            if (MarketAndLuggageViewParser.Instance.IsViewRelevant())
            {
                return ExecutionResult.Success();
            }
            else
            {
                // This shouldn't happen. Maybe had some timed stuff and action hasn't unregistered in time before neuro decided to do it
                // Or user did some input outside neuro's control?
                return ExecutionResult.Failure(NeuroSdkStrings.ActionFailedUnregistered);
            }
        }

        protected override void Execute()
        {
            MarketAndLuggageViewParser.Instance.SellItem(itemSlot);
        }
    }
}
