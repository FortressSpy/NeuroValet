using Game.Player;
using GameResources.MapData;
using GameResources.MapData.RouteFinder;
using GameViews.Globe.Impl;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;

namespace NeuroValet.StateData
{
    internal struct Journey
    {
        public string Name { get; private set; }
        public string RouteName { get; private set; }
        public float Cost { get; private set; }
        public string StartCity { get; private set; }
        public ICityInfo DestinationCity { get; private set; }
        public string ViaCities { get; private set; }
        public string DepartTime { get; private set; } = string.Empty;
        public string ArrivalTime { get; private set; } = string.Empty;
        public bool CanDepartRightNow { get; private set; }
        public bool IsCheap { get; private set; }
        public bool IsExpensive { get; private set; }
        public bool IsRough { get; private set; }
        public bool IsSlow { get; private set; }
        public bool IsFast { get; private set; }
        public bool IsLimitedLuggage { get; private set; }
        public string TransportType { get; private set; }
        public string KnownCluesAboutDestination { get; private set; }

        public string MinimalContext { get; private set; }
        public string GlobeViewContext { get; private set; }
        public string FullContext { get; private set; }
        public string DebugText { get; private set; }

        private static string CalcDistanceInMiles(float surfaceDistance)//Vector2 playerCoordinates, Vector2 cityCoordinates)
        {
            float num = GlobeMaths.SurfaceDistanceInMiles(surfaceDistance);//GlobeMaths.SurfaceDistanceBetweenCoordinates(playerCoordinates, this.cityInfo.location));
            num = num.RoundToSig(2);
            string text = string.Format("{0:N0}", num);
            text += " miles away";
            return text;
        }

        public Journey(IJourneyInfo j)
        {
            var player = Game.Static.player;

            // TODO - do I need to save journey info in here so I can select the route in the globe view? maybe need to save destination city?
            Name = j.displayName;
            RouteName = j.routeName;
            StartCity = j.startCity.displayName;
            DestinationCity = j.destinationCity;
            KnownCluesAboutDestination = (player.cluesByCity != null) ? player.cluesByCity.TryGetValue(DestinationCity, out var clues) ? string.Join("; ", clues.Select(c => c.text)) : "" : "";
            ViaCities = string.Join(", ", j.viaCities.Select(c => $"{c.viaCity.displayName}"));
            DepartTime = player?.DescriptionOfDaysUntilNextSailingOf(j) ?? "";
            CanDepartRightNow = player?.journeysAvailableToLeaveNow.Contains(j) ?? false;
            Cost = GameData.Static.journeyData.CostOfTicketForJourney(j).pounds;
            IsCheap = j.isCheap;
            IsExpensive = j.isExpensive;
            IsRough = j.isRoughGoing;
            IsFast = j.isFast;
            IsSlow = j.isSlow;
            IsLimitedLuggage = j.hasLimitedLuggage;
            TransportType = j.info.transportCategory;

            // TODO - arrival time is using routefinder, which is only available when selecting a city apparently.
            // TODO - This kinda means to have enough data she'll have to click on cities and go through all of the routes
            // TODO - consider just moving this data to full context, drop globeviewcontext, and add depart time in minimal context.
            var routeToDestination = player.routeFinder?.RouteTo(DestinationCity);
            if (routeToDestination != null) 
            {
                ArrivalTime = routeToDestination.approximated 
                    ? TextGen.Calendar.ApproximateNumberOfDaysInWords(routeToDestination.arrivalDayNumber) 
                    : TextGen.Calendar.ArrivalDay(routeToDestination.arrivalDayNumber, Game.Static.player.dayOfWeek);
            }

            DebugText = $"Name: {Name}. Route Name: {RouteName}. Unspecifically named: {j.unspecificallyNamed}\n" +
                $"{StartCity}->{DestinationCity.displayName}. {(CanDepartRightNow ? "READY TO DEPART NOW" : "")}\n" +
                $"{(IsCheap ? "Cheap " : "")}{(IsExpensive ? "Expensive " : "")}{(IsRough ? "Rough " : "")}{(IsFast ? "Fast " : "")}{(IsSlow ? "Slow " : "")}{(IsLimitedLuggage ? "Limited Luggage " : "")}{(j.info.isFlexiblyTimed ? "Flexible Time " : "")}\n" +
                $"Via cities: \n{string.Join("\n", j.viaCities.Select(c => $"({c.viaCity.displayName}. On Day {c.onDay}. Surface Distance: {CalcDistanceInMiles(c.surfaceDistance)}. Distance East of London: {CalcDistanceInMiles(c.viaCity.distanceEastOfLondon)})"))}\n" +
                $"Cities on journey: \n{string.Join(",", j.citiesOnJourney.Select(c => $"({c.displayName}. DOeL {CalcDistanceInMiles(c.distanceEastOfLondon)})"))}\n" +
                $"INFO:\nDepart Time: {DepartTime}. Departures per week: {j.info.departuresPerWeek}. Banned day: {j.info.bannedDepartureDay}\n" +
                $"{ArrivalTime} Journey Set: {j.info.journeySet}.\n" +
                $"Cost: £{Cost.ToString()}. Luggage: [slots: {j.info.luggage.luggageSlots}. limited: {j.info.luggage.hasLimitedLuggage}. max increase {j.info.luggage.maxIncreases}. cost: {j.info.luggage.extraLuggage.cost}. extra capacity: {j.info.luggage.extraLuggage.extraCapacity}]\n" +
                $"Destination Clues: {KnownCluesAboutDestination}\n" +
                $"Transport: [type: {j.info.transportType}, category: {TransportType}, medium: {j.info.transportMedium} size: {j.info.transportSize}]]\n";
            
            MinimalContext = $"{Name}. {TransportType} going from {StartCity} to {DestinationCity.displayName}" +
                $"{(ViaCities.IsNullOrEmpty() ? "" : $", via: [{ViaCities}]")}";
            GlobeViewContext = MinimalContext + $"\nDepart {DepartTime}, {ArrivalTime} and Cost: £{Cost.ToString()}";
            FullContext = $"{GlobeViewContext}" +
                $"{(IsLimitedLuggage ? $"\nHas space for {j.info.luggage.luggageSlots} suitcases. Can buy extra space for £{j.info.luggage.extraLuggage.cost}." : "")}" +
                $"{(!KnownCluesAboutDestination.IsNullOrEmpty() ? $"\nRumours about {DestinationCity.displayName}: {KnownCluesAboutDestination}." : "")}";
            // TODO - any additional depart screen info, for example can't take due to money or luggage limit? Maybe extra space only if needed
        }
    }
    
    internal struct JourneyData
    {
        public List<Journey> RoutesFromCurrentCity { get; set; }
        public List<string> NewRoutesBeingRevealed { get; set; }
        public List<string> KnownRoutesWorldwide { get; set; }
        public List<string> CitiesPassed { get; set; }

        public bool HasActiveJourney { get; set; } 
        public Journey ActiveJourney { get; set; }
        public float ActiveJourneyProgress { get; set; }
        public int ActiveJourneyArrivalDay { get; set; }
        public int ActiveJourneyDepartedOnDay { get; set; }

        public JourneyData()
        {
            RoutesFromCurrentCity = new List<Journey>();
            NewRoutesBeingRevealed = new List<string>();
            KnownRoutesWorldwide = new List<string>();
            CitiesPassed = new List<string>();
        }
    }
}
