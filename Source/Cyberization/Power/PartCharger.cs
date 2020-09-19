using System.Collections.Generic;
using System.Linq;
using FrontierDevelopments.General;
using FrontierDevelopments.General.Energy;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Cyberization.Power
{
    public class PartChargerProperties : CompProperties
    {
        public bool wirelessCharging;

        public PartChargerProperties()
        {
            compClass = typeof(PartCharger);
        }
    }

    public class PartCharger : ThingComp, IChargeSource, IEnergyConsumer
    {
        private IEnergyNet _parent;
        private readonly List<Pawn> _connected = new List<Pawn>();

        private PartChargerProperties Props => (PartChargerProperties) props;

        public bool Available => RateAvailable > 0;

        public float RateAvailable => _parent?.RateAvailable ?? 0f;

        public bool WirelessCharging => Props.wirelessCharging;

        public void ConnectTo(IEnergyNet net)
        {
            _parent?.Disconnect(this);
            _parent = net;
            _parent?.Connect(this);
        }

        public void Disconnect()
        {
            _parent?.Disconnect();
            _parent = null;
        }

        public IEnergyNet Parent => _parent;

        public void HasPower(bool isPowered)
        {
        }

        public float Rate => 0f;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            ConnectTo(parent.AllComps.OfType<IEnergyNet>().First());
        }

        public void Charge(Pawn pawn)
        {
            _connected.Add(pawn);
        }

        private IEnumerable<IEnergyProvider> ValidChargeTargets()
        {
            var chargables = new HashSet<IEnergyProvider>(ChargablesConnected());
            if(Props.wirelessCharging) chargables.AddRange(ChargablesOnPad().ToList());
            return chargables;
        }

        private IEnumerable<Thing> ThingsOnPad()
        {
            if(!parent.Spawned) return new List<Thing>();
            return parent.Map.thingGrid.ThingsAt(parent.def.hasInteractionCell ? parent.InteractionCell : parent.Position);
        }

        private IEnumerable<IEnergyProvider> ChargablesOnPad()
        {
            var result = new List<IEnergyProvider>();
            var things = ThingsOnPad().ToList();

            // Pawn nets
            result.AddRange(
                things
                    .OfType<Pawn>()
                    .Select(PawnPartPowerNet.Get)
                    .Cast<IEnergyProvider>());

            // Non pawn, not charger anything else
            result.AddRange(
                things
                    .OfType<ThingWithComps>()
                    .Where(thing => thing.GetType() != typeof(Pawn))
                    .Where(thing => thing != parent)
                    .SelectMany(thing => thing.AllComps)
                    .OfType<IEnergyProvider>());

            return result;
        }

        private IEnumerable<IEnergyProvider> ChargablesConnected()
        {
            return _connected
                .Select(PawnPartPowerNet.Get)
                .Select(net => net as IEnergyProvider);
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
                    var consumed = chargables.Aggregate(0f, (sum, chargable) => sum + chargable.Provide(ratePer));
                    _parent.Consume(consumed / Mod.Settings.ElectricRatio / GenDate.TicksPerDay);
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

        public override void PostExposeData()
        {
            Scribe_References.Look(ref _parent, "partChargerNetParent");
            
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                ConnectTo(_parent);
            }
        }
    }
}
