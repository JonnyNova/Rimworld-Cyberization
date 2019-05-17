using System.Linq;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Cyberization
{
    public class AddedPartEnergyNeed : Need
    {
        public AddedPartEnergyNeed(Pawn pawn) : base(pawn)
        {
        }
        
        public override float MaxLevel =>
            PowerProvider.Providers(pawn).Aggregate(0L, (sum, provider) => sum + provider.MaxEnergy);
        
        public override float CurLevel => 
            PowerProvider.Providers(pawn).Aggregate(0L, (sum, provider) => sum + provider.Energy);

        public override void NeedInterval()
        {
        }
    }
}