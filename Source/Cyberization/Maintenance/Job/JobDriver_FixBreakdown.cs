using System;
using System.Linq;
using FrontierDevelopments.Cyberization.Parts;
using RimWorld;
using Verse;
using Verse.AI;

namespace FrontierDevelopments.Cyberization.Maintenance.Job
{
    public class JobDriver_FixBreakdown : JobDriver_BaseMaintenance
    {
        private const float baseDuration = 60f;
        
        private AddedPartBreakdownable _part;

        protected override HediffComp Part => _part;

        protected override bool ShouldFail => !_part.IsBrokenDown;

        protected override Toil Repair()
        {
            _part = PartUtility.PartsNeedingBreakdownRepair(Patient).First();
            var workSpeed = pawn.GetStatValue(StatDefOf.WorkSpeedGlobal);
            var duration = (int)(1f / workSpeed * baseDuration);
            var waitToil = Toils_General
                .Wait(duration)
                .WithProgressBarToilDelay(TargetIndex.A)
                .WithEffect(Patient.def.repairEffect, TargetIndex.A);
            waitToil.tickAction = delegate
            {
                pawn.skills.Learn(SkillDefOf.Crafting, 0.125f * workSpeed);
            };
            waitToil.AddFinishAction(delegate
            {
                try
                {
                    _part.Repair();
                }
                catch (InvalidOperationException)
                {
                }
            });
            return waitToil;
        }
    }
}