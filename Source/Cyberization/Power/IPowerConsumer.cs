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
        
        IEnumerable<BodyPartTagDef> Tags { get; }
    }
}
