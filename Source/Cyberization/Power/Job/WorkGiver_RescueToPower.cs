using RimWorld;
using Verse;

namespace FrontierDevelopments.Cyberization.Power.Job
{
    public class WorkGiver_RescueToPower : WorkGiver_Scanner
    {
        public override bool HasJobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            switch (thing)
            {
                case Pawn target:
                    target.Downed 
                    return 
            }
        }

        public override Verse.AI.Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return base.JobOnThing(pawn, t, forced);
        }
    }
}