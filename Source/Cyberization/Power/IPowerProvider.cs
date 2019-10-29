using System.Collections.Generic;
using System.Linq;
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
        // TODO rules for what is selected in what order
        // TODO cache for each pawn
        public static IEnumerable<IPowerProvider> Providers(Pawn pawn)
        {
            var result = new List<IPowerProvider>();
            
            // TODO this list is probably not stable
            var items = pawn.inventory?.innerContainer
                .OfType<ThingWithComps>()
                .SelectMany(thing => thing.AllComps)
                .OfType<IPowerProvider>();
            if(items != null) result.AddRange(items);

                // TODO this list is probably not stable
            var apparelProviders = pawn.apparel?.WornApparel
                .SelectMany(apparel => apparel.AllComps)
                .OfType<IPowerProvider>();
            if(apparelProviders != null) result.AddRange(apparelProviders);

            // TODO this list is probably not stable
            var hediffProviders = pawn.health?.hediffSet?.hediffs?
                .OfType<HediffWithComps>()
                .SelectMany(hediff => hediff.comps)
                .OfType<IPowerProvider>();
            if(hediffProviders != null) result.AddRange(hediffProviders);
            return result;
        }

        public static float TotalEnergy(Pawn pawn)
        {
            return Providers(pawn).Aggregate(0f, (sum, provider) => sum + provider.RateAvailable);
        }

        public static float TotalMaxEnergy(Pawn pawn)
        {
            return Providers(pawn).Aggregate(0f, (sum, provider) => sum + provider.TotalAvailable);
        }

        public static float TotalEnergyPercent(Pawn pawn)
        {
            return 1.0f * TotalEnergy(pawn) / TotalMaxEnergy(pawn);
        }

        private float _energy;
        private float _maxEnergy;
        private float _maxRate;

        private float _drawThisTick;

        public float RateAvailable => _energy;

        public float TotalAvailable => _maxEnergy;

        public float Discharge => _drawThisTick;

        public float AmountAvailable => _maxEnergy;
        
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