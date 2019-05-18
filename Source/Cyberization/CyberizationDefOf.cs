using RimWorld;
using Verse;

namespace FrontierDevelopments.Cyberization
{
    [DefOf]
    public static class CyberizationDefOf
    {
        public static JobDef SeekPartPower;
        public static NeedDef PartEnergy;
        
        static CyberizationDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof (NeedDefOf));
        }
    }
}