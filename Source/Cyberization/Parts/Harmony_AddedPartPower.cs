using System.Collections.Generic;
using System.Linq;
using FrontierDevelopments.Cyberization.Power;
using Harmony;
using RimWorld;
using RimWorld.Planet;
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
            var result = PowerConsumer.All(pawn).ToList();
            if (result.NullOrEmpty()) return new List<IPowerConsumer>();
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
            PriorityOrder(pawn)?.Do(part => part.PowerTick());
        }

        [HarmonyPatch(typeof(Pawn), nameof(Pawn.Tick))]
        static class CalculateAddedPartPower
        {
            [HarmonyPostfix]
            static void Postfix(Pawn __instance)
            {
                // Only tick power if the pawn can fill the need
                if (__instance.Spawned || __instance.IsCaravanMember())
                {
                    TickPowerProviders(__instance);
                    TickPowerConsumers(__instance);
                }
            }
        }

        [HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn), new []{ typeof(PawnGenerationRequest) })]
        static class Patch_GeneratePawn
        {
            [HarmonyPostfix]
            static Pawn GeneratePartPowerProviderIfNeeded(Pawn __result)
            {
                if (PowerConsumer.All(__result).Any() && !PowerProvider.Providers(__result).Any())
                {
                    __result.health.AddHediff(CyberizationDefOf.BionicEnergyCell, __result.RaceProps.body.corePart);
                }
                return __result;
            }
        }
    }
}