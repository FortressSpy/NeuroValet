using NeuroValet.StateData;
using System;

namespace NeuroValet
{
    /// <summary>
    /// This class is responsible for parsing game state data and reporting it back.
    /// </summary>
    internal class StateReporter
    {
        // Implement a singleton pattern for the StateReporter
        private static readonly Lazy<StateReporter> _instance = new Lazy<StateReporter>(() => new StateReporter());
        public static StateReporter Instance => _instance.Value;

        private GameStateData currentStateData;

        private StateReporter()
        {
            CurrentStateData = new GameStateData();
        }

        public GameStateData CurrentStateData { get => currentStateData; private set => currentStateData = value; }

        public void UpdateGameStateData()
        {
            currentStateData.GeneralData = GetGeneralData();
            currentStateData.Player = GetPlayerData();
            currentStateData.City = GetCityData();
            // currentStateData.BasicData = GetBasicData();
            // currentStateData.Clock = GetClockData();
            // currentStateData.Luggage = GetLuggageData();
            // currentStateData.Journey = GetJourneyData();
        }

        private GeneralGameData GetGeneralData()
        {
            var game = Game.Static.game;
            var player = Game.Static.player;
            var clock = GameData.Static.clock;

            return new GeneralGameData
            {
                CurrentTime = clock != null ? $"{clock.currentTime.hoursPart.hours}:{clock.currentTime.minutesPart.minutes}" : "",
                CurrentDayNumber = player?.dayNumberAdjustedForDateLine ?? 0,
                DayOfWeek = player?.dayOfWeekName,
                IsNightTime = player?.nightActive ?? false,
                DidPassMidpoint = player != null && player.dateLineCrossedAndReported,

                CurrentSituation = player?.descriptionOfSituation ?? "",

                IsPrologueActive = game != null && game.prologueActive,
                IsEpilogueActive = game != null && game.epilogueActive,
                DidWin = player?.won ?? false,
                MoneySpentDuringGame = player?.amountSpentOnTrip.poundsFloat ?? 0f
            };
        }

        private PlayerData GetPlayerData()
        {
            var player = Game.Static.player;

            PlayerData playerData = new PlayerData();
            if (player == null)
            {
                playerData.HasData = false;
            }
            else
            {
                playerData.HasData = true;
                playerData.PlayerName = player.currentCharacter;
                playerData.CurrentHealth = player.health;
                playerData.CurrentMoneyInPounds = player.money.poundsFloat;
                playerData.IsInLondonAtStartOfGame = player.isInLondonAtStartOfGame;
                playerData.BackInLondon = player.backInLondon;
                playerData.IsWithFogg = !player.withoutFogg;
            }

            return playerData;
        }

        private CityData GetCityData()
        {
            var player = Game.Static.player;
            var city = player?.currentCity;

            CityData cityData = new CityData();

            if (city == null)
            {
                cityData.IsInCity = false;
            }
            else
            {
                cityData.IsInCity = true;
                cityData.CityName = city.displayName;
                cityData.HasMarket = city.hasMarket;
                cityData.HasBank = city.hasBank;
                cityData.IsFakeCity = city.isFakeCity;
                cityData.BankTransferInProgress = player.bankTransferInProgress;
                cityData.BankFundsReady = player.bankFundsReady;
            }
            return cityData;
        }
    }
}
