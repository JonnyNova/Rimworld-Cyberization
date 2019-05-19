using Verse;
using Verse.AI;

namespace FrontierDevelopments.Cyberization.Power.Job
{
    public class ThinkNode_PartPowerSatisfied : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            var need = pawn.needs.TryGetNeed<PartEnergyNeed>();
            if (need == null || !need.CanBeSatisfied) return true;
            return need.CurLevelPercentage > Settings.SeekPowerPercent;
        }
    }
}