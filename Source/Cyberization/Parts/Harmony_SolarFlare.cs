using System.Collections.Generic;
using System.Linq;
using Harmony;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Cyberization.Parts
{
    public class Harmony_GameCondition
    {
        private static IEnumerable<AddedPartSolarFlareVulnerability> PartsToUpdate(ICollection<Map> maps)
        {
            return maps
                .SelectMany(map => map.listerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.Pawn)))
                .OfType<Pawn>()
                .SelectMany(pawn => pawn.health.hediffSet.hediffs)
                .OfType<HediffWithComps>()
                .SelectMany(hediff => hediff.comps)
                .OfType<AddedPartSolarFlareVulnerability>();
        }

        [HarmonyPatch(typeof(GameCondition), nameof(GameCondition.Init))]
        static class Patch_Init
        {
            [HarmonyPostfix]
            static void UpdateSolarFlareVulnerableParts(GameCondition __instance)
            {
                if(__instance.def == GameConditionDefOf.SolarFlare)
                    PartsToUpdate(__instance.AffectedMaps).Do(part => part.SolarFlare(true));
            }
        }

        [HarmonyPatch(typeof(GameCondition), nameof(GameCondition.End))]
        static class Patch_End
        {
            [HarmonyPostfix]
            static void UpdateSolarFlareVulnerableParts(GameCondition __instance)
            {
                if(__instance.def == GameConditionDefOf.SolarFlare)
                    PartsToUpdate(__instance.AffectedMaps).Do(part => part.SolarFlare(false));
            }
        }
    }
}