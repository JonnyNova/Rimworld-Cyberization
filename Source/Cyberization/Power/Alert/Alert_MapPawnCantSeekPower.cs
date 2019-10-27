using System.Collections.Generic;
using System.Linq;
using FrontierDevelopments.Cyberization.Parts;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Cyberization.Power.Alert
{
    public class Alert_MapPawnCantSeekPower : Alert_MapPartPowerBase
    {
        private List<Pawn> _needPower;
        private List<Pawn> _needPowerLive;
        private List<Pawn> _needPowerMove;

        protected override string MessageType => "Map";
        protected override string MessageKey => "CantSeekPartPower";

        protected override int UsersCount => _needPower.Count;
        protected override int UsersMoveCount => _needPowerMove.Count;
        protected override int UsersLiveCount => _needPowerLive.Count;
        
        public override AlertReport GetReport()
        {
            return AlertReport.CulpritsAre(_needPower);
        }

        protected override void Update()
        {
            _needPower = PartPowerUsers.Where(pawn => ChargeSourceUtility.ClosestChargeSource(pawn) == null).ToList();
            _needPowerLive = _needPower.Where(PartUtility.RequiresPartsToLive<AddedPartPowerConsumer>).ToList();
            _needPowerMove = _needPower.Where(PartUtility.RequiresPartsForMovement<AddedPartPowerConsumer>).ToList();
        }
    }
}