using System.Collections.Generic;
using System.Linq;
using FrontierDevelopments.Cyberization.Parts;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Cyberization.Power.Alert
{
    public class Alert_MapPartPowerLow : Alert_MapPartPowerBase
    {
        private List<Pawn> _lowPower;
        private List<Pawn> _lowPowerMove;
        private List<Pawn> _lowPowerLive;

        protected override string MessageType => "Map";
        protected override string MessageKey => "LowPartPower";

        protected override int UsersCount => _lowPower.Count;
        protected override int UsersMoveCount => _lowPowerMove.Count;
        protected override int UsersLiveCount => _lowPowerLive.Count;

        public override AlertReport GetReport()
        {
            return AlertReport.CulpritsAre(_lowPower);
        }
        
        protected override void Update()
        {
            _lowPower = PartPowerUsers
                .Where(pawn => pawn.needs.TryGetNeed<PartEnergyNeed>().SeekSatisfaction)
                .ToList();
            _lowPowerLive = _lowPower.Where(PartUtility.RequiresPartsToLive<AddedPartPowerConsumer>).ToList();
            _lowPowerMove = _lowPower.Where(PartUtility.RequiresPartsForMovement<AddedPartPowerConsumer>).ToList();
        }
    }
}