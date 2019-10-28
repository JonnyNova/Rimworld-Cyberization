using UnityEngine;
using Verse;

namespace FrontierDevelopments.Cyberization
{
    public class Settings : ModSettings
    {
        public bool UsePartPower = true;
        public bool UsePartMaintenance = true;
        public bool UsePartBreakdowns = true;

        public float ElectricRatio = 1;

        public float SeekPowerPercent = 0.3f;
        public float SeekPowerChargeTo = 0.9f;

        public float SeekMaintenancePercent = 0.5f;
        public float SatisfiedMaintenancePercent = 0.99f;

        public void DoWindowContents(Rect inRect)
        {
            var list = new Listing_Standard();
            list.Begin(inRect);

            list.CheckboxLabeled(
                "Cyberization.Settings.Power.Enable".Translate(),
                ref UsePartPower);

            list.CheckboxLabeled(
                "Cyberization.Settings.Maintenance.Enable".Translate(),
                ref UsePartMaintenance);

            list.CheckboxLabeled(
                "Cyberization.Settings.Breakdowns.Enable".Translate(),
                ref UsePartBreakdowns);

            if (UsePartPower)
            {
                var electricRatioBuffer = ElectricRatio.ToString();
                Widgets.TextFieldNumericLabeled(
                    list.GetRect(Text.LineHeight),
                    "Cyberization.Settings.Power.ElectricRatio".Translate(),
                    ref ElectricRatio,
                    ref electricRatioBuffer);

                list.Label("Cyberization.Settings.Power.SeekPowerPercent".Translate() + ": " + SeekPowerPercent * 100 + "%");
                SeekPowerPercent = list.Slider(SeekPowerPercent, 0f, 1f);

                list.Label("Cyberization.Settings.Power.SeekPowerChargeTo".Translate() + ": " + SeekPowerChargeTo * 100 + "%");
                SeekPowerChargeTo = list.Slider(SeekPowerChargeTo, 0f, 1f);
            }

            if (UsePartMaintenance)
            {
                list.Label("Cyberization.Settings.Maintenance.SeekMaintenancePercent".Translate() + ": " + SeekMaintenancePercent * 100 + "%");
                SeekMaintenancePercent = list.Slider(SeekMaintenancePercent, 0f, 1f);

                list.Label("Cyberization.Settings.Maintenance.SatisfiedMaintenancePercent".Translate() + ": " + SatisfiedMaintenancePercent * 100 + "%");
                SatisfiedMaintenancePercent = list.Slider(SatisfiedMaintenancePercent, 0f, 1f);
            }

            list.End();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref UsePartPower, "usePowerPower", true);
            Scribe_Values.Look(ref UsePartMaintenance, "usePartMaintenance", true);
            Scribe_Values.Look(ref UsePartBreakdowns, "usePartBreakdowns", true);

            Scribe_Values.Look(ref ElectricRatio, "electricRatio", 1f);

            Scribe_Values.Look(ref SeekPowerPercent, "SeekPowerPercent", 0.3f);
            Scribe_Values.Look(ref SeekPowerChargeTo, "SeekPowerChargeTo", 0.9f);

            Scribe_Values.Look(ref SeekMaintenancePercent, "SeekMaintenancePercent", 0.5f);
            Scribe_Values.Look(ref SatisfiedMaintenancePercent, "SatisfiedMaintenancePercent", 0.99f);
        }
    }
}