using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Cyberization.Parts
{
    static class Harmony_AddedPart
    {
        private static bool HasPart(Pawn pawn, BodyPartRecord record)
        {
            // TODO is searching for the parent part needed?
            return pawn.health.hediffSet.hediffs
                .Where(hediff => hediff.Part == record)
                .OfType<Hediff_AddedPart>()
                .Any();
        }

        private static void DropBloodOnDamageApply(Pawn pawn, DamageInfo dinfo)
        {
            if (!HasPart(pawn, dinfo.HitPart))
            {
                pawn.health.DropBloodFilth();
            }
            else
            {
                FilthMaker.TryMakeFilth(
                    pawn.PositionHeld, 
                    pawn.MapHeld, 
                    ThingDefOf.Filth_Trash, 
                    pawn.LabelIndefinite(), 
                    1);
            }
        }

        [HarmonyPatch(typeof(Pawn), nameof(Pawn.PostApplyDamage))]
        static class Patch_Pawn_PostApplyDamage
        {
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> AddPartFilthCheck(IEnumerable<CodeInstruction> instructions)
            {
                var success = false;
                foreach (var instruction in instructions)
                {
                    if (instruction.opcode == OpCodes.Callvirt
                        && (MethodInfo)instruction.operand == AccessTools.Method(
                            typeof(Pawn_HealthTracker),
                            nameof(Pawn_HealthTracker.DropBloodFilth)))
                    {
                        yield return new CodeInstruction(OpCodes.Pop);
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldarg_1);
                        yield return new CodeInstruction(
                            OpCodes.Call, 
                            AccessTools.Method(
                                typeof(Harmony_AddedPart),
                                nameof(DropBloodOnDamageApply)));
                        success = true;
                    }
                    else
                    {
                        yield return instruction;
                    }
                }

                if (!success)
                {
                    Log.Error("Failed to patch Pawn.PostApplyDamage");
                }
            }
        }
    }
}