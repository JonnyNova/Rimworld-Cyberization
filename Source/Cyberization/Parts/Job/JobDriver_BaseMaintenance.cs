using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace FrontierDevelopments.Cyberization.Parts.Job
{
    public abstract class JobDriver_BaseMaintenance : JobDriver
    {
        protected abstract Toil Repair();
        
        protected abstract HediffComp Part { get; }
        
        protected abstract bool ShouldFail { get; }

        protected Pawn Patient => TargetThingA as Pawn;
        
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(TargetA, job);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOn(() => !Patient.InBed());
            this.FailOn(() => !pawn.CanReach(pawn, PathEndMode.ClosestTouch, Danger.Deadly));
            this.FailOn(() => Part == null || ShouldFail);

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);

            yield return Repair();
        }
    }
}