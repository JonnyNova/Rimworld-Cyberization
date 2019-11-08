using System.Linq;
using FrontierDevelopments.Cyberization.Power;
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

    public class AddedPartPowerProvider : HediffComp, IPowerProvider
    {
        private PowerProvider _provider;
        
        public override void CompPostMake()
        {
            _provider = new PowerProvider(Props.maxEnergy, Props.maxRate, Props.maxEnergy, this);
            ConnectTo(parent.pawn.AllComps.OfType<IEnergyNet>().First());
        }

        public override void CompPostPostRemoved()
        {
            _provider.Disconnect();
        }

        public IEnergyNet Parent => _provider.Parent;

        public float RateAvailable => _provider.RateAvailable;

        public float TotalAvailable => _provider.TotalAvailable;

        public float AmountAvailable => _provider.AmountAvailable;
        
        public float MaxRate => _provider.MaxRate;

        public string Label => parent.Label;

        private AddedPartPowerProviderProperties Props => (AddedPartPowerProviderProperties) props;

        public void ConnectTo(IEnergyNet net)
        {
            _provider.ConnectTo(net);
        }

        public void Disconnect()
        {
            _provider.Disconnect();
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

        public float Request(float amount)
        {
            return _provider.Request(amount);
        }

        public override void CompExposeData()
        {
            Scribe_Deep.Look(ref _provider, "provider");
            
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                _provider.Labeled = this;
            }
        }

        public override string ToString()
        {
            return base.ToString() + " in " + parent;
        }
    }
}