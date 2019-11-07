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

    public class PartPowerConsumer : HediffComp, IPowerConsumer, ICombatListener
    {
        private IEnergyNet _parent;
        private bool _enabled = true;
        private bool _powered;
        private float _rate;
        private bool _enableWhileDrafted = true;
        private bool _enableWhileNotDrafted = true;
        private bool _enableInCombat = true;
        private bool _enableOutOfCombat = true;

        private PartPowerConsumerProperties Props => (PartPowerConsumerProperties) props;

        private bool ShouldConsume => Mod.Settings.UsePartPower || Props.essential || !(Pawn.Downed || Pawn.InBed());

        public bool Powered => _enabled && _powered;

        public int Priority => Props.priority;

        public bool Essential => Props.essential;

        public float Efficiency => _rate / Props.powerPerTick;

        public float MatRate => Props.powerPerTick;

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

        public bool EnableWhileDrafted
        {
            get => _enableWhileDrafted;
            set
            {
                _enableWhileDrafted = value;
                if (parent.pawn.Drafted) Enabled = value;
            }
        }

        public bool EnabledWhileNotDrafted
        {
            get => _enableWhileNotDrafted;
            set
            {
                _enableWhileNotDrafted = value;
                if (!parent.pawn.Drafted) Enabled = value;
            }
        }

        public bool EnabledInCombat
        {
            get => _enableInCombat;
            set
            {
                _enableInCombat = value;
                if (PawnCombatHandler.IsInCombat(parent.pawn)) Enabled = value;
            }
        }

        public bool EnabledOutOfCombat
        {
            get => _enableOutOfCombat;
            set
            {
                _enableOutOfCombat = value;
                if (!PawnCombatHandler.IsInCombat(parent.pawn)) Enabled = value;
            }
        }

        public void ConnectTo(IEnergyNet net)
        {
            _parent?.Disconnect(this);
            _parent = net;
            _parent.Connect(this);
        }

        public void Disconnect()
        {
            _parent?.Disconnect(this);
            _parent = null;
        }

        public override void CompPostMake()
        {
            ConnectTo(parent.pawn.AllComps.OfType<IEnergyNet>().First());
            _rate = Props.powerPerTick;
            PawnCombatHandler.Add(parent.pawn, this);
        }

        public override void CompPostPostRemoved()
        {
            _parent?.Disconnect(this);
            PawnCombatHandler.Remove(parent.pawn, this);
        }

        public IEnergyNet Parent => _parent;

        public IEnumerable<BodyPartTagDef> Tags => parent.Part.def.tags;

        public void HasPower(bool isPowered)
        {
            var last = Powered;
            _powered = isPowered;
            if(last != Powered) Pawn.health.Notify_HediffChanged(parent);
        }

        public float RateWanted
        {
            get => _rate;
            set
            {
                var last = _rate;

                if (value < 0) _rate = 0;
                else if (value > Props.powerPerTick) _rate = Props.powerPerTick;
                else _rate = value;

                if (last != _rate)
                {
                    _parent?.Changed();
                    Pawn.health.Notify_HediffChanged(parent);
                }
            }
        }

        public float Rate => ShouldConsume && _enabled ? _rate : 0f;

        public override string CompTipStringExtra => Powered ? null : "NoPower".Translate();

        public override void CompExposeData()
        {
            Scribe_References.Look(ref _parent, "partConsumerNetParent");
            Scribe_Values.Look(ref _enabled, "partConsumerPowered");
            Scribe_Values.Look(ref _powered, "partConsumerNetPowered");
            Scribe_Values.Look(ref _rate, "partConsumerRate", Props.powerPerTick);
            Scribe_Values.Look(ref _enableWhileDrafted, "partConsumerEnableWhileDrafted", true);
            Scribe_Values.Look(ref _enableWhileNotDrafted, "partConsumerEnableWhileNotDrafted", true);
            Scribe_Values.Look(ref _enableInCombat, "partConsumerEnableInCombat", true);
            Scribe_Values.Look(ref _enableOutOfCombat, "partConsumerEnableOutOfCombat", true);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                ConnectTo(_parent);
                PawnCombatHandler.Add(parent.pawn, this);
            }
        }

        public override string ToString()
        {
            return base.ToString() + " in " + parent;
        }

        public string Label => parent.Label;

        public void Drafted(bool drafted)
        {
            CheckStateChange(drafted, null);
        }

        public void InCombat(bool inCombat)
        {
            CheckStateChange(null, inCombat);
        }

        private void CheckStateChange(bool? drafted, bool? inCombat)
        {
            if (drafted == null) drafted = Pawn.Drafted;
            if (inCombat == null) inCombat = PawnCombatHandler.IsInCombat(Pawn);

            var combatWantOn = inCombat.Value && EnabledInCombat;
            var noCombatWantOn = !inCombat.Value && EnabledOutOfCombat;
            var draftWantOn = drafted.Value && EnableWhileDrafted;
            var noDraftWanton = !drafted.Value && EnabledWhileNotDrafted;

            Enabled = combatWantOn || noCombatWantOn || draftWantOn || noDraftWanton;
        }
    }
}
