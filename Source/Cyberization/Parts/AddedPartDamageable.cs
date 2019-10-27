using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Harmony;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Cyberization.Parts
{
    public class AddedPartDamageableProperties : HediffCompProperties
    {
        public float healRate = 1f;
        
        public AddedPartDamageableProperties()
        {
            compClass = typeof(AddedPartDamageable);
        }
    }

    public class AddedPartDamageable : HediffComp
    {
        private AddedPartDamageableProperties Props => (AddedPartDamageableProperties) props;

        public bool CanHeal => Props.healRate > 0;

        public float HealRate => Props.healRate;

        public bool Damaged =>  PartUtility.GetHediffsForPart(parent)
            .OfType<Hediff_Injury>()
            .Any(hediff => hediff.Severity > 0);
    }

    static class DisableNaturalHealing
    {
        private static IEnumerable<Hediff> GetHediffsForPart(BodyPartRecord record, HediffSet set)
        {
            return set.hediffs.Where(hediff => hediff.Part == record);
        }

        private static AddedPartDamageable GetDamageablePart(BodyPartRecord partRecord, HediffSet hediffSet)
        {
            try
            {
                return GetHediffsForPart(partRecord, hediffSet)
                    .OfType<HediffWithComps>()
                    .SelectMany(hediff => hediff.comps)
                    .OfType<AddedPartDamageable>()
                    .First();
            }
            catch (Exception)
            {
                return partRecord.IsCorePart 
                    ? null 
                    : GetDamageablePart(partRecord.parent, hediffSet);
            }
        }

        private static bool IsOnDamageablePart(BodyPartRecord partRecord, HediffSet hediffSet)
        {
            // the engine sometimes passes these in
            if (partRecord == null || hediffSet == null) return false;
            return GetDamageablePart(partRecord, hediffSet) != null;
        }
        
        private static bool PartCanHeal(BodyPartRecord partRecord, HediffSet hediffSet)
        {
            // the engine sometimes passes these in
            if (partRecord == null || hediffSet == null) return false;
            return GetDamageablePart(partRecord, hediffSet)?.CanHeal ?? true;
        }
        
        private static bool AllInjuriesWontHeal(Pawn pawn)
        {
            return pawn.health.hediffSet.hediffs
                .OfType<Hediff_Injury>()
                .All(injury => !PartCanHeal(injury.Part, pawn.health.hediffSet));
        }

        private static bool AllInjuriesAreOnParts(Pawn pawn)
        {
            return pawn.health.hediffSet.hediffs
                .OfType<Hediff_Injury>()
                .All(injury => IsOnDamageablePart(injury.Part, pawn.health.hediffSet));
        }
        
        [HarmonyPatch(typeof(HediffUtility), nameof(HediffUtility.CanHealNaturally))]
        static class Harmony_HediffUtility_CanHealNaturally
        {
            [HarmonyPostfix]
            static bool DamageablePartsDontHeal(bool __result, Hediff_Injury hd)
            {
                return !PartCanHeal(hd.Part, hd.pawn.health.hediffSet) ? false : __result;
            }
        }

        [HarmonyPatch(typeof(HediffUtility), nameof(HediffUtility.CanHealFromTending))]
        static class Harmony_HediffUtility_CanHealFromTending
        {
            [HarmonyPostfix]
            static bool DamageablePartsDontNeedTending(bool __result, Hediff_Injury hd)
            {
                return IsOnDamageablePart(hd.Part, hd.pawn.health.hediffSet) ? false : __result;
            }
        }

        [HarmonyPatch(typeof(Hediff), nameof(Hediff.TendableNow))]
        static class Harmony_Hediff_TendableNow
        {
            [HarmonyPostfix]
            static bool DamageablePartsDontNeedTending(bool __result, Hediff __instance, bool ignoreTimer)
            {
                return IsOnDamageablePart(__instance.Part, __instance.pawn.health.hediffSet) ? false : __result;
            }
        }

        [HarmonyPatch(typeof(HealthAIUtility), nameof(HealthAIUtility.ShouldSeekMedicalRest))]
        static class Harmony_HealthAIUtility_ShouldSeekMedicalRest
        {
            [HarmonyPostfix]
            static bool DamageablePartsDontNeedRest(bool __result, Pawn pawn)
            {
                // ensures that colonists incapable of walking are rescued (happens from downed in part only leg injuries)
                if (pawn.health.Downed) return true;
                if (AllInjuriesWontHeal(pawn)) return false;
                return __result;
            }
        }

        [HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.HealthTick))]
        static class Harmony_Pawn_HealthTracker_HealthTick_LogSpam
        {
            static bool ShouldSendMessage(bool damagedHealed, Pawn pawn)
            {
                return AllInjuriesAreOnParts(pawn) ? false : damagedHealed;
            }

            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> DontSpawmFullyHeadledMessaged(IEnumerable<CodeInstruction> instructions)
            {
                const int localHealedBool = 7;
                var phase = 0;
                Label? skipMessage = null;

                foreach (var instruction in instructions)
                {
                    switch (phase)
                    {
                        case 0:
                            if (instruction.opcode == OpCodes.Call
                                && instruction.operand == AccessTools.Method(
                                    typeof(PawnUtility),
                                    nameof(PawnUtility.ShouldSendNotificationAbout)))
                            {
                                phase = 1;
                            }
                            break;
                        case 1:
                            if (instruction.opcode == OpCodes.Brfalse)
                            {
                                phase = 2;
                                skipMessage = instruction.operand as Label?;
                            }
                            break;
                        case 2:
                            phase = -1;

                            yield return new CodeInstruction(OpCodes.Ldloc, localHealedBool);
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldfld, 
                                AccessTools.Field(
                                    typeof(Pawn_HealthTracker), 
                                    "pawn"));
                            yield return new CodeInstruction(
                                OpCodes.Call, 
                                AccessTools.Method(
                                    typeof(Harmony_Pawn_HealthTracker_HealthTick_LogSpam), 
                                    nameof(ShouldSendMessage)));
                            
                            yield return new CodeInstruction(OpCodes.Brfalse, skipMessage.Value);
                            break;
                    }

                    yield return instruction;
                }
            }
        }

        [HarmonyPatch(typeof(Hediff_Injury), nameof(Hediff_Injury.Heal))]
        static class Harmony_Hediff_Injury_Heal
        {
            static float GetHealAmount(Hediff_Injury injury, Pawn pawn, float amount)
            {
                try
                {
                    return pawn.HealthScale * 0.01f * pawn.health.hediffSet.hediffs
                        .Where(hediff => hediff.Part == injury.Part)
                        .OfType<HediffWithComps>()
                        .SelectMany(hediff => hediff.comps)
                        .OfType<AddedPartDamageable>()
                        .Select(part => part.HealRate)
                        .First();
                }
                catch
                {
                    return amount;
                }
            }

            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> UseAddedPartHealRate(IEnumerable<CodeInstruction> instructions, ILGenerator il)
            {
                var phase = 0;
                var healAmountIndex = il.DeclareLocal(typeof(float));
                foreach (var instruction in instructions)
                {
                    var emit = true;
                    switch (phase)
                    {
                        case 0:
                            if (instruction.opcode == OpCodes.Ldarg_1)
                            {
                                phase = 1;
                                emit = false;
                                
                                yield return new CodeInstruction(OpCodes.Ldarg_0);
                                yield return new CodeInstruction(OpCodes.Dup);
                                yield return new CodeInstruction(
                                    OpCodes.Ldfld, 
                                    AccessTools.Field(
                                        typeof(Hediff), 
                                        nameof(Hediff.pawn)));
                                yield return instruction;
                                yield return new CodeInstruction(
                                    OpCodes.Call, 
                                    AccessTools.Method(
                                        typeof(Harmony_Hediff_Injury_Heal),
                                        nameof(GetHealAmount)));
                                yield return new CodeInstruction(OpCodes.Stloc, healAmountIndex.LocalIndex);
                                yield return new CodeInstruction(OpCodes.Ldloc, healAmountIndex.LocalIndex);
                            }
                            break;
                        case 1:
                            if (instruction.opcode == OpCodes.Ldarg_1)
                            {
                                instruction.opcode = OpCodes.Ldloc;
                                instruction.operand = healAmountIndex.LocalIndex;
                            }
                            break;
                    }

                    if(emit) 
                        yield return instruction;
                }
            }
        }
    }
}