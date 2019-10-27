using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Cyberization.Parts
{
    public class IncidentWorker_AddedPartBreakdown : IncidentWorker
    {
        private IEnumerable<AddedPartBreakdownable> FindTargets()
        {
            return Find.WorldObjects.Caravans.SelectMany(caravan => caravan.PawnsListForReading)
                .Concat(Find.Maps.SelectMany(map => map.mapPawns.AllPawns))
                .SelectMany(p => p.health.hediffSet.hediffs)
                .OfType<Hediff_AddedPart>()
                .SelectMany(h => h.comps.Where(comp => comp is AddedPartBreakdownable))
                .OfType<AddedPartBreakdownable>()
                .Where(comp => comp.CanBreakdown);
        }

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return FindTargets().Any();
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            return FindTargets().RandomElement().BreakDown();
        }
    }
}