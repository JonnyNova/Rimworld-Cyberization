using System.Collections.Generic;
using System.Linq;
using FrontierDevelopments.Cyberization.Parts;
using Verse;

namespace FrontierDevelopments.Cyberization.Power
{
    public class Alert_PartPowerLow : Alert_PartPowerBase
    {
        private List<Pawn> _lowPower;
        private List<Pawn> _lowPowerMove;
        private List<Pawn> _lowPowerLive;

        protected override string MessageKey => "LowPartPower";
        protected override List<Pawn> Users => _lowPower;
        protected override List<Pawn> UsersMove => _lowPowerMove;
        protected override List<Pawn> UsersLive => _lowPowerLive;

        protected override void Update()
        {
            _lowPower = PartPowerUsers
                .Where(pawn => PowerProvider.TotalEnergyPercent(pawn) < Settings.SeekPowerPercent)
                .ToList();
            _lowPowerLive = _lowPower.Where(PartUtility.RequiresPowerToLive).ToList();
            _lowPowerMove = _lowPower.Where(PartUtility.RequiresPowerForMovement).ToList();
        }
    }
}