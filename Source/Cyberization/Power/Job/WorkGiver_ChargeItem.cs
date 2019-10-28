using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace FrontierDevelopments.Cyberization.Power.Job
{
    public class WorkGiver_ChargeItem : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.HaulableEverOrMinifiable);
        
        public override PathEndMode PathEndMode => PathEndMode.ClosestTouch;
        
        public override bool HasJobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            return PowerProvider.Providers(thing).Any(provider => provider.NeedCharge);
        }

        public override Verse.AI.Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return base.JobOnThing(pawn, t, forced);
        }
    }
}