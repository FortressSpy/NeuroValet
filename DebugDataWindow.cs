using GameViews.Departure.SubViews;
using Inklewriter;
using System;
using System.Linq;
using System.Text;
using UnityEngine;

public class DebugDataWindow
{
    private Game.Player.IPlayer player;
    private Game.Game gameInfo;
    private GameData.Clock clock;
    private Inklewriter.Story story;

    private bool showWindow = true; // Tracks whether the window is visible
    private Rect windowRect = new Rect(20, 20, 950, 750); // Default size

    // Add scroll positions for scrollable areas
    private Vector2 textDataScrollPos = Vector2.zero;
    private Vector2 choicesScrollPos = Vector2.zero;
    private Vector2 luggageScrollPos = Vector2.zero;

    public void ToggleDebugWindow()
    {
        showWindow = !showWindow;
    }

    public void SetGameInfo(Game.Game game, GameData.Clock clock)
    {
        gameInfo = game;
        this.clock = clock;
    }

    public void SetPlayer(Game.Player.IPlayer playerData)
    {
        player = playerData;
    }

    public void SetStory(Inklewriter.Story story)
    {
        this.story = story;
    }

    public void Draw()
    {
        if (showWindow)
        {
            windowRect = GUI.Window(1234, windowRect, DrawWindow, "");
        }
    }

    private void DrawWindow(int windowID)
    {
        // Draw the title with a custom style
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("NeuroValet", GetTitleStyle());
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(10); // Add some spacing below the title

        GUILayout.BeginHorizontal();

        // Left Section: Player State and Journey State
        GUILayout.BeginVertical(GUILayout.Width(300));
        GUILayout.Label("<b>Player State</b>", GetLabelStyle());
        GUILayout.Label(GetPlayerStateData(), GetInfoAreaStyle(), GUILayout.ExpandHeight(true));

        GUILayout.Space(10);

        GUILayout.Label("<b>Journey State</b>", GetLabelStyle());
        GUILayout.Label(GetJourneyStateData(), GetInfoAreaStyle(), GUILayout.ExpandHeight(true));
        GUILayout.EndVertical();

        // Middle Section: Game Data, Text Data, and Market
        GUILayout.BeginVertical(GUILayout.Width(300));
        GUILayout.Label("<b>Game Data</b>", GetLabelStyle());
        GUILayout.Label(GetGameData(), GetInfoAreaStyle(), GUILayout.Height(150));

        GUILayout.Space(5);

        GUILayout.Label("<b>Text Data</b>", GetLabelStyle());
        textDataScrollPos = GUILayout.BeginScrollView(textDataScrollPos, GUILayout.Height(250));
        GUILayout.Label(GetStoryData(), GetInfoAreaStyle(), GUILayout.ExpandHeight(true));
        GUILayout.EndScrollView();

        GUILayout.Space(3);

        GUILayout.Label("<b>Market</b>", GetLabelStyle());
        GUILayout.Label("Market data not yet implemented.", GetInfoAreaStyle(), GUILayout.ExpandHeight(true));
        GUILayout.EndVertical();

        // Right Section: Available Choices and Luggage
        GUILayout.BeginVertical(GUILayout.Width(300));
        GUILayout.Label("<b>Available Choices</b>", GetLabelStyle());
        choicesScrollPos = GUILayout.BeginScrollView(choicesScrollPos, GUILayout.Height(350));
        GUILayout.TextArea(GetChoicesData(), GetChoicesAreaStyle(), GUILayout.ExpandHeight(true));
        GUILayout.EndScrollView();

        GUILayout.Space(10);

        GUILayout.Label("<b>Luggage</b>", GetLabelStyle());
        luggageScrollPos = GUILayout.BeginScrollView(luggageScrollPos, GUILayout.Height(250));
        GUILayout.Label(GetLuggageData(), GetInfoAreaStyle(), GUILayout.ExpandHeight(true));
        GUILayout.EndScrollView();

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        GUI.DragWindow();
    }

    private string GetPlayerStateData()
    {
        if (player == null)
            return "Player data not available.";

        return $"Current Character: {player.currentCharacter}\n" +
               $"Health: {player.health}\n" +
               $"HealthInInk: {player.healthInInk}\n" +
               $"Money: {player.money.poundsFloat}\n" +
               $"Current City: {player.currentCity?.name ?? "Unknown"}\n" +
               $"Next City: {player.nextScheduledCity?.name ?? "Unknown"}\n" +
               $"Ready for Story: {player.readyForStory}\n" +
               $"Story Content Seen: {player.storyContentSeenForExplore}\n" +
               $"In London: {player.isInLondonAtStartOfGame}\n" +
               $"Back in London: {player.backInLondon}";
    }
    private string GetChoicesData()
    {
        if (player == null)
            return "Choices not available.";

        StringBuilder availableActions = new StringBuilder();
        availableActions.Append($"Can Attend: {player.canAttendToFogg}\n");
        availableActions.Append($"Can beg for money: {player.canBeg}\n");
        availableActions.Append($"Can Skip Hour: {player.canSkipHour}\n");
        availableActions.Append($"Can stay at hotel: {player.canStayInHotel}\n");
        availableActions.Append($"Can Visit Market: {player.currentCityHasMarket}\n");
        availableActions.Append($"Can Visit Bank: {player.bankIsAvailable}\n");

        // Note player.canConverseToday throws if this condition is not met
        if (this.player.converseCharacterData != null)
        {
            availableActions.Append($"Can Converse: {player.canConverseToday}\n");
            availableActions.Append($"Converse Characters: {string.Join(", ", player.converseCharacterData.Select(c => c.Key))}");
            availableActions.Append("\n");
        }
        else
        {
            availableActions.Append("Can Converse: false\n");
        }

        availableActions.Append($"Available destination cities: \n");
        foreach (var city in player.availableDestinationCities)
        {
            availableActions.Append($" {city.name}");
        }
        availableActions.Append("\n");

        availableActions.Append($"Current available journeys:\n");
        foreach (var journey in player.currentAvailableJourneys)
        {
            availableActions.Append($" {journey.displayName} ({journey.name})[{journey.routeName}] " +
                $"via: {string.Join(", ", journey.viaCities.Select(c => c.viaCity.displayName))}\n");
        }        
        availableActions.Append("\n");

        return availableActions.ToString();
    }

    private string GetJourneyStateData()
    {
        if (player == null)
            return "Journey data not available.";

        StringBuilder journeysData = new StringBuilder();

        journeysData.Append($"Available Cities: {player.availableDestinationCities.Count}\n");
        journeysData.Append($"Custom Journeys: {gameInfo.customJourneys.customJourneys.Count}\n");
        journeysData.Append($"Number of Completions: {gameInfo.numberOfCompletions}\n");

        journeysData.Append($"Journeys Now: {player.journeysAvailableToLeaveNow.Count}\n");
        foreach (var journey in player.journeysAvailableToLeaveNow)
        {
            journeysData.Append($" {journey.displayName} ({journey.name})[{journey.routeName}]\n");
        }
        journeysData.Append("\n");

        journeysData.Append($"Journeys Later: {player.journeysAvailableToLeaveLater.Count}\n");
        foreach (var journey in player.journeysAvailableToLeaveLater)
        {
            journeysData.Append($" {journey.displayName} ({journey.name})[{journey.routeName}]\n");
        }
        journeysData.Append("\n");

        return journeysData.ToString();
    }

    private string GetStoryData()
    {
        if (story == null)
            return "Story data not available.";

        StringBuilder storyData = new StringBuilder();
        storyData.Append($"Story Name: {story.name}\n");
        storyData.Append($"Finished: {story.IsFinished}\n");
        storyData.Append($"Num Of Chunks: {story.state?.NumberOfChunks ?? -1}\n");

        storyData.Append($"Latest chunk options count: {story.state?.LatestChunk?.Options?.Count ?? 0}\n");
        for (int i = 0; i < story.state?.LatestChunk?.Options?.Count; i++)
        {
            storyData.Append($"  Option {i}: {story.state?.LatestChunk?.Options[i].title}\n");
        }

        return storyData.ToString();
    }

    private string GetLuggageData()
    {
        if (player == null)
            return "Luggage data not available.";
        StringBuilder luggageInfo = new StringBuilder();

        for (int i = 0; i < player.suitcases.Count; i++)
        {
            luggageInfo.Append($"Suitcase {i + 1}:\n");
            for (int j = 0; j < player.suitcases[i].contents.Count; j++)
            {
                var contents = player.suitcases[i].contents[j];
                luggageInfo.Append($"  {contents.quantity} ");
                luggageInfo.Append($"{contents.item.displayName} ({contents.item.nameOfSet})");

                luggageInfo.Append("[");
                if (contents.isValuableHere) luggageInfo.Append("V");
                if (contents.item.canBeUsed) luggageInfo.Append("U");
                luggageInfo.Append("]\n");
            }
        }
        return luggageInfo.ToString();
    }

    private string GetGameData()
    {
        if (gameInfo == null)
            return "Game data not available.";

        return
            $"Date: {player.dayNumberAdjustedForDateLine}. Time: {clock.currentTime.ToString()}, TimeOfDay: {clock.currentTimeOfDay.ToString()}\n" +
            $"Prologue Active: {gameInfo.prologueActive}\n" +
            $"Epilogue Active: {gameInfo.epilogueActive}\n" +
            $"Begun Game: {gameInfo.hasBegunGame}, Zoomed Out: {gameInfo.hasBegunGameAndVisitedMarketAndZoomedOut}," +
            $" Displayed routes: {gameInfo.hasBegunGameAndVisitedMarketAndZoomedOutAndFinishedDisplayingRoutes} \n";
    }

    private GUIStyle GetTitleStyle()
    {
        var style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleCenter; // Center the title
        style.fontSize = 24; // Increase font size
        style.fontStyle = FontStyle.Bold; // Make it bold
        style.normal.textColor = Color.white; // Set text color to white
        return style;
    }

    private GUIStyle GetLabelStyle()
    {
        var style = new GUIStyle(GUI.skin.label);
        style.richText = true; // Enable rich text for bold labels
        style.fontSize = 20; // Increase font size
        return style;
    }

    private GUIStyle GetInfoAreaStyle()
    {
        var style = new GUIStyle(GUI.skin.textArea);
        style.fontSize = 18; // Increase font size
        style.wordWrap = true;
        return style;
    }
    private GUIStyle GetChoicesAreaStyle()
    {
        var style = new GUIStyle(GUI.skin.textArea);
        style.fontSize = 18; // Increase font size
        return style;
    }
}
