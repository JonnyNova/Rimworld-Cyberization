using System.Collections.Generic;
using System.Linq;
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
        private bool _powered = true;
        private bool _override = false;

        PartPowerConsumerProperties Props => (PartPowerConsumerProperties) props;

        public bool ShouldConsume => Props.essential || !(Pawn.Downed || Pawn.InBed());

        public bool Powered => _powered && !_override;

        public int Priority => Props.priority;

        public IEnumerable<BodyPartTagDef> Tags => parent.Part.def.tags;

        public override string CompTipStringExtra => Powered ? null : "NoPower".Translate();

        // Used when checking capabilities
        public void OverridePowered(bool set)
        {
            // intentionally opposite here
            _override = !set;
        }
        public void PowerTick()
        {
            if (ShouldConsume)
            {
                var last = _powered;
                _powered = PowerProvider.Providers(Pawn).Any(provider => provider.ProvideEnergy(Props.powerPerTick));
                if(last != _powered) Pawn.health.Notify_HediffChanged(parent);
            }
        }

        

        public override void CompExposeData()
        {
            Scribe_Values.Look(ref _powered, "powered");
        }
    }

    
}