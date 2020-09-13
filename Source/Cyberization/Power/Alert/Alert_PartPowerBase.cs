using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace FrontierDevelopments.Cyberization.Power.Alert
{
    public abstract class Alert_PartPowerBase : RimWorld.Alert
    {
        protected abstract string MessageType { get; }
        protected abstract string MessageKey { get; }

        protected abstract int UsersCount { get; }
        protected abstract int UsersMoveCount { get; }
        protected abstract int UsersLiveCount { get; }

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
                if (UsersLiveCount > 0) return AlertSeverity.Fatal;
                if (UsersMoveCount > 0) return AlertSeverity.Rescue;
                return AlertSeverity.Normal;
            }
        }

        public override AlertReport GetReport()
        {
            Update();
            return Mod.Settings.UsePartPower && UsersCount > 0;
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

        public override string GetLabel()
        {
            return ("Cyberization.Alert." + MessageType + "." + MessageKey +".Label").Translate()
                .Replace("{0}", UsersCount.ToString());
        }

        public override TaggedString GetExplanation()
        {
            var result = new StringBuilder();

            result.AppendLine(("Cyberization.Alert." + MessageType + "." + MessageKey + ".Text").Translate());
            result.AppendLine();

            if (UsersLiveCount > 0)
            {
                result.AppendLine(("Cyberization.Alert." + MessageType + ".PartPower.Fatal").Translate()
                    .Replace("{0}", UsersLiveCount.ToString()));
            }

            if (UsersMoveCount > 0)
            {
                result.AppendLine(("Cyberization.Alert." + MessageType + ".PartPower.Rescue").Translate()
                    .Replace("{0}", UsersMoveCount.ToString()));
            }

            var remaining = UsersCount - (UsersLiveCount + UsersMoveCount);
            if (remaining > 0)
            {
                result.AppendLine(("Cyberization.Alert." + MessageType + ".PartPower.Remaining").Translate()
                    .Replace("{0}", remaining.ToString()));
            }
            
            return result.ToString().Trim();
        }
    }
}