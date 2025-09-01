using BepInEx.Configuration;
using GameData;
using HarmonyLib;
using System;
using System.Collections;
using System.Reflection;

namespace NeuroValet.Overrides
{
    public static class ClockOverrides
    {
        public static void Initialize(ConfigFile config, BepInEx.Logging.ManualLogSource logger)
        {
            // Load configuration entry for multiplier
            var slowTickClockMultiplier = config.Bind(
                "NeuroValet",
                "defaultMultiplier",
                1.5f, // Default value
                "Float Multiplier for Clock timer outside special scenarios, I.E mostly while viewing the map or a city (1.0 = 30 seconds per hour)\n" +
                "This multiplier is annoying, and will only effect new runs."
            );
            // Load configuration entry for multiplier
            var marketClockMultiplier = config.Bind(
                "NeuroValet",
                "MarketClockMultiplier",
                2.5f, // Default value
                "Float Multiplier for Clock timer while in a market (1.0 = 15 seconds per hour)"
            );

            PassTimeAtSpeedPatch.Initialize(slowTickClockMultiplier, marketClockMultiplier, logger);
            PassTimeUntilGameTimePatch.Initialize(slowTickClockMultiplier, marketClockMultiplier, logger);
        }
    }

    [HarmonyPatch(typeof(Clock))]
    [HarmonyPatch("PassTimeAtSpeed")]
    [HarmonyPatch(new Type[] { typeof(float), typeof(ClockLayer) })]
    public static class PassTimeAtSpeedPatch
    {
        private static ConfigEntry<float> SlowTickClockMultiplier;
        private static ConfigEntry<float> MarketClockMultiplier;
        private static BepInEx.Logging.ManualLogSource Logger;

        public static void Initialize(ConfigEntry<float> slowTickClockMultiplier, ConfigEntry<float> marketClockMultiplier, BepInEx.Logging.ManualLogSource logger)
        {
            SlowTickClockMultiplier = slowTickClockMultiplier;
            MarketClockMultiplier = marketClockMultiplier;
            Logger = logger;
        }

        static bool Prefix(Clock __instance, ref float realSecondsPerHour, ClockLayer layer)
        {
            if (layer == ClockLayer.MarketTick)
            {
                realSecondsPerHour *= MarketClockMultiplier.Value;
            }
            else if (layer == ClockLayer.SlowTick)
            {
                realSecondsPerHour *= SlowTickClockMultiplier.Value;
            }

            // Log information about the clock layer and target time
            PrintTimestack(__instance);
            //Logger?.LogDebug($"New Clock layer: {layer}, Real seconds per hour: {realSecondsPerHour}\n");

            return true;
        }

        private static void PrintTimestack(Clock __instance)
        {
            // Get the private field
            Type clockType = typeof(Clock);
            Type timeStackElementType = clockType.GetNestedType("TimeStackElement", BindingFlags.NonPublic);
            FieldInfo timeStackField = clockType.GetField("timeStack", BindingFlags.NonPublic | BindingFlags.Instance);
            var timeStack = (IDictionary)timeStackField.GetValue(__instance);

            Type elementType = clockType.GetNestedType("TimeStackElement", BindingFlags.NonPublic);

            foreach (DictionaryEntry kvp in timeStack)
            {
                var layer = kvp.Key; // ClockLayer
                var element = kvp.Value; // TimeStackElement (boxed)

                //Logger.LogDebug($"Layer: {layer}");

                foreach (var field in elementType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    var value = field.GetValue(element);
                    //Logger.LogDebug($"  {field.Name} = {value}");
                }
            }
        }
    }


    [HarmonyPatch(typeof(Clock))]
    [HarmonyPatch("PassTimeUntilGameTime")]
    [HarmonyPatch(new Type[] { typeof(Utils.Time), typeof(float), typeof(ClockLayer) })]
    public static class PassTimeUntilGameTimePatch
    {
        private static ConfigEntry<float> SlowTickClockMultiplier;
        private static ConfigEntry<float> MarketClockMultiplier;
        private static BepInEx.Logging.ManualLogSource Logger;

        public static void Initialize(ConfigEntry<float> slowTickClockMultiplier, ConfigEntry<float> marketClockMultiplier, BepInEx.Logging.ManualLogSource logger)
        {
            SlowTickClockMultiplier = slowTickClockMultiplier;
            MarketClockMultiplier = marketClockMultiplier;
            Logger = logger;
        }

        static bool Prefix(Clock __instance, Utils.Time targetTime, ref float realSecondsPerHour, ClockLayer layer)
        {
            if (layer == ClockLayer.MarketTick)
            {
                realSecondsPerHour *= MarketClockMultiplier.Value;
            }
            else if (layer == ClockLayer.SlowTick)
            {
                realSecondsPerHour *= SlowTickClockMultiplier.Value;
            }

            // Log information about the clock layer and target time
            PrintTimestack(__instance);
            //Logger?.LogDebug($"New Clock layer: {layer}, Target time: {targetTime}, Real seconds per hour: {realSecondsPerHour}\n");

            return true;
        }

        private static void PrintTimestack(Clock __instance)
        {
            // Get the private field
            Type clockType = typeof(Clock);
            Type timeStackElementType = clockType.GetNestedType("TimeStackElement", BindingFlags.NonPublic);
            FieldInfo timeStackField = clockType.GetField("timeStack", BindingFlags.NonPublic | BindingFlags.Instance);
            var timeStack = (IDictionary)timeStackField.GetValue(__instance);

            Type elementType = clockType.GetNestedType("TimeStackElement", BindingFlags.NonPublic);

            foreach (DictionaryEntry kvp in timeStack)
            {
                var layer = kvp.Key; // ClockLayer
                var element = kvp.Value; // TimeStackElement (boxed)

                //Logger.LogDebug($"Layer: {layer}");

                foreach (var field in elementType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    var value = field.GetValue(element);
                    //Logger.LogDebug($"  {field.Name} = {value}");
                }
            }
        }
    }
}
