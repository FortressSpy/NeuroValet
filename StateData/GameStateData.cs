namespace NeuroValet.StateData
{
    internal struct GameStateData
    {
        public GeneralGameData GeneralData { get; set; }
        public PlayerData Player { get; set; }
        public CityData City { get; set; }
        public JourneyData Journey { get; set; }
    }
}
