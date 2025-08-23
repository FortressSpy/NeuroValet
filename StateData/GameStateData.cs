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
        public GeneralGameData GeneralData { get; set; }
        public PlayerData Player { get; set; }
        public CityData City { get; set; }
        // public ClockData Clock { get; set; }
        // public LuggageData Luggage { get; set; }
        // public JourneyData Journey { get; set; }
    }
}
