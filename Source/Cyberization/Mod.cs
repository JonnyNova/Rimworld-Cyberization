using System.Reflection;
using Harmony;
using Verse;

namespace FrontierDevelopments.Cyberization
{
    public class Mod : Verse.Mod
    {
        public Mod(ModContentPack content) : base(content)
        {
            var harmony = HarmonyInstance.Create("frontierdevelopment.cyberization");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}