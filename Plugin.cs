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
    internal static new ManualLogSource Logger;

    private IPlayer player;
    private Game.Game gameInfo;
    private ConversationTopic conversationTopic;
        
    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    private void Start()
    {
        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} Starting!");

        player = FindObjectOfType<Player>();
        gameInfo = FindObjectOfType<Game.Game>();
        StartCoroutine(PeriodicLogger());
    }

    private IEnumerator PeriodicLogger()
    {
        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} Periodic!");
        while (true)
        {
            if (player != null)
            {
                Logger.LogInfo($"Prologue active: {gameInfo.prologueActive}");
                Logger.LogInfo($"Prologue complete: {gameInfo.epilogueActive}");
                Logger.LogInfo($"Prologue complete: {gameInfo.store.isActiveAndEnabled}");

                Logger.LogInfo($"Active Journey: {JsonUtility.ToJson(player.activeJourney)}");
                Logger.LogInfo($"Current City: {JsonUtility.ToJson(player.currentCity)}");
                Logger.LogInfo($"Player Health: {player.health}. HealthInInk: {player.healthInInk}. Money: {player.money.poundsFloat}");
                Logger.LogInfo($"Next City: {JsonUtility.ToJson(player.nextScheduledCity)}. Is Night: {player.nightActive}");
                Logger.LogInfo($"Available Cities: {player.availableDestinationCities.Count}");
                Logger.LogInfo($"Available Journeys Now {player.journeysAvailableToLeaveNow.Count}");
                Logger.LogInfo($"First available journey now {player.journeysAvailableToLeaveNow.FirstOrDefault()?.viaCities.FirstOrDefault()?.viaCity.name ?? ""}");
                Logger.LogInfo($"Available Journeys Later {player.journeysAvailableToLeaveLater.Count}");
                Logger.LogInfo($"First available journey Later {player.journeysAvailableToLeaveLater.FirstOrDefault()?.viaCities.FirstOrDefault()?.viaCity.name ?? ""}");
                Logger.LogInfo($"Ready for story: {player.readyForStory}. StoryContentSeenForExplore {player.storyContentSeenForExplore}");
                Logger.LogInfo($"London: {player.isInLondonAtStartOfGame}, back in london: {player.backInLondon}");
                Logger.LogInfo("------------------------------------");
            }
            else
            {
                Logger.LogWarning("Player not found!");
                player = FindObjectOfType<Player>();
            }
            yield return new WaitForSeconds(5f);
        }
    }
}
