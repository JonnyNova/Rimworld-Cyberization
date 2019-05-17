using FrontierDevelopments.Cyberization.Power;
using Verse;

namespace FrontierDevelopments.Cyberization.Parts
{
    public class AddedPartPowerProviderProperties : HediffCompProperties
    {
        public long maxEnergy;
        public long maxRate;
        
        public AddedPartPowerProviderProperties()
        {
            compClass = typeof(AddedPartPowerProvider);
        }
    }

    public class AddedPartPowerProvider : HediffComp, IPowerProvider
    {
        private PowerProvider _provider;
        
        public override void CompPostMake()
        {
            _provider = new PowerProvider(Props.maxEnergy, Props.maxRate, Props.maxEnergy);
        }

        public long Energy => _provider.Energy;

        public long MaxEnergy => _provider.MaxEnergy;

        public AddedPartPowerProviderProperties Props => (AddedPartPowerProviderProperties) props;

        public void Tick()
        {
            _provider.Tick();
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            _provider.Tick();
        }

        public long Discharge => _provider.Discharge;


        public override string CompTipStringExtra => "Discharge: " + _provider.Discharge + "/" + Props.maxRate;

        public bool ProvideEnergy(long amount)
        {
            return _provider.ProvideEnergy(amount);
        }

        public long Charge(long amount)
        {
            return _provider.Charge(amount);
        }

        public override void CompExposeData()
        {
            Scribe_Deep.Look(ref _provider, "provider");
        }
    }
}