using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace FrontierDevelopments.Cyberization.Power
{
    public abstract class Alert_PartPowerBase : Alert
    {
        protected abstract string MessageKey { get; }

        protected abstract List<Pawn> Users { get; }
        protected abstract List<Pawn> UsersMove { get; }
        protected abstract List<Pawn> UsersLive { get; }

        protected abstract void Update();

        protected enum AlertSeverity
        {
            Normal,
            Rescue,
            Fatal
        }
        
        protected AlertSeverity Severity
        {
            get
            {
                if (UsersLive.Any()) return AlertSeverity.Fatal;
                if (UsersMove.Any()) return AlertSeverity.Rescue;
                return AlertSeverity.Normal;
            }
        }

        public override bool Active
        {
            get
            {
                Update();
                return Users.Any();
            }
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
        
        public override AlertReport GetReport()
        {
            return AlertReport.CulpritsAre(Users);
        }
        
        public override string GetLabel()
        {
            return ("Cyberization.Alert." + MessageKey +".Label").Translate()
                .Replace("{0}", Users.Count.ToString());
        }

        public override string GetExplanation()
        {
            var result = new StringBuilder();

            result.AppendLine(("Cyberization.Alert." + MessageKey + ".Text").Translate());
            result.AppendLine();

            if (UsersLive.Any())
            {
                result.AppendLine("Cyberization.Alert.PartPower.Fatal".Translate()
                    .Replace("{0}", UsersLive.Count.ToString()));
            }

            if (UsersMove.Any())
            {
                result.AppendLine("Cyberization.Alert.PartPower.Rescue".Translate()
                    .Replace("{0}", UsersMove.Count.ToString()));
            }

            var remaining = Users.Count - (UsersLive.Count + UsersMove.Count);
            if (remaining > 0)
            {
                result.AppendLine("Cyberization.Alert.PartPower.Remaining".Translate()
                    .Replace("{0}", remaining.ToString()));
            }
            
            return result.ToString().Trim();
        }

        private static IEnumerable<PartPowerUserCache> Caches => Find.Maps.Select(map => map.GetComponent<PartPowerUserCache>());
        protected static IEnumerable<Pawn> PartPowerUsers => Caches.SelectMany(cache => cache.PartPowerPowerUsers);
    }
}