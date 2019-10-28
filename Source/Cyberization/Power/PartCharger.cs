using System;
using System.Collections.Generic;
using System.Linq;
using FrontierDevelopments.General.Energy;
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
        private IEnergyNode _energySource;
        private readonly List<Pawn> _connected = new List<Pawn>();

        private IEnergyNode EnergySource
        {
            get
            {
                if (_energySource == null)
                {
                    _energySource = parent.AllComps.OfType<IEnergyNode>().First();
                }

                return _energySource;
            }
        }

        private PartChargerProperties Props => (PartChargerProperties) props;

        public bool Available => RateAvailable > 0;

        public float Provide(float amount)
        {
            return _energySource.Provide(amount);
        }

        public float Consume(float amount)
        {
            return _energySource.Consume(amount);
        }

        public float AmountAvailable => _energySource.AmountAvailable;
        public float TotalAvailable => _energySource.TotalAvailable;
        public float RateAvailable => _energySource.RateAvailable;
        public float MaxRate => Props.chargeRate;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            _energySource = parent.AllComps.OfType<IEnergyNode>().First();
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
            if (Available)
            {
                var chargables = ValidChargeTargets().ToList();
                if (chargables.Count > 0)
                {
                    var ratePer = RateAvailable / chargables.Count;
                    var consumed = chargables.Aggregate(0L, (sum, chargable) => sum + chargable.Charge((long)ratePer));
                    EnergySource.Consume(consumed / Mod.Settings.ElectricRatio / GenDate.TicksPerDay);
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