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

        private IPowerConsumer Consumer
        {
            get
            {
                // this exists due to the comp not being ready when loading a save
                if (_consumer == null)
                {
                    _consumer = comps.OfType<IPowerConsumer>().First();
                }

                return _consumer;
            }
        }

        public override int CurStageIndex => Consumer?.Powered ?? false ? 1 : 0;
    }
}