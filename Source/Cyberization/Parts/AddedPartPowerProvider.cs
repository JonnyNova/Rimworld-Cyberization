using System.Linq;
using FrontierDevelopments.Cyberization.Power;
using FrontierDevelopments.General.Energy;
using Verse;
using IEnergyNet = FrontierDevelopments.General.IEnergyNet;

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

    public class AddedPartPowerProvider : HediffComp, IEnergyProvider
    {
        private PowerProvider _provider;
        
        public override void CompPostMake()
        {
            _provider = new PowerProvider(Props.maxEnergy, Props.maxRate, Props.maxEnergy);
            ConnectTo(parent.pawn.AllComps.OfType<IEnergyNet>().First());
        }

        public override void CompPostPostRemoved()
        {
            _provider.Removed();
        }

        public IEnergyNet Parent => _provider.Parent;

        public float RateAvailable => _provider.RateAvailable;

        public float TotalAvailable => _provider.TotalAvailable;

        public float AmountAvailable => _provider.AmountAvailable;
        
        public float MaxRate => _provider.MaxRate;

        private AddedPartPowerProviderProperties Props => (AddedPartPowerProviderProperties) props;

        public void ConnectTo(IEnergyNet net)
        {
            _provider.ConnectTo(net);
        }

        public void Update()
        {
            _provider.Update();
        }

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

        public override string ToString()
        {
            return base.ToString() + " in " + parent;
        }
    }
}