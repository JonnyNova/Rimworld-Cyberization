using System.Linq;
using FrontierDevelopments.Cyberization.Power;
using Verse;

namespace FrontierDevelopments.Cyberization.Implants
{
    public class ImplantPowered : Hediff_Implant
    {
        private IPowerConsumer _consumer;
        
        // not used. allows setting negative values to reduce XML
        public override float Severity { get; set; }

        public IPowerConsumer Consumer => _consumer;

        public override void PostMake()
        {
            base.PostMake();
            _consumer = comps.OfType<IPowerConsumer>().First();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
                _consumer = comps.OfType<IPowerConsumer>().First();
        }

        public override int CurStageIndex => Consumer?.Powered ?? false ? 1 : 0;
    }
}