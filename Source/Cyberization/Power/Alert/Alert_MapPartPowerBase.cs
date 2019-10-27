using System.Collections.Generic;
using System.Linq;
using Verse;

namespace FrontierDevelopments.Cyberization.Power.Alert
{
    public abstract class Alert_MapPartPowerBase : Alert_PartPowerBase
    {
        private static IEnumerable<PartPowerUserCache> Caches => Find.Maps.Select(map => map.GetComponent<PartPowerUserCache>());
        protected static IEnumerable<Pawn> PartPowerUsers => Caches.SelectMany(cache => cache.PartPowerPowerUsers);
    }
}