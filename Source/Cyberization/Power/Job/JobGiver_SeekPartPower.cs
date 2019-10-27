using Verse;
using Verse.AI;

namespace FrontierDevelopments.Cyberization.Power.Job
{
    public class JobGiver_SeekPartPower : ThinkNode_JobGiver
    {
        protected override Verse.AI.Job TryGiveJob(Pawn pawn)
        {
            if (pawn.Downed) return null;
            var need = pawn.needs.TryGetNeed<PartEnergyNeed>();
            if (need == null || !need.CanBeSatisfied|| !need.SeekSatisfaction) return null;
            var result = ChargeSourceUtility.ClosestChargeSource(pawn);
            return result != null
                ? new Verse.AI.Job(CyberizationDefOf.SeekPartPower, result)
                : null;
        }
    }
}