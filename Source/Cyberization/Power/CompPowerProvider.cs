using FrontierDevelopments.General;
using FrontierDevelopments.General.Energy;
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

    public class CompPowerProvider : ThingComp, IEnergyProvider
    {
        private PowerProvider _provider;

        public float RateAvailable => _provider.RateAvailable;
        public float TotalAvailable => _provider.TotalAvailable;
        public float AmountAvailable => _provider.AmountAvailable;
        public float MaxRate => _provider.MaxRate;
        public float Discharge => _provider.Discharge;

        public IEnergyNet Parent => _provider.Parent;

        public void ConnectTo(IEnergyNet net)
        {
            _provider.ConnectTo(net);
        }

        public override void Initialize(CompProperties props)
        {
            var providerProps = (CompPowerProviderProperties) props;
            _provider = new PowerProvider(providerProps.maxEnergy, providerProps.maxRate, providerProps.maxEnergy);
        }

        public void Update()
        {
            _provider.Update();
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