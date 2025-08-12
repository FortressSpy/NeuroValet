using BepInEx;
using BepInEx.Logging;
using Game.Luggage;
using Game.Player;
using GameResources.Items;
using HarmonyLib;
using NeuroSdk;
using System.Collections;
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
        Harmony.CreateAndPatchAll(typeof(MouseSimulator).Assembly, MyPluginInfo.PLUGIN_GUID);

        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        // Load mouse debug texture
        string pluginDir = System.IO.Path.GetDirectoryName(Info.Location);
        string cursorPath = System.IO.Path.Combine(pluginDir, @"Assets\mouse-pointer.png");
        MouseSimulator.LoadCursorTexture(cursorPath);
    }

    private void Start()
    {
        NeuroSdkSetup.Initialize("80 Days");
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
            MouseSimulator.ReleaseMousePosition();
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            MouseSimulator.SetMousePosition(new Vector3(500, 500, 0), Logger);
        }
        else if (Input.GetKeyDown(KeyCode.F4))
        {
            MouseSimulator.SetMousePosition(new Vector3(200, 200, 0), Logger);
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
        MouseSimulator.DrawCursor();
    }
}
