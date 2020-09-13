using System.Collections.Generic;
using System.Linq;
using FrontierDevelopments.Cyberization.Power;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Cyberization.Parts
{
    public class Harmony_AddedPartPower
    {
        private static readonly BodyPartTagDef[] PartOrdering = 
        {
            // brain
            BodyPartTagDefOf.ConsciousnessSource,
            // heart
            BodyPartTagDefOf.BloodPumpingSource,
            //lungs
            BodyPartTagDefOf.BreathingSource,
            BodyPartTagDefOf.BreathingPathway,
            BodyPartTagDefOf.BreathingSourceCage,
            // filtration
            BodyPartTagDefOf.BloodFiltrationLiver,
            BodyPartTagDefOf.BloodFiltrationKidney,
            BodyPartTagDefOf.BloodFiltrationSource,
            // stomach
            BodyPartTagDefOf.MetabolismSource,
            // spine and pelvis
            BodyPartTagDefOf.Spine,
            BodyPartTagDefOf.Pelvis,
            // legs
            BodyPartTagDefOf.MovingLimbDigit,
            BodyPartTagDefOf.MovingLimbSegment,
            BodyPartTagDefOf.MovingLimbCore,
            // eyes
            BodyPartTagDefOf.SightSource,
            // arms
            BodyPartTagDefOf.ManipulationLimbDigit,
            BodyPartTagDefOf.ManipulationLimbSegment,
            BodyPartTagDefOf.ManipulationLimbCore,
            // ears
            BodyPartTagDefOf.HearingSource,
            // jaw
            BodyPartTagDefOf.EatingSource,
            BodyPartTagDefOf.EatingPathway,
            BodyPartTagDefOf.TalkingSource,
            BodyPartTagDefOf.TalkingPathway
        };

        private static int GetSorting(IPowerConsumer consumer)
        {
            var ordering = consumer.Tags
                .Select(tag => PartOrdering.FirstIndexOf(a => a == tag))
                .ToList();
            return (!ordering.NullOrEmpty() ? ordering.First() : 100) * consumer.Priority;
        }
        
        private static IEnumerable<IPowerConsumer> PriorityOrder(Pawn pawn)
        {
            var result = PawnPartPowerNet.Get(pawn)?.Nodes.OfType<IPowerConsumer>().ToList();
            if (result.NullOrEmpty()) return new List<IPowerConsumer>();
            result.Sort((left, right) => GetSorting(left).CompareTo(GetSorting(right)));
            return result;
        }

        [HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn), new []{ typeof(PawnGenerationRequest) })]
        static class Patch_GeneratePawn
        {
            [HarmonyPostfix]
            static Pawn GeneratePartPowerProviderIfNeeded(Pawn __result)
            {
                var net = PawnPartPowerNet.Get(__result);
                if (net.Nodes.OfType<IPowerConsumer>().Any() && net.MaxRate <= 0)
                {
                    __result.health.AddHediff(CyberizationDefOf.BionicEnergyCell, __result.RaceProps.body.corePart);
                }
                return __result;
            }
        }
    }
}