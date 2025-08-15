using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Game.Luggage;
using Game.Player;
using GameResources.Items;
using HarmonyLib;
using NeuroSdk;
using NeuroValet.Utils;
using System;
using System.Collections;
using System.Text;
using UnityEngine;

namespace NeuroValet;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class NeuroValet : BaseUnityPlugin
{
    private ConfigEntry<string> configWebSocketUrl;

    private DebugDataWindow gameDataForm = new DebugDataWindow();
    internal static new ManualLogSource Logger;

    private Game.Game gameInfo;
    private GameData.Clock clock;
    private bool isReady = false;

    private void Awake()
    {
        configWebSocketUrl = Config.Bind("NeuroSdk",                // The section under which the option is shown
                                         "WebSocket",               // The key of the configuration option in the configuration file
                                         "ws://localhost:8000",     // default value
                                         "Neuro SDK's Web Socket"); // Description

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
        // Set environment variable for NeuroSdk
        if (!configWebSocketUrl.Value.IsNullOrEmpty())
        {
            Environment.SetEnvironmentVariable("NEURO_SDK_WS_URL", configWebSocketUrl.Value);

            NeuroSdkSetup.Initialize("80 Days");
            isReady = true;
            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} launched. Web socket set to: {configWebSocketUrl.Value}");
            StartCoroutine(GatherGameData());
        }
        else
        {
            Logger.LogError($"{MyPluginInfo.PLUGIN_GUID} can't start because web socket is not defined in BepInEx\\config\\{MyPluginInfo.PLUGIN_GUID}.cfg!");
        }
    }

    private void Update()
    {
        if (!isReady) return;

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
