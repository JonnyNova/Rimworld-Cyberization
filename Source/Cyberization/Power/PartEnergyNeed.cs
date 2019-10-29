using System.Collections.Generic;
using System.Linq;
using Harmony;
using RimWorld;
using UnityEngine;
using Verse;

namespace FrontierDevelopments.Cyberization.Power
{
    public class PartEnergyNeed : Need
    {
        public static bool HasNeed(Pawn pawn)
        {
            return pawn.needs.TryGetNeed<PartEnergyNeed>() != null;
        }

        public PartEnergyNeed(Pawn pawn) : base(pawn)
        {
        }
        
        public override float MaxLevel
        {
            get
            {
                var total = PowerProvider.Providers(pawn).Aggregate(0L, (sum, provider) => sum + provider.MaxEnergy);
                if (total <= 0) return 1f;
                return total;
            }
        }
        
        public override float CurLevel => 
            PowerProvider.Providers(pawn).Aggregate(0L, (sum, provider) => sum + provider.Energy);

        public bool CanBeSatisfied => PowerProvider.Providers(pawn).Any();

        public bool SeekSatisfaction => CurLevelPercentage <= Mod.Settings.SeekPowerPercent;

        public bool Satisfied => CurLevelPercentage >= Mod.Settings.SeekPowerChargeTo;

        public override bool ShowOnNeedList => Mod.Settings.UsePartPower;

        public override void NeedInterval()
        {
        }

        public override void DrawOnGUI(
            Rect rect,
            int maxThresholdMarkers = 2147483647,
            float customMargin = -1,
            bool drawArrows = true,
            bool doTooltip = true)
        {
            threshPercents = new List<float>(new []{ Mod.Settings.SeekPowerPercent, Mod.Settings.SeekPowerChargeTo });
            base.DrawOnGUI(rect, maxThresholdMarkers, customMargin, drawArrows, doTooltip);
        }
    }

    static class PartEnergyNeedFromApparel
    {
        private static bool ApparelHasSource(Apparel apparel)
        {
            return apparel.AllComps.OfType<IPowerProvider>().Any();
        }

        private static void HandleApparelChange(Apparel apparel, Pawn pawn)
        {
            if (ApparelHasSource(apparel))
            {
                pawn.needs.AddOrRemoveNeedsAsAppropriate();
            }
        }

        [HarmonyPatch(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Notify_ApparelAdded))]
        static class ApparelAdded
        {
            [HarmonyPostfix]
            static void OnAdded(Pawn_ApparelTracker __instance, Apparel apparel)
            {
                HandleApparelChange(apparel, __instance.pawn);
            }
        }
        
        [HarmonyPatch(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Notify_ApparelRemoved))]
        static class ApparelRemoved
        {
            [HarmonyPostfix]
            static void OnRemoved(Pawn_ApparelTracker __instance, Apparel apparel)
            {
                HandleApparelChange(apparel, __instance.pawn);
            }
        }

        [HarmonyPatch(typeof(Pawn_NeedsTracker), "ShouldHaveNeed")]
        static class ShouldHaveNeed
        {
            [HarmonyPostfix]
            static bool AddEnergySourceCheck(bool __result, Pawn_NeedsTracker __instance, NeedDef nd, Pawn ___pawn)
            {
                if (nd != CyberizationDefOf.PartEnergy) return __result;
                return PowerProvider.Providers(___pawn).Any() || PowerConsumer.All(___pawn).Any();
            }
        }
    }
}