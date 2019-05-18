using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace FrontierDevelopments.Cyberization.Power.Job
{
    public class JobDriver_SeekPartPower : JobDriver
    {
        private const TargetIndex PowerSourceIndex = TargetIndex.A;
        private Building PowerSource => (Building) TargetThingA;

        private Need EnergyNeed => pawn.needs.TryGetNeed<PartEnergyNeed>();
        
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, (ReservationLayerDef) null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            var charger = PowerSource.AllComps.OfType<IChargeSource>().First();

            var pathMode = PowerSource.def.hasInteractionCell ? PathEndMode.InteractionCell : PathEndMode.OnCell;
            
            this.FailOnDestroyedOrNull(PowerSourceIndex);
            this.FailOn(() => !charger.Available);
            
            yield return Toils_Goto
                .GotoThing(PowerSourceIndex, pathMode)
                .FailOnDestroyedNullOrForbidden(PowerSourceIndex)
                .FailOn(() => charger == null || !charger.Available)
                .FailOn(() => !pawn.CanReach(PowerSource, pathMode, Danger.Deadly));
            
            var waitForCharge = new Toil()
                .FailOn(() =>
                    charger == null
                    || !charger.Available
                    || EnergyNeed == null)
                .WithProgressBar(PowerSourceIndex, () => EnergyNeed.CurLevelPercentage);
            waitForCharge.initAction = () => waitForCharge.actor.pather.StopDead();
            waitForCharge.defaultCompleteMode = ToilCompleteMode.Never;
            waitForCharge.AddEndCondition(() =>
                EnergyNeed.CurLevelPercentage > Settings.SeekPowerChargeTo
                    ? JobCondition.Succeeded
                    : JobCondition.Ongoing);
            
            yield return waitForCharge;
        }
    }
}