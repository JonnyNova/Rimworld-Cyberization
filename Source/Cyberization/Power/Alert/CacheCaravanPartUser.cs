using System.Collections.Generic;
using System.Linq;
using FrontierDevelopments.Cyberization.Parts;
using Harmony;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace FrontierDevelopments.Cyberization.Power
{
    internal static class CacheCaravanPartUser
    {
        private static List<Caravan> _partPowerCaravans;

        public static IEnumerable<Caravan> PartPowerCaravans => _partPowerCaravans;

        private static bool HasPowerUsers(Caravan caravan)
        {
            return caravan.pawns.Any<Pawn>(PartUtility.HasPoweredParts);
        }

        private static void Update()
        {
            _partPowerCaravans = Find.WorldObjects.Caravans
                .Where(HasPowerUsers)
                .ToList();
        }

        [HarmonyPatch(typeof(AlertsReadout), nameof(AlertsReadout.AlertsReadoutUpdate))]
        static class Patch_AlertsReadout_AlertsReadoutUpdate
        {
            [HarmonyPrefix]
            static void UpdateCaravanPartUsersCache()
            {
                Update();
            }
        }
    }
}