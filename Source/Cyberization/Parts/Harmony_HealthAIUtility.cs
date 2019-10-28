using Harmony;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Cyberization.Parts
{
    public class Harmony_HealthAIUtility
    {
        [HarmonyPatch(typeof(HealthAIUtility), nameof(HealthAIUtility.ShouldSeekMedicalRest))]
        static class Patch_HealthAIUtility_ShouldSeekMedicalRest
        {
            [HarmonyPostfix]
            static bool SeekPartMaintenance(bool __result, Pawn pawn)
            {
                if (PartUtility.NeedMaintenance(pawn)) return true;
                return __result;
            }
        }

        [HarmonyPatch(typeof(HealthAIUtility), nameof(HealthAIUtility.ShouldSeekMedicalRestUrgent))]
        static class Patch_HealthAIUtility_ShouldSeekMedicalRestUrgent
        {
            [HarmonyPostfix]
            static bool SeekPartMaintenance(bool __result, Pawn pawn)
            {
                if (PartUtility.NeedMaintenanceUrgent(pawn)) return true;
                return __result;
            }
        }
    }
}