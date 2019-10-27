using System;
using System.Linq;
using Harmony;
using RimWorld.Planet;
using Verse;

namespace FrontierDevelopments.Cyberization.Power
{
    public class CaravanPartEnergy
    {
        private static void TrySatisfyPowerNeed(Pawn pawn, PartEnergyNeed need, Caravan caravan)
        {
            try
            {
                var providers = PowerProvider.Providers(pawn).ToList();
                var charger = ChargeSourceUtility
                    .FindSources(CaravanInventoryUtility.AllInventoryItems(caravan), true)
                    .Where(source => source.Available)
                    .OrderByDescending(source => source.RateAvailable(providers))
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
                var need = pawn.needs.TryGetNeed<PartEnergyNeed>();
                if (need != null)
                {
                    TrySatisfyPowerNeed(pawn, need, ___caravan);
                }
            }
        }
    }
}