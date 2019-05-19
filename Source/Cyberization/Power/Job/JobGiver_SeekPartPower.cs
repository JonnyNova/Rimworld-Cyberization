using System;
using System.Linq;
using Verse;
using Verse.AI;

namespace FrontierDevelopments.Cyberization.Power.Job
{
    public class JobGiver_SeekPartPower : ThinkNode_JobGiver
    {
        private static Predicate<Thing> Validator(Pawn pawn)
        {
            return thing =>
            {
                switch (thing)
                {
                    case ThingWithComps thingWithComps:
                        return thingWithComps.AllComps.OfType<IChargeSource>().Any(charger =>
                            charger.Available && charger.Faction == pawn.Faction);
                    default:
                        return false;
                }
            };
        }

        protected override Verse.AI.Job TryGiveJob(Pawn pawn)
        {
            if (pawn.Downed) return null;
            var need = pawn.needs.TryGetNeed<PartEnergyNeed>();
            if (need == null || !need.CanBeSatisfied|| need.CurLevelPercentage > Settings.SeekPowerPercent) return null;
            var result = GenClosest.ClosestThingReachable(
                pawn.Position, 
                pawn.Map, 
                ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), 
                PathEndMode.InteractionCell, 
                TraverseParms.For(pawn), 
                100f, 
                Validator(pawn));
            return result != null
                ? new Verse.AI.Job(CyberizationDefOf.SeekPartPower, result)
                : null;
        }
    }
}