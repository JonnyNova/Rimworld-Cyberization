using Verse;

namespace FrontierDevelopments.Cyberization
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

        public CompPowerProviderProperties Props => (CompPowerProviderProperties) props;

        public long Energy => _provider.Energy;
        public long MaxEnergy => _provider.MaxEnergy;

        public long Discharge => _provider.Discharge;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            _provider = new PowerProvider(Props.maxEnergy, Props.maxRate, Props.maxEnergy);
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