using System;
using System.Collections.Generic;
using System.Linq;
using FrontierDevelopments.General;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Cyberization.Power
{
    public class ChargePadProperties : CompProperties
    {
        public int chargeRate;

        public ChargePadProperties()
        {
            compClass = typeof(CompChargePad);
        }
    }

    public class CompChargePad : ThingComp, IChargeSource
    {
        private IEnergySource _energySource;

        private ChargePadProperties Props => (ChargePadProperties) props;

        public bool Available => _energySource.IsActive() && Rate > 0;

        public Faction Faction => parent.Faction;

        public int Rate => (int) Math.Min(_energySource.EnergyAvailable - _energySource.BaseConsumption, Props.chargeRate);

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            _energySource = EnergySourceUtility.Find(parent);
        }

        private IEnumerable<Thing> ThingsOnPad()
        {
            return parent.Map.thingGrid.ThingsAt(parent.def.hasInteractionCell ? parent.InteractionCell : parent.Position);
        }

        private IEnumerable<IPowerProvider> ChargablesOnPad()
        {
            var result = new List<IPowerProvider>();
            result.AddRange(
                ThingsOnPad()
                    .OfType<Pawn>()
                    .SelectMany(pawn => PowerProvider.Providers(pawn)));
            result.AddRange(
                ThingsOnPad()
                    .OfType<ThingWithComps>()
                    .SelectMany(thing => thing.AllComps)
                    .OfType<IPowerProvider>());
            return result;
        }

        // TODO improve performance somehow?
        public override void CompTick()
        {
            if (_energySource.IsActive())
            {
                var chargables = ChargablesOnPad().ToList();
                if (!chargables.NullOrEmpty())
                {
                    var ratePer = Rate / chargables.Count;
                    var consumed = chargables.Aggregate(0L, (sum, chargable) => sum + chargable.Charge(ratePer));
                    _energySource.BaseConsumption = -consumed / Settings.ElectricRatio;
                }
                else
                {
                    _energySource.BaseConsumption = 0;
                }
            }
        }
    }
}