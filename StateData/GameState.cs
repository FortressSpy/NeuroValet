using NeuroValet.ViewsParsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroValet.StateData
{
    internal struct GameStateData
    {
        // TODO Implement all these structs (and any others that are needed)
        // public PlayerData Player { get; set; }
        // public GameData BasicData { get; set; }
        // public ClockData Clock { get; set; }
        // public LuggageData Luggage { get; set; }
        // public JourneyData Journey { get; set; }
        public StoryData Story { get; set; }
    }

    internal class GameState
    {
        // Most data is gathered from game object classes, with the exception of Story data which is parsed from the StoryView.
        StoryViewParser storyViewParser;

        public GameState()
        {
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
