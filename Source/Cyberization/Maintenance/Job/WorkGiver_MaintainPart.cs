using System.Linq;
using FrontierDevelopments.Cyberization.Parts;
using RimWorld;
using Verse;
using Verse.AI;

namespace FrontierDevelopments.Cyberization.Maintenance.Job
{
    public class WorkGiver_MaintainPart : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);

        public override PathEndMode PathEndMode => PathEndMode.ClosestTouch;

        public override bool HasJobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            switch (thing)
            {
                case Pawn target: return PartUtility.PartsNeedingAnyMaintenance(target).Any() && target.InBed();
                default: return false;
            }
        }

        public override Verse.AI.Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            var patient = thing as Pawn;
            if (patient == null) return null;

            JobDef jobDef;
            if (PartUtility.PartsNeedingBreakdownRepair(patient).Any())
            {
                jobDef = CyberizationDefOf.Cyberization_FixBreakdown;
            } else if (PartUtility.PartsNeedingDamageRepair(patient).Any())
            {
                jobDef = CyberizationDefOf.Cyberization_RepairPartDamage;
            } else if (PartUtility.PartsNeedingRoutineMaintenance(patient).Any())
            {
                jobDef = CyberizationDefOf.Cyberization_RoutinePartMaintenance;
            }
            else
            {
                return null;
            }

            return new Verse.AI.Job(jobDef)
            {
                targetA = patient
            };
        }
    }
}