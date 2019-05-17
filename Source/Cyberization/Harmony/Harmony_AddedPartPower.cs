using System.Collections.Generic;
using System.Linq;
using Harmony;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Cyberization.Harmony
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

        private static int GetSorting(AddedPartPowerConsumer consumer)
        {
            var ordering = consumer.parent.Part.def.tags
                .Select(tag => PartOrdering.FirstIndexOf(a => a == tag))
                .ToList();
            return (!ordering.NullOrEmpty() ? ordering.First() : 100) * consumer.Props.priority;
        }
        
        private static IEnumerable<AddedPartPowerConsumer> PriorityOrder(Pawn pawn)
        {
            var result = AddedPartPowerConsumer.All(pawn).ToList();
            if (result.NullOrEmpty()) return new List<AddedPartPowerConsumer>();
            result.Sort((left, right) => GetSorting(left).CompareTo(GetSorting(right)));
            return result;
        }

        private static void TickPowerProviders(Pawn pawn)
        {
            PowerProvider.Providers(pawn)?.Do(provider => provider.Tick());
        }

        private static void TickPowerConsumers(Pawn pawn)
        {
            // TODO load balance power among same priority
            PriorityOrder(pawn)?.Do(part => part.Tick());
        }

        [HarmonyPatch(typeof(Pawn), nameof(Pawn.Tick))]
        static class CalculateAddedPartPower
        {
            [HarmonyPostfix]
            static void Postfix(Pawn __instance)
            {
                TickPowerProviders(__instance);
                TickPowerConsumers(__instance);
            }
        }
    }
}