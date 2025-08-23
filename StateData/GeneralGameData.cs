using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroValet.StateData
{
    internal struct GeneralGameData
    {
        public string CurrentTime { get; set; }
        public int CurrentDayNumber { get; set; }
        public string DayOfWeek { get; set; }
        public bool IsNightTime { get; set; }
        public bool DidPassMidpoint { get; set; }

        // game provided summary of current date and position (journey/city)
        public string CurrentSituation { get; set; } 

        public bool IsPrologueActive { get; set; }
        public bool IsEpilogueActive { get; set; }
        public bool DidWin { get; set; }
        public float MoneySpentDuringGame { get; set; }
    }
}
