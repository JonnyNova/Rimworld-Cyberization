using System.Collections.Generic;
using System.Linq;
using FrontierDevelopments.Cyberization.Parts;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace FrontierDevelopments.Cyberization.Power.Alert
{
    public class Alert_CaravanCantSeekPartPower : Alert_PartPowerBase
    {
        private List<Caravan> _needPower;
        private List<Caravan> _needPowerLive;
        private List<Caravan> _needPowerMove;

        protected override string MessageType => "Caravan";
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
            _needPower = CacheCaravanPartUser.PartPowerCaravans
                .Where(caravan => !ChargeSourceUtility.FindSources(caravan).Any())
                .ToList();
            _needPowerMove = _needPower.Where(caravan => caravan.pawns.Any<Pawn>(PartUtility.RequiresPartsForMovement<AddedPartPowerConsumer>)).ToList();
            _needPowerLive = _needPower.Where(caravan => caravan.pawns.Any<Pawn>(PartUtility.RequiresPartsToLive<AddedPartPowerConsumer>)).ToList();
        }
    }
}