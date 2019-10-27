using System.Collections.Generic;
using System.Linq;
using FrontierDevelopments.Cyberization.Parts;
using Harmony;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Cyberization.Power
{
    internal class PartPowerUserCache : MapComponent
    {
        private List<Pawn> _partPowerUsers;

        public IEnumerable<Pawn> PartPowerPowerUsers => _partPowerUsers;

        public PartPowerUserCache(Map map) : base(map)
        {
            
        }

        private void Update()
        {
            _partPowerUsers = map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer)
                .Where(PartUtility.HasPoweredParts)
                .ToList();
        }

        public override void MapComponentTick()
        {
            Update();
        }

        [HarmonyPatch(typeof(AlertsReadout), nameof(AlertsReadout.AlertsReadoutUpdate))]
        static class Patch_AlertsReadout_AlertsReadoutUpdate
        {
            [HarmonyPrefix]
            static void UpdateMapPartUsersCache()
            {
                Find.Maps
                    .Select(map => map.GetComponent<PartPowerUserCache>())
                    .Do(manager => manager.Update());
            }
        }
    }
}