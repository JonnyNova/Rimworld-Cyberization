using System.Collections.Generic;
using System.Linq;
using FrontierDevelopments.General.Energy;
using HarmonyLib;
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
                var total = PawnPartPowerNet.Get(pawn).TotalAvailable;
                if (total <= 0) return 1f;
                return total;
            }
        }

        public override float CurLevel => PawnPartPowerNet.Get(pawn).AmountAvailable;

        public bool CanBeSatisfied => PawnPartPowerNet.Get(pawn).MaxRate > 0;

        public bool SeekSatisfaction => CurLevelPercentage <= Mod.Settings.SeekPowerPercent;

        // Needed since power can tick before charging happens leaving a pawn unable to charge to 100%
        public bool Satisfied => CurLevelPercentage >= (Mod.Settings.SeekPowerChargeTo >= 1f ? 0.99f : Mod.Settings.SeekPowerChargeTo);

        public bool Empty => CurLevel <= 0;

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
            return apparel.AllComps.OfType<IEnergyProvider>().Any();
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
                var net = PawnPartPowerNet.Get(___pawn);

                if (nd != CyberizationDefOf.PartEnergy) return __result;

                return net.Nodes.Any();
            }
        }
    }
}