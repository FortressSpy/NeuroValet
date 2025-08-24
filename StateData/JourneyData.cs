using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroValet.StateData
{
    internal struct Journey
    {
        public float DistanceFromLondon { get; set; }
        public float DistanceToLondon { get; set; }
        public float RouteLength { get; set; }
        public float RouteDuration { get; set; }
        public float Cost { get; set; }
        public string StartCity { get; set; }
        public string TargetCity { get; set; }
    }

    internal struct JourneyData
    {
        public List<Journey> NewRoutesBeingRevealed { get; set; }
        public List<Journey> KnownRoutesWorldwide { get; set; }
        public List<Journey> RoutesFromCurrentCity { get; set; }
        public List<Journey> RoutesPassed { get; set; }
    }
}
