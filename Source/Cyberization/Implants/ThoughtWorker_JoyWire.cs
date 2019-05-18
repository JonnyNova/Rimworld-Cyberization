using System.Linq;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Cyberization.Implants
{
    public class ThoughtWorker_JoyWire : ThoughtWorker_Hediff
    {
        private static bool AnyPoweredJoywires(Pawn pawn)
        {
            return pawn.health.hediffSet.hediffs
                .Where(implant => implant.def.defName == "Joywire")
                .OfType<ImplantPowered>()
                .Any(joywire => joywire.Consumer.Powered);
        }

        protected override ThoughtState CurrentStateInternal(Pawn pawn)
        {
            return AnyPoweredJoywires(pawn)
                ? base.CurrentStateInternal(pawn) 
                : ThoughtState.Inactive;
        }
    }
}