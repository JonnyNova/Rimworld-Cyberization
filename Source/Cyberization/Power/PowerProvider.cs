using System;
using FrontierDevelopments.General;
using FrontierDevelopments.General.Energy;
using Verse;

namespace FrontierDevelopments.Cyberization.Power
{
    public class PowerProvider : IEnergyProvider, IExposable
    {
        private float _energy;
        private float _maxEnergy;
        private float _maxRate;

        private float _drawThisTick;

        private IEnergyNet _parent;

        public float RateAvailable => Math.Min(_maxRate - _drawThisTick, AmountAvailable);

        public float TotalAvailable => _maxEnergy;

        public float Discharge => _drawThisTick;

        public float AmountAvailable => _energy;
        
        public float MaxRate => _maxRate;

        public void ConnectTo(IEnergyNet net)
        {
            _parent?.Disconnect(this);
            _parent = net;
            _parent?.Connect(this);
        }

        public General.IEnergyNet Parent => _parent;

        public PowerProvider()
        {
            
        }

        public void Removed()
        {
            _parent?.Disconnect(this);
        }
        
        public PowerProvider(long maxEnergy, long maxRate, long energy)
        {
            _maxEnergy = maxEnergy;
            _maxRate = maxRate;
            _energy = energy;
        }

        public float Provide(float amount)
        {
            if (amount + _energy > _maxEnergy)
            {
                _energy = _maxEnergy;
                return  _maxEnergy - _energy;
            }

            _energy += amount;
            return amount;
        }

        public float Consume(float amount)
        {
            if (amount > RateAvailable) amount = RateAvailable;
            if (amount > AmountAvailable) amount = AmountAvailable;
            _drawThisTick += amount;
            _energy -= amount;
            return amount;
        }

        public void Update()
        {
            _drawThisTick = 0;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref _maxEnergy, "maxEnergy");
            Scribe_Values.Look(ref _maxRate, "maxRate");
            Scribe_Values.Look(ref _energy, "energy");
            Scribe_Values.Look(ref _drawThisTick, "drawThisTick");
            
            Scribe_References.Look(ref _parent, "powerProviderNetParent");
            
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                ConnectTo(_parent);
            }
        }
    }
}