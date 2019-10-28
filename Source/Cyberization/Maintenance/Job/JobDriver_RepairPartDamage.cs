using System;
using System.Linq;
using FrontierDevelopments.Cyberization.Parts;
using RimWorld;
using Verse;
using Verse.AI;

namespace FrontierDevelopments.Cyberization.Maintenance.Job
{
    public class JobDriver_RepairPartDamage : JobDriver_BaseMaintenance
    {
        private const float baseRate = 250f;

        private AddedPartDamageable _part;

        protected override HediffComp Part => _part;

        protected override bool ShouldFail => !_part.Damaged;

        protected override Toil Repair()
        {
            _part = PartUtility.PartsNeedingDamageRepair(Patient).First();
            var workSpeed = pawn.GetStatValue(StatDefOf.WorkSpeedGlobal);
            var repair = new Toil()
                .WithProgressBar(TargetIndex.A, () => 1f - _part.HealthPercent);
            repair.defaultCompleteMode = ToilCompleteMode.Never;
            repair.tickAction = delegate
            {
                try
                {
                    PartUtility.GetHediffsForPart(_part.parent).OfType<Hediff_Injury>().First().Heal(workSpeed * baseRate);
                    pawn.skills.Learn(SkillDefOf.Crafting, 0.125f * workSpeed);
                }
                catch (InvalidOperationException)
                {
                }
            };
            return repair;
        }
    }
}