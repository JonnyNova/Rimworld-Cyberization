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

        public long Energy => _provider.Energy;
        public long MaxEnergy => _provider.MaxEnergy;
        public long Discharge => _provider.Discharge;

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

        public bool ProvideEnergy(long amount)
        {
            return _provider.ProvideEnergy(amount);
        }

        public long Charge(long amount)
        {
            return _provider.Charge(amount);
        }

        public override void PostExposeData()
        {
            Scribe_Deep.Look(ref _provider, "provider");
        }
    }
}