using GameViews.BottomNav;
using NeuroValet;
using System;
using System.Text;
using UnityEngine;

public class DebugDataWindow
{
    private bool showWindow = true; // Tracks whether the window is visible
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

        //GUILayout.Space(5);

        //GUILayout.Label("<b>Text Data</b>", GetLabelStyle());
        //GUILayout.Label(GetStoryData(), GetInfoAreaStyle(), GUILayout.ExpandHeight(true));

        //GUILayout.Space(3);

        //GUILayout.Label("<b>Market</b>", GetLabelStyle());
        //GUILayout.Label(GetMarketData(), GetInfoAreaStyle(), GUILayout.ExpandHeight(true));
        GUILayout.EndVertical();

        // Right Section: Available Choices and Luggage
        //GUILayout.BeginVertical(GUILayout.Width(300));
        //GUILayout.Label("<b>Available Choices</b>", GetLabelStyle());
        //choicesScrollPos = GUILayout.BeginScrollView(choicesScrollPos, GUILayout.Height(350));
        //GUILayout.TextArea(GetChoicesData(), GetChoicesAreaStyle(), GUILayout.ExpandHeight(true));
        //GUILayout.EndScrollView();

        //GUILayout.Space(2);

        //GUILayout.Label("<b>Luggage</b>", GetLabelStyle());
        //luggageScrollPos = GUILayout.BeginScrollView(luggageScrollPos, GUILayout.Height(100));
        //GUILayout.Label(GetLuggageData(), GetInfoAreaStyle(), GUILayout.ExpandHeight(true));
        //GUILayout.EndScrollView();

        //GUILayout.EndVertical();

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

    //private string GetMarketData()
    //{
    //    if (player == null)
    //        return "Market data not available.";
    //    else if (!player.currentCityHasMarket)
    //        return "City does not have a market.";
    //    else if (player.currentCity?.name != "London" && player.marketIsShut)
    //        return "Market is currently shut.";

    //    var market = player.marketForCurrentCity;
    //    StringBuilder marketReport = new StringBuilder();
    //    marketReport.Append($"Market Name: {player.currentCity.market.marketName}\n");
    //    if (market.sellsCases)
    //    {
    //        var marketCasePrice = GameData.Static.markets.PriceOfSuitcaseInMarket(player.currentCity);
    //        bool canBuy = player.money > marketCasePrice;

    //        marketReport.Append($"Buy case for {marketCasePrice.pounds}£.\n");
    //    }
    //    else
    //    {
    //        marketReport.Append("Market does not sell cases.\n");
    //    }

    //    marketReport.Append($"Items: \n");
    //    foreach (var item in market.items)
    //    {
    //        bool canBuy = player.CanBuyItemAtPrice(item.item, item.price);

    //        GUILayout.BeginHorizontal();
    //        GUI.enabled = canBuy; // Enable or disable the button based on the condition
    //        if (GUILayout.Button($"Buy {item.item.name} - Price: {item.price.pounds}", GUILayout.Width(300)))
    //        {
    //            player.SlotsAvailableInSuitcaseAtIndex(0);
    //            player.BuyItemFromMarket(item.item, item.price, player.suitcases[0], new SuitcasePosition(0,0)); // TODO figure out open slots
    //        }
    //        GUI.enabled = true; // Re-enable GUI for other elements
    //        GUILayout.EndHorizontal();
    //    }

    //    return marketReport.ToString();
    //}

    //private string GetLuggageData()
    //{
    //    if (player == null)
    //        return "Luggage data not available.";
    //    StringBuilder luggageInfo = new StringBuilder();

    //    for (int i = 0; i < player.suitcases.Count; i++)
    //    {
    //        luggageInfo.Append($"Suitcase {i + 1}:\n");
    //        for (int j = 0; j < player.suitcases[i].contents.Count; j++)
    //        {
    //            var contents = player.suitcases[i].contents[j];
    //            luggageInfo.Append($"  {contents.quantity} ");
    //            luggageInfo.Append($"{contents.item.displayName} ({contents.item.nameOfSet})");

    //            luggageInfo.Append("[");
    //            if (contents.isValuableHere) luggageInfo.Append("V");
    //            if (contents.item.canBeUsed) luggageInfo.Append("U");
    //            luggageInfo.Append("]\n");
    //        }
    //    }
    //    return luggageInfo.ToString();
    //}


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
