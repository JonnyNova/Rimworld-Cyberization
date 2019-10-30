using System.Collections.Generic;
using System.Linq;
using FrontierDevelopments.General;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Cyberization.Power
{
    public class PartPowerConsumerProperties : HediffCompProperties
    {
        public int powerPerTick = 1;
        public int priority = 5;
        public bool essential;
        
        public PartPowerConsumerProperties()
        {
            compClass = typeof(PartPowerConsumer);
        }
    }

    public class PartPowerConsumer : HediffComp, IPowerConsumer
    {
        private IEnergyNet _parent;
        private bool _enabled = true;
        private bool _powered;

        private PartPowerConsumerProperties Props => (PartPowerConsumerProperties) props;

        private bool ShouldConsume => Mod.Settings.UsePartPower || Props.essential || !(Pawn.Downed || Pawn.InBed());

        public bool Powered => _enabled && _powered;

        public int Priority => Props.priority;

        public bool Essential => Props.essential;

        public bool Enabled
        {
            get => _enabled;
            set
            {
                var last = _enabled;
                _enabled = value;
                if (last != _enabled)
                {
                    _parent?.Changed();
                    Pawn.health.Notify_HediffChanged(parent);
                }
            }
        }

        public void ConnectTo(IEnergyNet net)
        {
            _parent?.Disconnect(this);
            _parent = net;
            _parent.Connect(this);
        }

        public override void CompPostMake()
        {
            ConnectTo(parent.pawn.AllComps.OfType<IEnergyNet>().First());
        }

        public override void CompPostPostRemoved()
        {
            _parent?.Disconnect(this);
        }

        public IEnergyNet Parent => _parent;

        public IEnumerable<BodyPartTagDef> Tags => parent.Part.def.tags;

        public void HasPower(bool isPowered)
        {
            var last = Powered;
            _powered = isPowered;
            if(last != Powered) Pawn.health.Notify_HediffChanged(parent);
        }

        public float Rate => ShouldConsume && _enabled ? Props.powerPerTick : 0f;

        public override string CompTipStringExtra => Powered ? null : "NoPower".Translate();

        public override void CompExposeData()
        {
            Scribe_References.Look(ref _parent, "partConsumerNetParent");
            Scribe_Values.Look(ref _enabled, "partConsumerPowered");
            Scribe_Values.Look(ref _powered, "partConsumerNetPowered");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                ConnectTo(_parent);
            }
        }
        
        public override string ToString()
        {
            return base.ToString() + " in " + parent;
        }

        public string Label => parent.Label;
    }
}
