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
        private bool? _overrideState;

        public float PartEffectiveness
        {
            get
            {
                if (!Mod.Settings.UsePartPower) return 1f;
                if (_overrideState != null) return _overrideState.Value ? 1f : 0f;
                return Powered ? Efficiency : 0f;
            }
        }

        public void OverrideEffectivenessState(bool? state)
        {
            _overrideState = state;
        }

        public override string CompTipStringExtra => 
            Powered ? null : AddedPartEffectivenessModifierUtils.EffectivenessString("NoPower".Translate(), 0f);
    }
}