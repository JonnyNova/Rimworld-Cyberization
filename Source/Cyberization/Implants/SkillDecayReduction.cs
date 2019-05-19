using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using FrontierDevelopments.Cyberization.Power;
using Harmony;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Cyberization.Implants
{
    public class SkillDecayReductionProperties : HediffCompProperties
    {
        public float skillDecayFactor;
        public List<SkillDef> skillDecayReduced = new List<SkillDef>();
        
        public SkillDecayReductionProperties()
        {
            compClass = typeof(SkillDecayReduction);
        }
    }
    
    public class SkillDecayReduction : HediffComp
    {
        private SkillDecayReductionProperties Props => (SkillDecayReductionProperties) props;

        public float ShouldReduceDecay(SkillDef skill)
        {
            return (parent.TryGetComp<PartPowerConsumer>()?.Powered ?? false) 
                   && Props.skillDecayReduced.Contains(skill) 
                ? Props.skillDecayFactor 
                : 1f;
        }
    }

    public class Harmony_SkillDecayReduction
    {
        [HarmonyPatch(typeof(SkillRecord), nameof(SkillRecord.Interval))]
        static class Patch_SkillRecord
        {
            private static float ShouldReduceDecay(Pawn pawn, SkillRecord skill)
            {
                if (pawn.story.traits.HasTrait(TraitDefOf.GreatMemory)) return 0.5f;

                try
                {
                    return pawn.health.hediffSet.hediffs
                        .OfType<HediffWithComps>()
                        .SelectMany(hediff => hediff.comps)
                        .OfType<SkillDecayReduction>()
                        .Select(comp => comp.ShouldReduceDecay(skill.def))
                        .First();
                }
                catch (Exception)
                {
                    return 1f;
                }
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
    }
}