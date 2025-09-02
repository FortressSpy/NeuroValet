using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Game.Luggage;
using Game.Player;
using GameResources.Items;
using HarmonyLib;
using NeuroSdk;
using NeuroSdk.Actions;
using NeuroSdk.Messages.Outgoing;
using NeuroValet.StateData;
using NeuroValet.Overrides;
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

    private bool isReady = false;

    private ActionManager actionManager;

    private ActionManager.PossibleActions neuroCurrentActions;
    private NeuroSdk.Actions.ActionWindow currentActionWindow;

    private void Awake()
    {
        Logger = base.Logger;

        configWebSocketUrl = Config.Bind("NeuroSdk",                // The section under which the option is shown
                                         "WebSocket",               // The key of the configuration option in the configuration file
                                         "ws://localhost:8000",     // default value
                                         "Neuro SDK's Web Socket"); // Description
        ClockOverrides.Initialize(Config, Logger);

        Harmony.CreateAndPatchAll(typeof(MouseSimulator).Assembly, MyPluginInfo.PLUGIN_GUID);

        // Load mouse debug texture
        string pluginDir = System.IO.Path.GetDirectoryName(Info.Location);
        string cursorPath = System.IO.Path.Combine(pluginDir, @"Assets\mouse-pointer.png");
        MouseSimulator.LoadCursorTexture(cursorPath);

        actionManager = new ActionManager(Logger);

        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    private void Start()
    {
        // Set environment variable for NeuroSdk
        if (!configWebSocketUrl.Value.IsNullOrEmpty())
        {
            Environment.SetEnvironmentVariable("NEURO_SDK_WS_URL", configWebSocketUrl.Value);

            StartCoroutine(ReportGameStateToNeuro());

            NeuroSdkSetup.Initialize("80 Days");
            isReady = true;
            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} launched. Web socket set to: {configWebSocketUrl.Value}");

            // TODO - improve the context. Ask how much should be here (explanation of the game mechanics? world? tips (like how much money you generally need)?
            Context.Send("You are playing as Passepartout, valet to Phileas Fogg. He made a bet to travel the world in 80 days or less, starting from London.", true);
        }
        else
        {
            Logger.LogError($"{MyPluginInfo.PLUGIN_GUID} can't start because web socket is not defined in BepInEx\\config\\{MyPluginInfo.PLUGIN_GUID}.cfg!");
        }
    }

    private void Update()
    {
        if (!isReady) return;

        CheckDebugInputs();
    }

    private void CheckDebugInputs()
    {
        // Toggle the visibility of the debug form when F1 is pressed
        if (Input.GetKeyDown(KeyCode.F1))
        {
            gameDataForm.ToggleDebugWindow();
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            MouseSimulator.DrawCursor();
        }
    }

    // Once a second, Gather game state data
    private IEnumerator ReportGameStateToNeuro()
    {
        while (true)
        {
            // Gather game state data
            StateReporter.Instance.UpdateGameStateData();

            bool canAct = StateReporter.Instance.CanNeuroActRightNow();

            // TODO - send context to neuro? Might want to do that more often than just when actions change though so she is more aware of the timer?
            // TODO - also need to consider if there are special conditions that cause custom context (like game start, game end, first city, first market...)
            // TODO - am i sending context before or after? context should maybe consider actions, so after probably...
            if (canAct)
            {
                // Get the current possible game actions, and check if they have changed from the ones Neuro has available already
                var possibleActions = actionManager.GetPossibleActions();
                if (DidPossibleActionsChange(possibleActions))
                {
                    // If there are new actions, prepare the action window for Neuro
                    PrepareActionWindow(possibleActions);
                }
            }
            else
            {
                UnregisterActionWindow();
            }

            yield return new WaitForSeconds(1f);
        }
    }
    
    private bool DidPossibleActionsChange(ActionManager.PossibleActions possibleActions)
    {
        if (neuroCurrentActions.Actions == null || neuroCurrentActions.Actions.Count != possibleActions.Actions.Count)
        {
            return true;
        }

        if (currentActionWindow.CurrentState == ActionWindow.State.Ended)
        {
            return true;
        }

        // Compare each action in the list to see if there any difference between them
        for (int i = 0; i < possibleActions.Actions.Count; i++)
        {
            if (!neuroCurrentActions.Actions[i].Equals(possibleActions.Actions[i]))
            {
                return true; // New action found
            }
        }

        return false;
    }

    void OnGUI()
    {
        gameDataForm.Draw();
        MouseSimulator.DrawCursor();
    }

    private void UnregisterActionWindow()
    {
        if (currentActionWindow != null && currentActionWindow.CurrentState != ActionWindow.State.Ended)
        {
            currentActionWindow.End();
        }
    }

    private void PrepareActionWindow(ActionManager.PossibleActions actionsInfo)
    {
        // Remember the new actions so we can compare and see when they've changed
        neuroCurrentActions = actionsInfo;

        // Make sure to disable previous action window if it is still active
        UnregisterActionWindow();

        // Create a new action window and set the context if provided
        currentActionWindow = ActionWindow.Create(this.gameObject);
        if (!actionsInfo.Context.IsNullOrEmpty())
        {
            currentActionWindow.SetContext(actionsInfo.Context, actionsInfo.IsContextSilent);
        }

        if (actionsInfo.IsForcedAction)
        {
            currentActionWindow.SetForce(0, "(There are optional actions you can do right now but have limited time to do)", "", true);
        }

        // Add the possible actions to the action window
        foreach (INeuroAction action in actionsInfo.Actions)
        {
            currentActionWindow.AddAction(action);
        }

        // Register the action window so it can be used by Neuro. Note it is only relevant if there are any actions
        if (actionsInfo.Actions.Count > 0)
        {
            currentActionWindow.Register();
        }
    }
}
