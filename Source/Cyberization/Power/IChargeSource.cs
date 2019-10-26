using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace FrontierDevelopments.Cyberization.Power
{
    public interface IChargeSource
    {
        bool Available { get; }
        int Rate { get; }
        Faction Faction { get; }
    }

    public static class ChargeSourceUtility
    {
        public static IEnumerable<Building> Find(Map map)
        {
            return map.listerBuildings
                .allBuildingsColonist
                .Where(IsChargeSource);
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
                                && charger.Faction == pawn.Faction);
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
                PathEndMode.InteractionCell,
                TraverseParms.For(pawn),
                100f,
                Validator(pawn));
        }
    }
}