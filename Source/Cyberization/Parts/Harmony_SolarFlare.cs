using System.Collections.Generic;
using System.Linq;
using Harmony;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Cyberization.Parts
{
    public class Harmony_GameCondition
    {
        private static bool PawnHasVulnerablePart(Pawn pawn)
        {
            return HediffsToUpdate(pawn).Any();
        }

        private static IEnumerable<HediffWithComps> HediffsToUpdate(Pawn pawn)
        {
            return pawn.health.hediffSet.hediffs
                .OfType<HediffWithComps>()
                .Where(hediff => hediff.TryGetComp<AddedPartSolarFlareVulnerability>() != null);
        }
        
        private static IEnumerable<Pawn> GetPawnsToNotify(GameCondition condition)
        {
            return condition.AffectedMaps
                .SelectMany(map => map.listerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.Pawn)))
                .OfType<Pawn>()
                .Where(pawn => PawnHasVulnerablePart(pawn));
        }

        [HarmonyPatch(typeof(GameCondition), nameof(GameCondition.Init))]
        static class Patch_Init
        {
            [HarmonyPostfix]
            static void UpdateSolarFlareVulnerableParts(GameCondition __instance)
            {
                if(__instance.def == GameConditionDefOf.SolarFlare)
                    GetPawnsToNotify(__instance).Do(pawn => HediffsToUpdate(pawn).Do(hediff => pawn.health.Notify_HediffChanged(hediff)));
            }
        }

        [HarmonyPatch(typeof(GameCondition), nameof(GameCondition.End))]
        static class Patch_End
        {
            [HarmonyPostfix]
            static void UpdateSolarFlareVulnerableParts(GameCondition __instance)
            {
                if(__instance.def == GameConditionDefOf.SolarFlare)
                    GetPawnsToNotify(__instance).Do(pawn => HediffsToUpdate(pawn).Do(hediff => pawn.health.Notify_HediffChanged(hediff)));
            }
        }
    }
}