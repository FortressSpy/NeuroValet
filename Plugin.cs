using BepInEx;
using BepInEx.Logging;
using Game;
using Game.Conversation;
using Game.Player;
using System.Collections;
using UnityEngine;
using UnityEngine.TextCore;

namespace NeuroValet;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private DebugDataWindow gameDataForm = new DebugDataWindow();
    internal static new ManualLogSource Logger;

    private IPlayer player;
    private Game.Game gameInfo;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    private void Start()
    {
        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} Starting!");
        gameInfo = FindObjectOfType<Game.Game>();

        // Pass initial references to GameDataForm
        gameDataForm.SetPlayer(player);
        gameDataForm.SetGameInfo(gameInfo);

        StartCoroutine(GatherGameData());
    }

    private void Update()
    {
        // Toggle the visibility of the debug form when F1 is pressed
        if (Input.GetKeyDown(KeyCode.F1))
        {
            gameDataForm.ToggleDebugWindow();
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
            else
            {
                player = gameInfo?.player;
            }

            gameDataForm.SetGameInfo(gameInfo);
            gameDataForm.SetPlayer(player);

            yield return new WaitForSeconds(1f);
        }
    }

    void OnGUI()
    {
        gameDataForm.Draw();
    }
}
