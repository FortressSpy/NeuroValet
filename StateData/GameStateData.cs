namespace NeuroValet.StateData
{
    internal struct GameStateData
    {
        // TODO Implement all these structs (and any others that are needed)
        // TODO - figure out how Fogg's dialogue is counted and how to add it. probably need it as part of context. Though there is also its 'negotiate' action
        public GeneralGameData GeneralData { get; set; }
        public PlayerData Player { get; set; }
        public CityData City { get; set; }
        // public ClockData Clock { get; set; }
        // public LuggageData Luggage { get; set; }
        public JourneyData Journey { get; set; }
    }
}
