//using System.Collections.Generic;
//using System.Linq;
//using Verse;
//using Verse.AI;
//
//namespace FrontierDevelopments.Cyberization.Power.Job
//{
//    public class JobDriver_RescueToPower : JobDriver
//    {
//        private TargetIndex TargetPawnIndex => TargetIndex.A;
//        private TargetIndex TargetChargerIndex => TargetIndex.B;
//        private Pawn TargetPawn => (Pawn) TargetThingA;
//        private IChargeSource TargetCharger => ((ThingWithComps)TargetThingB).AllComps.OfType<IChargeSource>().First();
//        
//        public override bool TryMakePreToilReservations(bool errorOnFailed)
//        {
//            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed) && 
//                   pawn.Reserve(job.targetB, job, 1, -1, null, errorOnFailed);
//        }
//
//        private Toil ConnectToCharger()
//        {
//            
//        }
//
//        private Toil PlaceOnCharger()
//        {
//            
//        }
//
//        protected override IEnumerable<Toil> MakeNewToils()
//        {
//            this.FailOnDespawnedOrNull(TargetPawnIndex);
//            this.FailOn(() => !pawn.Downed);
//
//            yield return Toils_Goto
//                .GotoThing(TargetPawnIndex, PathEndMode.ClosestTouch);
//            
//            if(TargetCharger.
//            
//            yield return 
//
//            throw new System.NotImplementedException();
//        }
//    }
//}