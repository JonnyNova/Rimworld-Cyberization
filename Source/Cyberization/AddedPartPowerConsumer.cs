using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Cyberization
{
    public class AddedPartPowerConsumerProperties : HediffCompProperties
    {
        public int powerPerTick = 1;
        public int priority = 5;
        public bool essential;
        
        public AddedPartPowerConsumerProperties()
        {
            compClass = typeof(AddedPartPowerConsumer);
        }
    }

    public class AddedPartPowerConsumer : HediffComp, AddedPartEffectivenessModifier
    {
        // TODO cache
        public static IEnumerable<AddedPartPowerConsumer> All(Pawn pawn)
        {
            return pawn.health.hediffSet.hediffs
                .OfType<HediffWithComps>()
                .SelectMany(h => h.comps)
                .OfType<AddedPartPowerConsumer>();
        }
        
        private bool _powered = true;

        public AddedPartPowerConsumerProperties Props => (AddedPartPowerConsumerProperties) props;

        public bool ShouldConsume => Props.essential || !(Pawn.Downed || Pawn.InBed());

        public bool Powered => _powered;

        public void Tick()
        {
            if (ShouldConsume)
            {
                var last = _powered;
                _powered = PowerProvider.Providers(Pawn).Any(provider => provider.ProvideEnergy(Props.powerPerTick));
                if(last != _powered) Pawn.health.Notify_HediffChanged(parent);
            }
        }

        public float PartEffectiveness => _powered ? 1f : 0f;

        public override string CompTipStringExtra => 
            _powered ? null : AddedPartEffectivenessModifierUtils.EffectivenessString("NoPower".Translate(), 0f);

        public override void CompExposeData()
        {
            Scribe_Values.Look(ref _powered, "powered");
        }
    }
}