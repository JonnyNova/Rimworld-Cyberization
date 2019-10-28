using System;
using System.Linq;
using FrontierDevelopments.Cyberization.Parts;
using RimWorld;
using Verse;
using Verse.AI;

namespace FrontierDevelopments.Cyberization.Maintenance.Job
{
    public class JobDriver_RountinePartMaintenance : JobDriver_BaseMaintenance
    {
        private const float baseRate = 250f;

        private AddedPartMaintenance _part;

        protected override HediffComp Part => _part;

        protected override bool ShouldFail => !_part.CanBeMaintained;

        protected override Toil Repair()
        {
            _part = PartUtility.PartsNeedingRoutineMaintenance(Patient).First();
            var workSpeed = pawn.GetStatValue(StatDefOf.WorkSpeedGlobal);
            var repair = new Toil()
                .WithProgressBar(TargetIndex.A, () => _part.Percent);
            repair.defaultCompleteMode = ToilCompleteMode.Never;
            repair.tickAction = delegate
            {
                try
                {
                    _part.DoMaintenance((int)(workSpeed * baseRate));
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