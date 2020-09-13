using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Cyberization
{
    public interface ISolarFlareListener
    {
        void SolarFlare(bool solarFlare);
    }

    public static class SolarFlareHandler
    {
        private static readonly HashSet<ISolarFlareListener> _listeners = new HashSet<ISolarFlareListener>();
        private static bool? _solarFlareActive;
    
        public static void Add(ISolarFlareListener listener)
        {
            _listeners.Add(listener);
        }

        public static void Remove(ISolarFlareListener listener)
        {
            _listeners.Remove(listener);
            
        }

        private static void Update(bool solarFlare)
        {
            _solarFlareActive = solarFlare;
            _listeners.Do(listener => listener.SolarFlare(solarFlare));
        }

        public static bool IsSolarFlareActive()
        {
            if (_solarFlareActive == null)
            {
                _solarFlareActive = Find.World.gameConditionManager.ConditionIsActive(GameConditionDefOf.SolarFlare);
            }
            return _solarFlareActive.Value;
        }
        
        [HarmonyPatch(typeof(GameCondition), nameof(GameCondition.Init))]
        static class Patch_GameCondition_Init
        {
            [HarmonyPostfix]
            static void UpdateSolarFlareVulnerableParts(GameCondition __instance)
            {
                if(__instance.def == GameConditionDefOf.SolarFlare)
                    Update(true);
            }
        }

        [HarmonyPatch(typeof(GameCondition), nameof(GameCondition.End))]
        static class Patch_GameCondition_End
        {
            [HarmonyPostfix]
            static void UpdateSolarFlareVulnerableParts(GameCondition __instance)
            {
                if(__instance.def == GameConditionDefOf.SolarFlare)
                    Update(false);
            }
        }

        [HarmonyPatch(typeof(SavedGameLoaderNow), nameof(SavedGameLoaderNow.LoadGameFromSaveFileNow))]
        private static class Patch_OnLoad
        {
            [HarmonyPostfix]
            private static void ClearCachedSolarFlareState()
            {
                _solarFlareActive = null;
            } 
        }
    }
}