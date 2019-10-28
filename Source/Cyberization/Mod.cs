using System.Reflection;
using Harmony;
using UnityEngine;
using Verse;

namespace FrontierDevelopments.Cyberization
{
    public class Mod : Verse.Mod
    {
        public static Settings Settings;

        public Mod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<Settings>();

            var harmony = HarmonyInstance.Create("frontierdevelopment.cyberization");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public override string SettingsCategory()
        {
            return "Cyberization.ModName".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.DoWindowContents(inRect);
        }
    }
}