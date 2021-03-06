using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace FrontierDevelopments.Cyberization.Power.Job
{
    public class JobDriver_SeekPartPower : JobDriver
    {
        private const TargetIndex PowerSourceIndex = TargetIndex.A;
        private Building PowerSource => (Building) TargetThingA;

        private PartEnergyNeed EnergyNeed => pawn.needs.TryGetNeed<PartEnergyNeed>();
        
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, (ReservationLayerDef) null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            var charger = PowerSource.AllComps.OfType<IChargeSource>().First();
            
            if(charger.WirelessCharging) 
                return ChargeToils(charger, PathEndMode.OnCell, () => pawn.pather.StopDead(), null);
            else
                return ChargeToils(charger, PathEndMode.Touch, null, () => charger.Charge(pawn));
        }

        private IEnumerable<Toil> ChargeToils(IChargeSource charger, PathEndMode pathMode, Action initAction, Action tickAction)
        {
            this.FailOnDestroyedOrNull(PowerSourceIndex);
            this.FailOn(() => !charger.Available);
            this.FailOn(() =>
                EnergyNeed == null
                || !EnergyNeed.CanBeSatisfied
                || EnergyNeed.Satisfied
                || charger == null
                || !charger.Available
                || !pawn.CanReach(PowerSource, pathMode, Danger.Deadly));

            yield return Toils_Goto
                .GotoThing(PowerSourceIndex, pathMode)
                .FailOnDestroyedNullOrForbidden(PowerSourceIndex);

            var charge = new Toil()
                .WithProgressBar(PowerSourceIndex, () => EnergyNeed.CurLevelPercentage, true);
            charge.defaultCompleteMode = ToilCompleteMode.Never;
            charge.AddEndCondition(() =>
                EnergyNeed.Satisfied
                    ? JobCondition.Succeeded
                    : JobCondition.Ongoing);
            if(initAction != null) charge.initAction = initAction;
            if(tickAction != null) charge.tickAction = tickAction;
            yield return charge;
        }
    }
}