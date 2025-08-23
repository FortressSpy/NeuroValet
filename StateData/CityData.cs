using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroValet.StateData
{
    internal class CityData
    {
        public bool IsInCity { get; set; }
        public string CityName { get; set; }
        public bool HasMarket { get; set; }
        public bool HasBank { get; set; }
        public bool IsFakeCity { get; set; }
        public bool BankTransferInProgress { get; set; }
        public bool BankFundsReady { get; set; }
    }
}
