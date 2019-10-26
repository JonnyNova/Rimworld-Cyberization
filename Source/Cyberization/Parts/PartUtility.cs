using System;
using System.Collections.Generic;
using System.Linq;
using FrontierDevelopments.Cyberization.Power;
using Harmony;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Cyberization.Parts
{
    public static class PartUtility
    {
        public static IEnumerable<Hediff_AddedPart> PoweredPartsFor(Pawn pawn)
        {
            return pawn.health.hediffSet.hediffs
                .OfType<Hediff_AddedPart>()
                .Where(part => part.comps.Any(comp => comp is PartPowerConsumer));
        }
        
        public static bool HasPoweredParts(Pawn pawn)
        {
            return PoweredPartsFor(pawn).Any();
        }

        private static bool ToggleAndCheckParts(Pawn pawn, Func<bool> callback)
        {
            var parts = PoweredPartsFor(pawn).ToList();
            
            // turn off all parts
            // checks if things such as both kidneys would be impacted
            parts
                .SelectMany(part => part.comps)
                .OfType<PartPowerConsumer>()
                .Do(comp => comp.OverridePowered(false));
            pawn.health.capacities.Notify_CapacityLevelsDirty();

            var result = callback();
            
            // turn on all parts
            parts
                .SelectMany(part => part.comps)
                .OfType<PartPowerConsumer>()
                .Do(comp => comp.OverridePowered(true));
            pawn.health.capacities.Notify_CapacityLevelsDirty();

            return result;
        }

        public static bool RequiresPowerToLive(Pawn pawn)
        {
            return ToggleAndCheckParts(pawn, () => pawn.health.ShouldBeDeadFromRequiredCapacity() != null);
        }

        public static bool RequiresPowerForMovement(Pawn pawn)
        {
            return ToggleAndCheckParts(pawn, () => !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving));
        }
    }
}