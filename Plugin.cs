using BepInEx;
using BepInEx.Logging;
using Game.Luggage;
using Game.Player;
using GameResources.Items;
using System.Collections;
using System.Numerics;
using System.Text;
using UnityEngine;

namespace NeuroValet;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private DebugDataWindow gameDataForm = new DebugDataWindow();
    internal static new ManualLogSource Logger;

    private Game.Game gameInfo;
    private GameData.Clock clock;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    private void Start()
    {
        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} Starting!");
        StartCoroutine(GatherGameData());
    }

    private void Update()
    {
        // Toggle the visibility of the debug form when F1 is pressed
        if (Input.GetKeyDown(KeyCode.F1))
        {
            gameDataForm.ToggleDebugWindow();
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            var player = gameInfo?.player;
            var market = player.marketForCurrentCity;
            var item = market?.items[0];

            if (player.CanBuyItemAtPrice(item.Value.item, item.Value.price))
            {
                player.BuyItemFromMarket(item.Value.item, item.Value.price, player.suitcases[0], new SuitcasePosition(0, 0)); // TODO figure out open slots
                player.RefreshSuitcasesNotingAcquisitions(true);
            }
        }
    }

    // Once a second, Gather game state data
    private IEnumerator GatherGameData()
    {
        while (true)
        {
            if (gameInfo == null)
            {
                Logger.LogWarning("Game info not found! Attempting to find...");
                gameInfo = FindObjectOfType<Game.Game>();
            }
            if (clock == null)
            {
                clock = (GameData.Clock)GameData.Static.clock;
            }

            gameDataForm.SetGameInfo(gameInfo, clock);
            gameDataForm.SetPlayer(gameInfo?.player);
            gameDataForm.SetStory(gameInfo?.story);

            yield return new WaitForSeconds(1f);
        }
    }

    void OnGUI()
    {
        gameDataForm.Draw();
    }
}
