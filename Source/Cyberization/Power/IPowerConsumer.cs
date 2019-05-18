using System.Collections.Generic;
using System.Linq;
using Verse;

namespace FrontierDevelopments.Cyberization.Power
{
    public interface IPowerConsumer
    {
        int Priority { get; }
        
        bool Powered { get; }
        
        IEnumerable<BodyPartTagDef> Tags { get; }

        void PowerTick();
    }

    public static class PowerConsumer
    {
        public static IEnumerable<IPowerConsumer> All(Pawn pawn)
        {
            return pawn.health.hediffSet.hediffs
                .OfType<IPowerConsumer>()
                .Concat(
                    pawn.health.hediffSet.hediffs
                        .OfType<HediffWithComps>()
                        .SelectMany(h => h.comps)
                        .OfType<IPowerConsumer>());
        }
    }
}