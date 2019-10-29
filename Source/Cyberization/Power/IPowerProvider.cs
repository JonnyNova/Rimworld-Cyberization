using FrontierDevelopments.General.Energy;
using Verse;

namespace FrontierDevelopments.Cyberization.Power
{
    public interface IPowerProvider : IEnergyNode
    {
        void Tick();
    }

    public class PowerProvider : IPowerProvider, IExposable
    {
        private float _energy;
        private float _maxEnergy;
        private float _maxRate;

        private float _drawThisTick;

        public float RateAvailable => _maxRate - _drawThisTick;

        public float TotalAvailable => _maxEnergy;

        public float Discharge => _drawThisTick;

        public float AmountAvailable => _energy;
        
        public float MaxRate => _maxRate;

        public PowerProvider()
        {
            
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
            _drawThisTick += amount;
            _energy -= amount;
            return amount;
        }

        public void Tick()
        {
            _drawThisTick = 0;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref _maxEnergy, "maxEnergy");
            Scribe_Values.Look(ref _maxRate, "maxRate");
            Scribe_Values.Look(ref _energy, "energy");
        }
    }
}