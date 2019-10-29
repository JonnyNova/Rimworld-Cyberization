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

        public float RateAvailable => _provider.RateAvailable;

        public float TotalAvailable => _provider.TotalAvailable;

        public float AmountAvailable => _provider.AmountAvailable;
        
        public float MaxRate => _provider.MaxRate;

        public AddedPartPowerProviderProperties Props => (AddedPartPowerProviderProperties) props;

        public void Tick()
        {
            _provider.Tick();
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            _provider.Tick();
        }

        public float Discharge => _provider.Discharge;


        public override string CompTipStringExtra => "Discharge: " + _provider.Discharge + "/" + Props.maxRate;

        public float Provide(float amount)
        {
            return _provider.Provide(amount);
        }

        public float Consume(float amount)
        {
            return _provider.Consume(amount);
        }

        public override void CompExposeData()
        {
            Scribe_Deep.Look(ref _provider, "provider");
        }
    }
}