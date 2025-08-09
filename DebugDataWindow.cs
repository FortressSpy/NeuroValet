using UnityEngine;

public class DebugDataWindow
{
    private Game.Player.IPlayer player;
    private Game.Game gameInfo;
    private bool showWindow = true; // Tracks whether the window is visible
    private Rect windowRect = new Rect(20, 20, 950, 600); // Default size

    public void ToggleDebugWindow()
    {
        showWindow = !showWindow;
    }

    public void SetGameInfo(Game.Game game)
    {
        gameInfo = game;
    }

    public void SetPlayer(Game.Player.IPlayer playerData)
    {
        player = playerData;
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

        GUILayout.Space(10);

        GUILayout.Label("<b>Luggage</b>", GetLabelStyle());
        GUILayout.Label("Luggage data not yet implemented.", GetInfoAreaStyle(), GUILayout.ExpandHeight(true));
        GUILayout.EndVertical();

        // Middle Section: Game Data and Text Data
        GUILayout.BeginVertical(GUILayout.Width(300));
        GUILayout.Label("<b>Game Data</b>", GetLabelStyle());
        GUILayout.Label(GetGameData(), GetInfoAreaStyle(), GUILayout.Height(150));

        GUILayout.Space(10);

        GUILayout.Label("<b>Text Data</b>", GetLabelStyle());
        GUILayout.Label("Text data not yet implemented.", GetInfoAreaStyle(), GUILayout.ExpandHeight(true));
        GUILayout.EndVertical();

        // Right Section: Available Choices and Market
        GUILayout.BeginVertical(GUILayout.Width(300));
        GUILayout.Label("<b>Available Choices</b>", GetLabelStyle());
        GUILayout.TextArea("Choices data not yet implemented.", GetChoicesAreaStyle(), GUILayout.Height(150));

        GUILayout.Space(10);

        GUILayout.Label("<b>Market</b>", GetLabelStyle());
        GUILayout.Label("Market data not yet implemented.", GetInfoAreaStyle(), GUILayout.ExpandHeight(true));
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        GUI.DragWindow();
    }

    private string GetPlayerStateData()
    {
        if (player == null)
            return "Player data not available.";

        return $"Health: {player.health}\n" +
               $"HealthInInk: {player.healthInInk}\n" +
               $"Money: {player.money.poundsFloat}\n" +
               $"Current City: {player.currentCity?.name ?? "Unknown"}\n" +
               $"Next City: {player.nextScheduledCity?.name ?? "Unknown"}\n" +
               $"Ready for Story: {player.readyForStory}\n" +
               $"Story Content Seen: {player.storyContentSeenForExplore}\n" +
               $"In London: {player.isInLondonAtStartOfGame}\n" +
               $"Back in London: {player.backInLondon}";
    }

    private string GetJourneyStateData()
    {
        if (player == null)
            return "Journey data not available.";

        return $"Available Cities: {player.availableDestinationCities.Count}\n" +
               $"Journeys Now: {player.journeysAvailableToLeaveNow.Count}\n" +
               $"Journeys Later: {player.journeysAvailableToLeaveLater.Count}";
    }

    private string GetGameData()
    {
        if (gameInfo == null)
            return "Game data not available.";

        return $"Prologue Active: {gameInfo.prologueActive}\n" +
               $"Epilogue Active: {gameInfo.epilogueActive}\n";
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
