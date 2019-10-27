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

        private IEnergySource EnergySource
        {
            get
            {
                if (_energySource == null)
                {
                    _energySource = EnergySourceUtility.Find(parent);
                }

                return _energySource;
            }
        }

        private PartChargerProperties Props => (PartChargerProperties) props;

        public bool Available => EnergySource.IsActive() && Rate > 0;

        public int Rate => (int) Math.Min(EnergySource.EnergyAvailable - EnergySource.BaseConsumption, Props.chargeRate);

        public int RateAvailable(IEnumerable<IPowerProvider> providers)
        {
            return (ValidChargeTargets().Count() + providers.Count()) / Rate;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            _energySource = EnergySourceUtility.Find(parent);
        }

        public void Charge(Pawn pawn)
        {
            _connected.Add(pawn);
        }

        private IEnumerable<IPowerProvider> ValidChargeTargets()
        {
            var chargables = new HashSet<IPowerProvider>(ChargablesConnected());
            if(Props.wirelessCharging) chargables.AddRange(ChargablesOnPad().ToList());
            return chargables;
        }

        private IEnumerable<Thing> ThingsOnPad()
        {
            if(!parent.Spawned) return new List<Thing>();
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
            if (EnergySource.IsActive())
            {
                var chargables = ValidChargeTargets().ToList();
                if (chargables.Count > 0)
                {
                    var ratePer = Rate / chargables.Count;
                    var consumed = chargables.Aggregate(0L, (sum, chargable) => sum + chargable.Charge(ratePer));
                    EnergySource.BaseConsumption = -consumed / Settings.ElectricRatio;
                }
                else
                {
                    EnergySource.BaseConsumption = 0;
                }
            }

            _connected.Clear();
        }

        public bool CanUse(Pawn pawn)
        {
            if (parent.Faction != pawn.Faction 
                && parent.Faction.RelationWith(pawn.Faction).kind != FactionRelationKind.Ally) return false;
            
            switch (pawn.RaceProps.intelligence)
            {
                case Intelligence.Animal: return Props.wirelessCharging;
                default: return true;
            }
        }
    }
}