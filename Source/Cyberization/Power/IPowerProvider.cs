using System.Collections.Generic;
using System.Linq;
using Harmony;
using Verse;

namespace FrontierDevelopments.Cyberization.Power
{
    public interface IPowerProvider
    {
        long Energy { get; }
        long MaxEnergy { get; }
        long Discharge { get; }

        void Tick();
        
        bool ProvideEnergy(long amount);
        long Charge(long amount);
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

        private long _energy;
        private long _maxEnergy;
        private long _maxRate;

        private long _discharging;

        public long Energy => _energy;

        public long MaxEnergy => _maxEnergy;

        public long Discharge => _discharging;

        public PowerProvider()
        {
            
        }
        
        public PowerProvider(long maxEnergy, long maxRate, long energy)
        {
            _maxEnergy = maxEnergy;
            _maxRate = maxRate;
            _energy = energy;
        }

        public bool ProvideEnergy(long amount)
        {
            if (_discharging + amount > _maxRate) return false;
            if (_energy - amount < 0) return false;
            _discharging += amount;
            _energy -= amount;
            return true;
        }

        public long Charge(long amount)
        {
            // discharging is reset to allow this to pass the discharge along to this source
            
            if (_energy + amount > _maxEnergy)
            {
                _energy = _maxEnergy;
                var difference = _maxEnergy - _energy;
                _discharging -= difference;
                return difference;
            }

            _energy += amount;
            _discharging -= amount;

            return amount;
        }

        public void Tick()
        {
            _discharging = 0;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref _maxEnergy, "maxEnergy");
            Scribe_Values.Look(ref _maxRate, "maxRate");
            Scribe_Values.Look(ref _energy, "energy");
        }
    }
}