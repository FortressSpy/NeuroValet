
namespace NeuroValet.StateData
{
    internal struct PlayerData
    {
        public bool HasData { get; set; }
        public string PlayerName { get; set; }
        public int CurrentHealth { get; set; }
        public float CurrentMoneyInPounds { get; set; }
        public bool IsInLondonAtStartOfGame { get; set; }
        public bool BackInLondon { get; set; }
        public bool IsWithFogg { get; set; }
        public bool CanSkipTime { get; set; }
    }
}
