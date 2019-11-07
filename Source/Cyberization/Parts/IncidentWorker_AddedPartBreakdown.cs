using System.Collections.Generic;
using System.Linq;
using Harmony;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Cyberization.Parts
{
    public class IncidentWorker_AddedPartBreakdown : IncidentWorker
    {
        private static readonly HashSet<AddedPartBreakdownable> Parts = new HashSet<AddedPartBreakdownable>();

        public static void Add(AddedPartBreakdownable part)
        {
            Parts.Add(part);
        }

        public static void Remove(AddedPartBreakdownable part)
        {
            Parts.Remove(part);
        }

        private IEnumerable<AddedPartBreakdownable> FindTargets()
        {
            return Parts.Where(part => part.CanBreakdown);
        }

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return Mod.Settings.UsePartBreakdowns && FindTargets().Any();
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            return FindTargets().RandomElement().BreakDown();
        }

        [HarmonyPatch(typeof(SavedGameLoaderNow), nameof(SavedGameLoaderNow.LoadGameFromSaveFileNow))]
        private static class Patch_OnLoad
        {
            [HarmonyPrefix]
            private static void ClearCachedSolarFlareState()
            {
                Parts.Clear();
            }
        }
    }
}