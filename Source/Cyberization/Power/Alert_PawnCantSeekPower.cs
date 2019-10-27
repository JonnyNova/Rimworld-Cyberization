using System.Collections.Generic;
using System.Linq;
using FrontierDevelopments.Cyberization.Parts;
using Verse;

namespace FrontierDevelopments.Cyberization.Power
{
    public class Alert_PawnCantSeekPower : Alert_PartPowerBase
    {
        private List<Pawn> _needPower;
        private List<Pawn> _needPowerLive;
        private List<Pawn> _needPowerMove;

        protected override string MessageKey => "CantSeekPartPower";
        protected override List<Pawn> Users => _needPower;
        protected override List<Pawn> UsersMove => _needPowerMove;
        protected override List<Pawn> UsersLive => _needPowerLive;

        protected override void Update()
        {
            _needPower = PartPowerUsers.Where(pawn => ChargeSourceUtility.ClosestChargeSource(pawn) == null).ToList();
            _needPowerLive = _needPower.Where(PartUtility.RequiresPowerToLive).ToList();
            _needPowerMove = _needPower.Where(PartUtility.RequiresPowerForMovement).ToList();
        }
    }
}