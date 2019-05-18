using RimWorld;

namespace FrontierDevelopments.Cyberization.Power
{
    public interface IChargeSource
    {
        bool Available { get; }
        int Rate { get; }
        Faction Faction { get; }
    }
}