using FrontierDevelopments.Cyberization.Parts;
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
                    return target.Downed  
                           && (target.needs?.TryGetNeed<PartEnergyNeed>()?.Empty ?? false) 
                           && PartUtility.RequiresPartsForMovement<AddedPartPowerConsumer>(target);
            }
            return false;
        }

        public override Verse.AI.Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Log.Message("rescue to power!");
            return base.JobOnThing(pawn, t, forced);
        }
    }
}