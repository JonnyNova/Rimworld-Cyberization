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
        private bool _disabled;

        public float PartEffectiveness
        {
            get
            {
                if (!Mod.Settings.UsePartPower) return 1f;
                return Powered && !_disabled ? Efficiency : 0f;
            }
        }

        public void SetDisabled(bool disabled)
        {
            _disabled = disabled;
        }

        public override string CompTipStringExtra => 
            Powered ? null : AddedPartEffectivenessModifierUtils.EffectivenessString("NoPower".Translate(), 0f);
    }
}