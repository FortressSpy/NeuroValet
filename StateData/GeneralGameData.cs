namespace NeuroValet.StateData
{
    internal struct GeneralGameData
    {
        #region Time variables
        public string CurrentTime { get; set; }
        public int CurrentDayNumber { get; set; }
        public string DayOfWeek { get; set; }
        public bool IsNightTime { get; set; }
        public bool DidPassMidpoint { get; set; }
        #endregion 

        #region CurrentSitutation
        // game provided summary of current date and position (journey/city)
        public string CurrentSituation { get; set; } 
        public bool IsRevealingNewRoutes { get; set; }
        #endregion

        #region Game Start/End
        public bool IsPrologueActive { get; set; }
        public bool IsEpilogueActive { get; set; }
        public bool DidWin { get; set; }
        public float MoneySpentDuringGame { get; set; }
        #endregion
    }
}
