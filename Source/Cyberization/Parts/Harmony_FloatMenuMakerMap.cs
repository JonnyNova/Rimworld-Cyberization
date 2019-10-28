using System.Collections.Generic;
using System.Linq;
using FrontierDevelopments.Cyberization.Power;
using Harmony;
using RimWorld;
using UnityEngine;
using Verse;

namespace FrontierDevelopments.Cyberization.Parts
{
    public class Harmony_FloatMenuMakerMap
    {
        [HarmonyPatch(typeof(FloatMenuMakerMap), nameof(FloatMenuMakerMap.ChoicesAtFor))]
        static class Patch_FloatMenuMakerMap_ChoicesAtFor
        {
            private static readonly TargetingParameters ForChargers = new TargetingParameters
            {
                canTargetBuildings = true,
                canTargetPawns = false,
                canTargetFires = false,
                canTargetItems = false,
                canTargetLocations = false,
                canTargetSelf = false,
                mapObjectTargetsMustBeAutoAttackable = false,
                validator = info => info.HasThing && ChargeSourceUtility.FindSources(info.Thing).Any(source => source.Available)
            };

            private static FloatMenuOption CreateMenuOption(Thing source, Pawn pawn, LocalTargetInfo target)
            {
                return FloatMenuUtility.DecoratePrioritizedTask(
                    new FloatMenuOption(
                        "Cyberization.Menu.PrioritizeCharging".Translate()
                            .Replace("{0}", source.Label),
                        () => pawn.jobs.TryTakeOrderedJob(
                            new Verse.AI.Job(CyberizationDefOf.SeekPartPower, source)),
                        MenuOptionPriority.Default,
                        null,
                        source
                    ),
                    pawn,
                    target);
            }

            [HarmonyPostfix]
            static List<FloatMenuOption> AddPrioritizingPartPowerOption(List<FloatMenuOption> __result, Vector3 clickPos, Pawn pawn)
            {
                if (pawn.Downed || pawn.needs.TryGetNeed<PartEnergyNeed>() == null) return __result;
                
                foreach (var target in GenUI.TargetsAt(clickPos, ForChargers))
                {
                    if (ChargeSourceUtility.FindSources(target.Thing).Any(charger => charger.Available))
                    {
                        __result.Add(CreateMenuOption(target.Thing, pawn, target));
                    }
                }

                return __result;
            }
        }
    }
}