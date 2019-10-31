using System.Linq;
using FrontierDevelopments.General;
using FrontierDevelopments.General.Energy;
using Harmony;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Cyberization.Power
{
    public class PawnPartPowerNet
    {
        public static IEnergyNet Get(Pawn pawn)
        {
            return pawn.AllComps.OfType<IEnergyNet>().First();
        }
    }

    [HarmonyPatch(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Notify_ApparelAdded))]
    static class PawnPartPowerNet_Add_Apparel
    {
        [HarmonyPostfix]
        static void AddToPawnPartPowerNet(Pawn_ApparelTracker __instance, Apparel apparel)
        {
            var net = PawnPartPowerNet.Get(__instance.pawn);
            apparel.AllComps
                .OfType<IEnergyNode>()
                .Do(node => node.ConnectTo(net));
        }
    }

    [HarmonyPatch(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Notify_ApparelRemoved))]
    static class PawnPartPowerNet_Remove_Apparel
    {
        [HarmonyPostfix]
        static void RemoveFromPawnPartPowerNet(Pawn_ApparelTracker __instance, Apparel apparel)
        {
            var net = PawnPartPowerNet.Get(__instance.pawn);
            apparel.AllComps
                .OfType<IEnergyNode>()
                .Do(node => node.Disconnect());
        }
    }

}