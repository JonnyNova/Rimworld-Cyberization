using System;
using System.Collections.Generic;
using System.Linq;
using Harmony;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Cyberization.Parts
{
    public static class PartUtility
    {
        public static IEnumerable<Hediff_AddedPart> AddedParts(Pawn pawn)
        {
            return pawn.health.hediffSet.hediffs
                .OfType<Hediff_AddedPart>();
        }

        public static IEnumerable<Hediff_AddedPart> PartsWithFeature(Pawn pawn, Type feature)
        {
            return AddedParts(pawn)
                .Where(part => part.comps.Any(comp => comp.GetType() == feature));
        }

        public static IEnumerable<Hediff_AddedPart> PoweredPartsFor(Pawn pawn)
        {
            return PartsWithFeature(pawn, typeof(AddedPartPowerConsumer));
        }

        public static IEnumerable<Hediff_AddedPart> MaintainablePartsFor(Pawn pawn)
        {
            return PartsWithFeature(pawn, typeof(AddedPartMaintenance));
        }

        private static bool PartNeedsMaintenance(Hediff_AddedPart part)
        {
            if (part.TryGetComp<AddedPartMaintenance>()?.NeedsMaintenance ?? false) return true;
            if (part.TryGetComp<AddedPartBreakdownable>()?.IsBrokenDown ?? false) return true;
            return false;
        }

        public static IEnumerable<Hediff_AddedPart> PartsNeedingMaintenance(Pawn pawn)
        {
            return AddedParts(pawn).Where(PartNeedsMaintenance);
        }

        public static IEnumerable<Hediff_AddedPart> PartsNeedingMaintenanceUrgent(Pawn pawn)
        {
            return PartsNeedingMaintenance(pawn).Where(RequiresPartToLive<AddedPartMaintenance>);
        }

        public static bool NeedMaintenance(Pawn pawn)
        {
            return PartsNeedingMaintenance(pawn).Any();
        }

        public static bool NeedMaintenanceUrgent(Pawn pawn)
        {
            return PartsNeedingMaintenanceUrgent(pawn).Any();
        }

        public static bool HasPoweredParts(Pawn pawn)
        {
            return PoweredPartsFor(pawn).Any();
        }

        private static bool ToggleAndCheckPart<T>(Hediff_AddedPart part, Func<bool> callback)  where T : HediffComp, AddedPartEffectivenessModifier
        {
            part.TryGetComp<T>().SetDisabled(true);
            part.pawn.health.capacities.Notify_CapacityLevelsDirty();

            var result = callback();

            part.TryGetComp<T>().SetDisabled(false);
            part.pawn.health.capacities.Notify_CapacityLevelsDirty();

            return result;
        }

        private static bool ToggleAndCheckParts<T>(Pawn pawn, Func<bool> callback) where T : HediffComp, AddedPartEffectivenessModifier
        {
            var parts = PoweredPartsFor(pawn).ToList();
            
            // turn off all parts
            // checks if things such as both kidneys would be impacted
            parts
                .Select(part => part.TryGetComp<T>())
                .Do(comp => comp.SetDisabled(true));
            pawn.health.capacities.Notify_CapacityLevelsDirty();

            var result = callback();
            
            // turn on all parts
            parts
                .Select(part => part.TryGetComp<T>())
                .Do(comp => comp.SetDisabled(false));
            pawn.health.capacities.Notify_CapacityLevelsDirty();

            return result;
        }

        public static bool RequiresPartsToLive<T>(Pawn pawn) where T : HediffComp, AddedPartEffectivenessModifier
        {
            return ToggleAndCheckParts<T>(pawn, () => pawn.health.ShouldBeDeadFromRequiredCapacity() != null);
        }

        public static bool RequiresPartsForMovement<T>(Pawn pawn) where T : HediffComp, AddedPartEffectivenessModifier
        {
            return ToggleAndCheckParts<T>(pawn, () => !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving));
        }

        public static bool RequiresPartToLive<T>(Hediff_AddedPart part) where T : HediffComp, AddedPartEffectivenessModifier
        {
            return ToggleAndCheckPart<T>(part, () => part.pawn.health.ShouldBeDeadFromRequiredCapacity() != null);
        }

        public static bool RequiresPartToMove<T>(Hediff_AddedPart part) where T : HediffComp, AddedPartEffectivenessModifier
        {
            return ToggleAndCheckPart<T>(part, () => !part.pawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving));
        }
    }
}