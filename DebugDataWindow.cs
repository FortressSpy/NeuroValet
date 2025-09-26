using GameViews.BottomNav;
using NeuroValet;
using System;
using System.Text;
using UnityEngine;

public class DebugDataWindow
{
    private bool showWindow = false; // Tracks whether the window is visible
    private Rect windowRect = new Rect(20, 20, 1000, 800); // Default size

    // Add scroll positions for scrollable areas
    private Vector2 worldwideRoutesScroll = Vector2.zero;
    private Vector2 currentRoutesScroll = Vector2.zero;
    private Vector2 luggageScrollPos = Vector2.zero;

    public bool showCurrentRouteFullDebugInfo = false;

    public void ToggleDebugWindow()
    {
        showWindow = !showWindow;
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
        GUILayout.BeginVertical(GUILayout.Width(480));

        showCurrentRouteFullDebugInfo = GUILayout.Toggle(showCurrentRouteFullDebugInfo, "Show Route Debug data");

        if (StateReporter.Instance.CurrentStateData.Journey.HasActiveJourney)
        {
            GUILayout.Label("<b>Active Journey</b>", GetLabelStyle());
            currentRoutesScroll = GUILayout.BeginScrollView(currentRoutesScroll, GUILayout.Height(480));
            GUILayout.Label(GetActiveJourney(), GetInfoAreaStyle(), GUILayout.ExpandHeight(true));
            GUILayout.EndScrollView();
        }
        else
        {
            GUILayout.Label("<b>Current Routes</b>", GetLabelStyle());
            currentRoutesScroll = GUILayout.BeginScrollView(currentRoutesScroll, GUILayout.Height(480));
            GUILayout.Label(GetCurrentRoutes(), GetInfoAreaStyle(), GUILayout.ExpandHeight(true));
            GUILayout.EndScrollView();
        }

        GUILayout.Space(5);
        GUILayout.Label("<b>New Routes</b>", GetLabelStyle());
        GUILayout.Label(GetNewRoutes(), GetInfoAreaStyle(), GUILayout.ExpandHeight(true));

        GUILayout.EndVertical();

        // Middle Section: Game Data, Text Data, and Market
        GUILayout.BeginVertical(GUILayout.Width(480));
        GUILayout.Label("<b>Worldwide Routes</b>", GetLabelStyle());
        worldwideRoutesScroll = GUILayout.BeginScrollView(worldwideRoutesScroll, GUILayout.Height(380));
        GUILayout.Label(GetAvailableWorldWideRoutes(), GetInfoAreaStyle(), GUILayout.ExpandHeight(true));
        GUILayout.EndScrollView();

        GUILayout.Space(5);
        GUILayout.Label("<b>Views</b>", GetLabelStyle());
        GUILayout.Label(GetViewsData(), GetInfoAreaStyle(), GUILayout.ExpandHeight(true));

        GUILayout.Space(5);
        GUILayout.Label("<b>Historic Path</b>", GetLabelStyle());
        GUILayout.Label(GetVisitedCities(), GetInfoAreaStyle(), GUILayout.ExpandHeight(true));

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        // Allow the window to be dragged
        GUI.DragWindow();
    }

    private string GetAvailableWorldWideRoutes()
    {
        var knownRoutes = StateReporter.Instance.CurrentStateData.Journey.KnownRoutesWorldwide;
        if (knownRoutes == null) return "";

        StringBuilder journeysData = new StringBuilder();
        foreach (var journey in knownRoutes)
        {
            journeysData.Append($"{journey}\n");
            journeysData.Append("=============================\n");
        }

        return journeysData.ToString();
    }

    private string GetCurrentRoutes()
    {
        var knownRoutes = StateReporter.Instance.CurrentStateData.Journey.RoutesFromCurrentCity;
        if (knownRoutes == null) { return ""; }

        StringBuilder journeysData = new StringBuilder();
        foreach (var journey in knownRoutes)
        {
            if (showCurrentRouteFullDebugInfo)
            {
                journeysData.Append(journey.DebugText);
            }
            else
            {
                journeysData.Append(journey.FullContext);
            }
            journeysData.Append("\n===============================\n");
        }

        return journeysData.ToString();
    }

    private string GetActiveJourney()
    {
        var journey = StateReporter.Instance.CurrentStateData.Journey;

        StringBuilder journeyData = new StringBuilder();
        journeyData.AppendLine(journey.ActiveJourney.Name);
        journeyData.AppendLine($"Passed {journey.ActiveJourneyProgress.ToString("P2")}. Departed on Day {journey.ActiveJourneyDepartedOnDay} and will arrive on Day {journey.ActiveJourneyArrivalDay}\n");
        journeyData.AppendLine(journey.ActiveJourney.DebugText);

        return journeyData.ToString();
    }

    private string GetVisitedCities()
    {
        var knownRoutes = StateReporter.Instance.CurrentStateData.Journey.CitiesPassed;
        if (knownRoutes == null) return "";

        return string.Join("->", knownRoutes);
    }

    private string GetNewRoutes()
    {
        var knownRoutes = StateReporter.Instance.CurrentStateData.Journey.NewRoutesBeingRevealed;
        if (knownRoutes == null) return "";

        StringBuilder journeysData = new StringBuilder();

        foreach (var journey in knownRoutes)
        {
            journeysData.Append($"{journey}");
            journeysData.Append("================================\n");
        }

        return journeysData.ToString();
    }
    private string GetViewsData()
    {
        var gameViews = GameViews.Static.gameViews;
        if (gameViews == null)
            return "Game views not available.";

        StringBuilder availableViews = new StringBuilder();

        availableViews.Append(gameViews.bottomNavView.isVisible ? "Bottom Nav\n" : "");
        availableViews.Append((gameViews.bottomNavView.foggPanelView as FoggPanelView)?.showingSpeechBubble ?? false ? "Fogg Panel\n" : "");
        availableViews.Append(gameViews.cameraControlsView.isVisible ? "Camera Control\n" : "");
        availableViews.Append(gameViews.clockView.isVisible ? "Clock View\n" : "");
        availableViews.Append(gameViews.cloudView.isVisible ? "Cloud View\n" : "");
        availableViews.Append(gameViews.converseView.isVisible ? "Converse\n" : "");
        availableViews.Append(gameViews.creditsView.isVisible ? "Credits\n" : "");
        availableViews.Append(gameViews.departureView.isVisible ? "Departure\n" : "");
        availableViews.Append(gameViews.fadeOutView.isVisible ? "Fade Out\n" : "");
        availableViews.Append(gameViews.fullScreenQuestionView.isVisible ? "Full Screen Question\n" : "");
        availableViews.Append(gameViews.infoCardView.isVisible ? "Info Card\n" : "");
        availableViews.Append(gameViews.introView.isVisible ? "Intro\n" : "");
        availableViews.Append(gameViews.largeTitleView.isVisible ? $"Large Title: {gameViews.largeTitleView.titleText}\n" : "");
        availableViews.Append(gameViews.marketAndLuggageView.isVisible ? "Market & Luggage\n" : "");
        availableViews.Append(gameViews.overviewView.isVisible ? "Overview\n" : "");
        availableViews.Append(gameViews.screenshotView.isVisible ? "Screenshot\n" : "");
        availableViews.Append(gameViews.settingsView.isVisible ? "Settings\n" : "");
        availableViews.Append(gameViews.storyView.isVisible ? "Story\n" : "");

        return availableViews.ToString();
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
        style.richText = true; // Enable rich text for bold labels
        style.fontSize = 18; // Increase font size
        style.wordWrap = true;
        return style;
    }
}
