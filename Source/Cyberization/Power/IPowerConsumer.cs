using System.Collections.Generic;
using FrontierDevelopments.General;
using FrontierDevelopments.General.Energy;
using Verse;

namespace FrontierDevelopments.Cyberization.Power
{
    public interface IPowerConsumer : IEnergyConsumer, ILabeled
    {
        int Priority { get; }

        bool Essential { get; }

        bool Powered { get; }
        
        bool Enabled { get; set; }

        bool EnableWhileDrafted { get; set; }

        bool EnabledWhileNotDrafted { get; set; }

        bool EnabledInCombat { get; set; }

        bool EnabledOutOfCombat { get; set; }

        float RateWanted { get; set; }

        float MaxRate { get; }

        float Efficiency { get; }

        IEnumerable<BodyPartTagDef> Tags { get; }
    }
}
