using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
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
            if (part.TryGetComp<AddedPartDamageable>()?.NeedsRepair ?? false) return true;
            return false;
        }

        public static IEnumerable<AddedPartBreakdownable> PartsNeedingBreakdownRepair(Pawn pawn)
        {
            return pawn.health.hediffSet.hediffs
                .OfType<HediffWithComps>()
                .SelectMany(hediff => hediff.comps)
                .OfType<AddedPartBreakdownable>()
                .Where(part => part.IsBrokenDown);
        }

        public static IEnumerable<AddedPartDamageable> PartsNeedingDamageRepair(Pawn pawn)
        {
            return AddedParts(pawn)
                .SelectMany(hediff => hediff.comps)
                .OfType<AddedPartDamageable>()
                .Where(part => part.NeedsRepair);
        }

        public static IEnumerable<AddedPartMaintenance> PartsNeedingRoutineMaintenance(Pawn pawn)
        {
            return AddedParts(pawn)
                .SelectMany(hediff => hediff.comps)
                .OfType<AddedPartMaintenance>()
                .Where(part => part.NeedsMaintenance);
        }

        public static IEnumerable<Hediff_AddedPart> PartsNeedingAnyMaintenance(Pawn pawn)
        {
            return AddedParts(pawn).Where(PartNeedsMaintenance);
        }

        public static IEnumerable<Hediff_AddedPart> PartsNeedingAnyMaintenanceUrgent(Pawn pawn)
        {
            return PartsNeedingAnyMaintenance(pawn).Where(RequiresPartToLive<AddedPartMaintenance>);
        }

        public static bool NeedMaintenance(Pawn pawn)
        {
            return PartsNeedingAnyMaintenance(pawn).Any();
        }

        public static bool NeedMaintenanceUrgent(Pawn pawn)
        {
            return PartsNeedingAnyMaintenanceUrgent(pawn).Any();
        }

        public static bool HasPoweredParts(Pawn pawn)
        {
            return PoweredPartsFor(pawn).Any();
        }

        private static bool CheckFeature<T>(
            T feature,
            bool? state,
            Func<bool> callback
            ) where T : HediffComp, AddedPartEffectivenessModifier
        {
            feature.OverrideEffectivenessState(state);
            feature.parent.pawn.health.capacities.Notify_CapacityLevelsDirty();
            return callback();
        }

        private static bool CheckFeatures<T>(
            Pawn pawn,
            IEnumerable<T> features,
            bool? state,
            Func<bool> callback
            ) where T : HediffComp, AddedPartEffectivenessModifier
        {
            features.Do(feature => feature.OverrideEffectivenessState(state));
            pawn.health.capacities.Notify_CapacityLevelsDirty();
            return callback();
        }

        private static bool ToggleAndCheckPart<T>(
            Hediff_AddedPart part,
            Func<bool> check,
            bool fallback = false
            ) where T : HediffComp, AddedPartEffectivenessModifier
        {
            var comp = part.TryGetComp<T>();
            if (comp == null) return fallback;

            var enabled = CheckFeature(comp, true, check);
            var disabled = CheckFeature(comp, false, check);
            CheckFeature(comp, null, () => true);

            return enabled != disabled;
        }

        private static bool ToggleAndCheckParts<T>(
            Pawn pawn,
            Predicate<T> predicate,
            Func<bool> check
            ) where T : HediffComp, AddedPartEffectivenessModifier
        {
            var features = AddedParts(pawn)
                .SelectMany(part => part.comps)
                .OfType<T>()
                .Where(predicate.Invoke)
                .ToList();
            
            // enable features
            var enabled = CheckFeatures(pawn, features, true, check);

            // disable features
            // checks if things such as both kidneys would be impacted
            var disabled = CheckFeatures(pawn, features, false, check);

            // restore features
            CheckFeatures(pawn, features, null, () => true);

            return enabled != disabled;
        }

        public static bool RequiresPartsToLive<T>(Pawn pawn) where T : HediffComp, AddedPartEffectivenessModifier
        {
            return RequiresPartsToLive<T>(pawn, part => true);
        }

        public static bool RequiresPartsToLive<T>(Pawn pawn, Predicate<T> predicate) where T : HediffComp, AddedPartEffectivenessModifier
        {
            return ToggleAndCheckParts(
                pawn,
                predicate,
                () => pawn.health.ShouldBeDeadFromRequiredCapacity() != null);
        }

        public static bool RequiresPartsForMovement<T>(Pawn pawn) where T : HediffComp, AddedPartEffectivenessModifier
        {
            return RequiresPartsForMovement<T>(pawn, part => true);
        }

        public static bool RequiresPartsForMovement<T>(Pawn pawn, Predicate<T> predicate) where T : HediffComp, AddedPartEffectivenessModifier
        {
            return ToggleAndCheckParts(
                pawn,
                predicate,
                () => !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving));
        }

        public static bool RequiresPartToLive<T>(Hediff_AddedPart part) where T : HediffComp, AddedPartEffectivenessModifier
        {
            return ToggleAndCheckPart<T>(part, () => part.pawn.health.ShouldBeDeadFromRequiredCapacity() != null);
        }

        public static bool RequiresPartToMove<T>(Hediff_AddedPart part) where T : HediffComp, AddedPartEffectivenessModifier
        {
            return ToggleAndCheckPart<T>(part, () => !part.pawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving));
        }

        public static IEnumerable<Hediff> GetHediffsForPart(Hediff part)
        {
            // TODO check if hediff is on a child part?
            return part.pawn.health.hediffSet.hediffs.Where(hediff => hediff.Part == part.Part);
        }
    }
}