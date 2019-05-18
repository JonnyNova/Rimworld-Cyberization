using FrontierDevelopments.Cyberization.Power;
using Verse;

namespace FrontierDevelopments.Cyberization.Parts
{
    public class AddedPartPowerConsumerProperties : PartPowerConsumerProperties
    {
        public AddedPartPowerConsumerProperties()
        {
            compClass = typeof(AddedPartPowerConsumer);
        }
    }

    public class AddedPartPowerConsumer : PartPowerConsumer, AddedPartEffectivenessModifier
    {
        public float PartEffectiveness => Powered ? 1f : 0f;
        
        public override string CompTipStringExtra => 
            Powered ? null : AddedPartEffectivenessModifierUtils.EffectivenessString("NoPower".Translate(), 0f);
    }
}