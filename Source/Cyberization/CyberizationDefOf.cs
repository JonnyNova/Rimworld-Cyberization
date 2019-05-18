using RimWorld;
using Verse;

namespace FrontierDevelopments.Cyberization
{
    [DefOf]
    public static class CyberizationDefOf
    {
        public static NeedDef PartEnergy;
        
        static CyberizationDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof (NeedDefOf));
        }
    }
}