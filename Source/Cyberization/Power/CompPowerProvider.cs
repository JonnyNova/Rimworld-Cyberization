using Verse;

namespace FrontierDevelopments.Cyberization.Power
{
    public class CompPowerProviderProperties : CompProperties
    {
        public long maxEnergy;
        public long maxRate;
        
        public CompPowerProviderProperties()
        {
            compClass = typeof(CompPowerProvider);
        }
    }

    public class CompPowerProvider : ThingComp, IPowerProvider
    {
        private PowerProvider _provider;

        public float RateAvailable => _provider.RateAvailable;
        public float TotalAvailable => _provider.TotalAvailable;
        public float AmountAvailable => _provider.AmountAvailable;
        public float MaxRate => _provider.MaxRate;
        public float Discharge => _provider.Discharge;

        public override void Initialize(CompProperties props)
        {
            var providerProps = (CompPowerProviderProperties) props;
            _provider = new PowerProvider(providerProps.maxEnergy, providerProps.maxRate, providerProps.maxEnergy);
        }

        public override void CompTick()
        {
            _provider.Tick();
        }

        public void Tick()
        {
            _provider.Tick();
        }

        public float Provide(float amount)
        {
            return _provider.Provide(amount);
        }

        public float Consume(float amount)
        {
            return _provider.Consume(amount);
        }

        public override void PostExposeData()
        {
            Scribe_Deep.Look(ref _provider, "provider");
        }
    }
}