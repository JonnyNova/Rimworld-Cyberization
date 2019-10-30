using System.Collections.Generic;
using FrontierDevelopments.General.Energy;
using Verse;

namespace FrontierDevelopments.Cyberization.Power
{
    public interface IPowerConsumer : IEnergyConsumer
    {
        int Priority { get; }
        
        bool Powered { get; }
        
        IEnumerable<BodyPartTagDef> Tags { get; }
    }
}