using BepInEx.Logging;
using NeuroValet.StateData;
using NeuroValet.ViewsParsers;

namespace NeuroValet
{
    /// <summary>
    /// This class is responsible for parsing game state data and reporting it back.
    /// </summary>
    internal class StateReporter
    {
        private ManualLogSource logger;

        // Most data is gathered from game object classes, with the exception of Story data which is parsed from the StoryView.
        StoryViewParser storyViewParser;

        public StateReporter(ManualLogSource logger)
        {
            this.logger = logger;
            storyViewParser = StoryViewParser.Instance;
        }

        public GameStateData GetGameStateData()
        {
            GameStateData gameStateData = new GameStateData
            {
                // Player = GetPlayerData(),
                // BasicData = GetBasicData(),
                // Clock = GetClockData(),
                // Luggage = GetLuggageData(),
                // Journey = GetJourneyData(),
                Story = new StoryData
                {
                    IsVisible = storyViewParser.IsStoryVisible(),
                    StoryName = storyViewParser.GetStoryName(),
                    Text = storyViewParser.GetStoryText(),
                    Choices = storyViewParser.GetAvailableChoices()
                }
            };
            return gameStateData;
        }
    }
}
