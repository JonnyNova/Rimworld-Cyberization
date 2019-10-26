using System.Collections.Generic;
using System.Linq;
using System.Text;
using FrontierDevelopments.Cyberization.Parts;
using RimWorld;
using UnityEngine;
using Verse;

namespace FrontierDevelopments.Cyberization.Power
{
    public class Alert_PawnCantSeekPower : Alert
    {
        private enum AlertSeverity
        {
            Normal,
            Rescue,
            Fatal
        }
        
        private List<Pawn> _needPower;
        private List<Pawn> _needPowerLive;
        private List<Pawn> _needPowerMove;
        
        private static bool UnableToReachCharger(Pawn pawn)
        {
            return ChargeSourceUtility.ClosestChargeSource(pawn) == null;
        }

        public override bool Active
        {
            get
            {
                _needPower = Find.Maps
                    .SelectMany(map => map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer))
                    .Where(PartUtility.HasPoweredParts)
                    .Where(UnableToReachCharger)
                    .ToList();
                
                _needPowerLive = _needPower.Where(PartUtility.RequiresPowerToLive).ToList();
                _needPowerMove = _needPower.Where(PartUtility.RequiresPowerForMovement).ToList();
                
                return _needPower.Any();
            }
        }

        public override string GetLabel()
        {
            return "Cyberization.AlertPartPower.Label".Translate()
                .Replace("{0}", _needPower.Count.ToString());
        }

        public override string GetExplanation()
        {
            var result = new StringBuilder();

            result.AppendLine("Cyberization.AlertPartPower.Text".Translate());
            result.AppendLine();

            if (_needPowerLive.Any())
            {
                result.AppendLine("Cyberization.AlertPartPower.Fatal".Translate()
                    .Replace("{0}", _needPowerLive.Count.ToString()));
            }

            if (_needPowerMove.Any())
            {
                result.AppendLine("Cyberization.AlertPartPower.Rescue".Translate()
                    .Replace("{0}", _needPowerMove.Count.ToString()));
            }

            var remaining = _needPower.Count - (_needPowerLive.Count + _needPowerMove.Count);
            if (remaining > 0)
            {
                result.AppendLine("Cyberization.AlertPartPower.Remaining".Translate()
                    .Replace("{0}", remaining.ToString()));
            }
            
            return result.ToString().Trim();
        }

        public override AlertReport GetReport()
        {
            return AlertReport.CulpritsAre(_needPower);
        }

        protected override Color BGColor
        {
            get
            {
                switch (Severity)
                {
                    case AlertSeverity.Fatal: return Color.red;
                    case AlertSeverity.Rescue: return Color.grey;
                    default: return base.BGColor;
                }
            }
        }

        private AlertSeverity Severity
        {
            get
            {
                if (_needPowerLive.Any()) return AlertSeverity.Fatal;
                if (_needPowerMove.Any()) return AlertSeverity.Rescue;
                return AlertSeverity.Normal;
            }
        }
    }
}