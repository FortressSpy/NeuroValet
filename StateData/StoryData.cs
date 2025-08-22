using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroValet.StateData
{
    internal struct StoryData
    {
        internal struct ChoiceData
        {
            public string ChoiceText { get; set; }
            public int ChoiceIndex { get; set; }
            public bool IsContinueChoice { get; set; }
        }

        public bool IsVisible { get; set; }
        public string StoryName { get; set; }
        public string Text { get; set; }
        public List<ChoiceData> Choices { get; set; }
    }
}
