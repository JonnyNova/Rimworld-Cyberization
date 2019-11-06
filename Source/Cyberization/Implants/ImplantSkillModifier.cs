using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using FrontierDevelopments.Cyberization.Power;
using Harmony;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Cyberization.Implants
{
    public class ImplantSkillModifierProperties : HediffCompProperties
    {
        public float skillLearnIncrease;
        public float skillDecayFactor;
        public List<SkillDef> skillsAffected = new List<SkillDef>();
        
        public ImplantSkillModifierProperties()
        {
            compClass = typeof(ImplantSkillModifier);
        }
    }
    
    public class ImplantSkillModifier : HediffComp
    {
        private ImplantSkillModifierProperties Props => (ImplantSkillModifierProperties) props;

        public float ShouldReduceDecay(SkillDef skill)
        {
            return (parent.TryGetComp<PartPowerConsumer>()?.Powered ?? false) 
                   && Props.skillsAffected.Contains(skill) 
                ? Props.skillDecayFactor 
                : 1f;
        }

        public float SkillLearnIncrease(SkillDef skill)
        {
            return (parent.TryGetComp<PartPowerConsumer>()?.Powered ?? false) 
                   && Props.skillsAffected.Contains(skill) 
                ? Props.skillLearnIncrease 
                : 0f;
        }
    }

    public class Harmony_SkillDecayReduction
    {
        static IEnumerable<ImplantSkillModifier> Implants(Pawn pawn)
        {
            return pawn.health.hediffSet.hediffs
                .OfType<ImplantPowered>()
                .Where(implant => implant.Powered)
                .SelectMany(hediff => hediff.comps)
                .OfType<ImplantSkillModifier>();
        }

        [HarmonyPatch(typeof(SkillRecord), nameof(SkillRecord.Interval))]
        static class Patch_SkillRecord
        {
            private static float ShouldReduceDecay(Pawn pawn, SkillRecord skill)
            {
                if (pawn.story.traits.HasTrait(TraitDefOf.GreatMemory)) return 0.5f;
                return Implants(pawn).Aggregate(1f, (factor, implant) => factor * implant.ShouldReduceDecay(skill.def));
            }

            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> ReduceSkillDecay(IEnumerable<CodeInstruction> instructions)
            {
                var emitExisting = true;
                var phase = 0;
                foreach (var instruction in instructions)
                {
                    switch (phase)
                    {
                        case 0:
                            if (instruction.opcode == OpCodes.Ldfld
                                && instruction.operand == AccessTools.Field(typeof(SkillRecord), "pawn"))
                            {
                                emitExisting = false;
                                phase = 1;

                                yield return instruction;
                                yield return new CodeInstruction(OpCodes.Ldarg_0);
                                yield return new CodeInstruction(OpCodes.Call, 
                                    AccessTools.Method(typeof(Patch_SkillRecord), 
                                        nameof(ShouldReduceDecay)));
                            }
                            break;
                        case 1:
                            if (instruction.opcode == OpCodes.Stloc_0)
                            {
                                emitExisting = true;
                                phase = -1;
                            }
                            break;
                    }

                    if (emitExisting)
                        yield return instruction;
                }
            }
        }

        [HarmonyPatch(typeof(SkillRecord), nameof(SkillRecord.LearnRateFactor))]
        static class Patch_LearnRateFactor
        {
            private static float AddedLearnRate(Pawn pawn, SkillRecord skill)
            {
                return Implants(pawn).Aggregate(0f, (sum, implant) => sum + implant.SkillLearnIncrease(skill.def));
            }

            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> AdjustWithImplant(IEnumerable<CodeInstruction> instructions, ILGenerator il)
            {
                var label = il.DefineLabel();
                
                foreach (var instruction in instructions)
                {
                    if (instruction.opcode == OpCodes.Br)
                    {
                        instruction.operand = label;
                    }
                    else if (instruction.opcode == OpCodes.Ldarg_1)
                    {
                        yield return new CodeInstruction(OpCodes.Ldarg_0)
                        {
                            labels = { label }
                        };
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(SkillRecord), "pawn"));
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_LearnRateFactor), nameof(AddedLearnRate)));
                        yield return new CodeInstruction(OpCodes.Ldloc_0);
                        yield return new CodeInstruction(OpCodes.Add);
                        yield return new CodeInstruction(OpCodes.Stloc_0);
                    }

                    yield return instruction;
                }
            }
        }

        [HarmonyPatch(typeof(SkillUI), "GetSkillDescription")]
        static class Patch_SkillUI
        {
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> UseActualValueInDescription(IEnumerable<CodeInstruction> instructions)
            {
                foreach (var instruction in instructions)
                {
                    if (instruction.opcode == OpCodes.Ldc_R4)
                    {
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SkillRecord), nameof(SkillRecord.LearnRateFactor)));
                    }
                    else
                    {
                        yield return instruction;
                    }
                }
            }
        }
    }
}