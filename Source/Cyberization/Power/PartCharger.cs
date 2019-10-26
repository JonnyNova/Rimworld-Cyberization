using System;
using System.Collections.Generic;
using System.Linq;
using FrontierDevelopments.General;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Cyberization.Power
{
    public class PartChargerProperties : CompProperties
    {
        public int chargeRate;
        public bool wirelessCharging;

        public PartChargerProperties()
        {
            compClass = typeof(PartCharger);
        }
    }

    public class PartCharger : ThingComp, IChargeSource
    {
        private IEnergySource _energySource;
        private readonly List<Pawn> _connected = new List<Pawn>();

        private PartChargerProperties Props => (PartChargerProperties) props;

        public bool Available => _energySource.IsActive() && Rate > 0;

        public Faction Faction => parent.Faction;

        public int Rate => (int) Math.Min(_energySource.EnergyAvailable - _energySource.BaseConsumption, Props.chargeRate);

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            _energySource = EnergySourceUtility.Find(parent);
        }

        public void Charge(Pawn pawn)
        {
            _connected.Add(pawn);
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
                    .SelectMany(PowerProvider.Providers));
            result.AddRange(
                ThingsOnPad()
                    .OfType<ThingWithComps>()
                    .SelectMany(thing => thing.AllComps)
                    .OfType<IPowerProvider>());
            return result;
        }

        private IEnumerable<IPowerProvider> ChargablesConnected()
        {
            return _connected.SelectMany(PowerProvider.Providers);
        }

        // TODO improve performance somehow?
        public override void CompTick()
        {
            if (_energySource.IsActive())
            {
                var chargables = new HashSet<IPowerProvider>(ChargablesConnected());
                if(Props.wirelessCharging) chargables.AddRange(ChargablesOnPad().ToList());
                
                if (chargables.Count > 0)
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

            _connected.Clear();
        }
    }
}