using System;
using System.Linq;
using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace FrontierDevelopments.Cyberization.Power
{
    public class CaravanPartEnergy
    {
        private static void TrySatisfyPowerNeed(Pawn pawn, Caravan caravan)
        {
            try
            {
                var charger = ChargeSourceUtility
                    .FindSources(CaravanInventoryUtility.AllInventoryItems(caravan), true)
                    .Where(source => source.Available)
                    .OrderByDescending(source => source.RateAvailable)
                    .First();
                charger.Charge(pawn);
            }
            catch (InvalidOperationException)
            {
            }
        }

        [HarmonyPatch(typeof(Caravan_NeedsTracker), "TrySatisfyPawnNeeds", new []{ typeof(Pawn) })]
        static class Patch
        {
            [HarmonyPostfix]
            static void SatisfyPowerNeed(Pawn pawn, Caravan ___caravan)
            {
                if (pawn.Dead) return;
                if (PartEnergyNeed.HasNeed(pawn))
                {
                    TrySatisfyPowerNeed(pawn, ___caravan);
                }
            }
        }
    }
}