using System.Linq;
using FrontierDevelopments.General.Energy;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Cyberization.Parts
{
    public class Harmony_PawnOverlay
    {
        private static bool HasBrokenDownPart(Pawn pawn)
        {
            return pawn.health.hediffSet.hediffs
                .OfType<HediffWithComps>()
                .SelectMany(hediff => hediff.comps)
                .OfType<AddedPartBreakdownable>()
                .Any(part => part.IsBrokenDown);
        }
        
        private static bool PowerEmpty(Pawn pawn)
        {
            var net = pawn.TryGetComp<CompEnergyNet>();
            return net != null && net.AmountAvailable <= 0 && net.Nodes.Any();
        }

        [HarmonyPatch(typeof(ThingWithComps), nameof(ThingWithComps.Draw))]
        static class Patch_ThingWithComps_Draw
        {
            [HarmonyPostfix]
            private static void AddPartIcons(ThingWithComps __instance)
            {
                switch (__instance)
                {
                    case Pawn pawn:
                        if (HasBrokenDownPart(pawn))
                        {
                            pawn.Map.overlayDrawer.DrawOverlay(pawn, OverlayTypes.BrokenDown);
                        }
                        if (PowerEmpty(pawn))
                        {
                            pawn.Map.overlayDrawer.DrawOverlay(pawn, OverlayTypes.NeedsPower);
                        }
                        break;
                }
            }
        }
    }
}