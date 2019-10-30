using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace FrontierDevelopments.Cyberization.Power
{
    public interface IChargeSource
    {
        bool Available { get; }
        float RateAvailable { get; }
        void Charge(Pawn pawn);
        bool CanUse(Pawn pawn);
    }

    public static class ChargeSourceUtility
    {
        public static IEnumerable<Building> Find(Map map)
        {
            return map.listerBuildings
                .allBuildingsColonist
                .Where(IsChargeSource);
        }

        public static IEnumerable<IChargeSource> FindSources(Caravan caravan)
        {
            return FindSources(CaravanInventoryUtility.AllInventoryItems(caravan), true);
        }

        public static IEnumerable<IChargeSource> FindSources(ThingWithComps thing)
        {
            foreach (var comp in thing.AllComps)
            {
                switch (comp)
                {
                    case IChargeSource source:
                        yield return source;
                        break;
                }
            }
        }

        public static IEnumerable<IChargeSource> FindSources(Thing thing)
        {
            switch (thing)
            {
                case ThingWithComps thingWithComps:
                    foreach (var source in FindSources(thingWithComps))
                    {
                        yield return source;
                    }
                    break;
            }
        }

        public static IEnumerable<IChargeSource> FindSources(IEnumerable<Thing> things, bool searchMinified = false)
        {
            foreach (var thing in things)
            {
                switch (thing)
                {
                    case MinifiedThing minifiedThing:
                        if (searchMinified)
                        {
                            foreach (var source in FindSources(minifiedThing.InnerThing))
                            {
                                yield return source;
                            }
                        }
                        break;
                    case ThingWithComps thingWithComps:
                        foreach (var source in FindSources(thingWithComps))
                        {
                            yield return source;
                        }
                        break;
                }
            }
        }

        public static bool IsChargeSource(ThingWithComps thing)
        {
            return thing.AllComps
                .OfType<IChargeSource>()
                .Any();
        }

        public static bool IsAvailableChargeSource(ThingWithComps thing)
        {
            return thing.AllComps
                .OfType<IChargeSource>()
                .Any(source => source.Available);
        }

        private static Predicate<Thing> Validator(Pawn pawn)
        {
            return thing =>
            {
                switch (thing)
                {
                    case ThingWithComps thingWithComps:
                        return thingWithComps.AllComps
                            .OfType<IChargeSource>()
                            .Any(charger =>
                                charger.Available
                                && charger.CanUse(pawn)
                                && pawn.Map.reservationManager
                                    .CanReserve(pawn, new LocalTargetInfo(thing)));
                    default:
                        return false;
                }
            };
        }

        public static Thing ClosestChargeSource(Pawn pawn)
        {
            return GenClosest.ClosestThingReachable(
                pawn.Position,
                pawn.Map,
                ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial),
                PathEndMode.Touch,
                TraverseParms.For(pawn),
                100f,
                Validator(pawn));
        }
    }
}