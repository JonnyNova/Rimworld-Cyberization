using RimWorld;
using Verse;

namespace FrontierDevelopments.Cyberization
{
    [DefOf]
    public static class CyberizationDefOf
    {
        public static JobDef SeekPartPower;
        public static NeedDef PartEnergy;
        public static HediffDef BionicEnergyCell;

        public static JobDef Cyberization_FixBreakdown;
        public static JobDef Cyberization_RepairPartDamage;
        public static JobDef Cyberization_RoutinePartMaintenance;
        
        static CyberizationDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof (NeedDefOf));
        }
    }
}