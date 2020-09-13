using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace FrontierDevelopments.Cyberization
{
    public interface ICombatListener
    {
        void Drafted(bool drafted);
        void InCombat(bool inCombat);
    }
    
    public static class PawnCombatHandler
    {
        private static Dictionary<Pawn, HashSet<ICombatListener>> listeners = new Dictionary<Pawn,HashSet<ICombatListener>>();

        public static void Add(Pawn pawn, ICombatListener listener)
        {
            if(!listeners.ContainsKey(pawn))
                listeners.Add(pawn, new HashSet<ICombatListener>());
            listeners[pawn].Add(listener);
        }

        public static void Remove(Pawn pawn, ICombatListener listener)
        {
            if(listeners.ContainsKey(pawn))
                listeners[pawn].Remove(listener);
        }

        private static void NotifyDrafted(Pawn pawn, bool drafted)
        {
            if(listeners.ContainsKey(pawn))
                listeners[pawn].Do(listener => listener.Drafted(drafted));
        }

        private static void NotifyInCombat(Pawn pawn, bool inCombat)
        {
            if(listeners.ContainsKey(pawn))
                listeners[pawn].Do(listener => listener.InCombat(inCombat));
        }
        
        public static bool IsInCombat(Pawn pawn)
        {
            return pawn.CurJob?.verbToUse != null || IsCombat(pawn.stances.curStance);
        }

        public static bool IsCombat(Stance stance)
        {
            switch (stance)
            {
                case Stance_Busy busy:
                    return busy.focusTarg.IsValid;
                default:
                    return false;
            }
        }

        [HarmonyPatch(typeof(Pawn_DraftController), nameof(Pawn_DraftController.Drafted), MethodType.Setter)]
        private static class CheckForCombatFromDraft
        {
            [HarmonyPostfix]
            private static void SetEnabledOnParts(bool value, Pawn ___pawn)
            {
                NotifyDrafted(___pawn, value);
            }
        }

        [HarmonyPatch(typeof(Pawn_StanceTracker), nameof(Pawn_StanceTracker.SetStance))]
        private static class CheckForCombatFromStance
        {
            [HarmonyPostfix]
            private static void SetCombatParts(Pawn ___pawn, Stance newStance)
            {
                NotifyInCombat(___pawn, IsCombat(newStance));
            }
        }

        [HarmonyPatch(typeof(Pawn), nameof(Pawn.TryStartAttack))]
        private static class CheckForCombatFromPawn
        {
            [HarmonyPostfix]
            private static void SetCombatParts(bool __result, Pawn __instance)
            {
                NotifyInCombat(__instance, __result || IsInCombat(__instance));
            }
        }

        [HarmonyPatch]
        private static class CheckForCombatFromToilsCombat
        {
            [HarmonyTargetMethod]
            private static MethodInfo FindAnonymousFunction(Harmony instance)
            {
                return typeof(Toils_Combat)
                    .GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance)
                    .SelectMany(type => type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
                    .Where(method => !method.IsConstructor)
                    .Where(method => method.ReturnType == typeof(void))
                    .First(method => method.Name.Contains("TrySetJobToUseAttackVerb"));
            }

            [HarmonyPostfix]
            private static void Postfix(Toil ___toil)
            {
                NotifyInCombat(___toil.actor, true);
            }
        }

        [HarmonyPatch(typeof(Pawn), nameof(Pawn.TryGetAttackVerb))]
        private static class CheckForCombatFromPawnVerb
        {
            [HarmonyPostfix]
            private static void SetCombatParts(Verb __result, Pawn __instance)
            {
                NotifyInCombat(__instance, __result != null || IsInCombat(__instance));
            }
        }

        [HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.EndCurrentJob))]
        private static class CheckForCombatFromJobTracker
        {
            [HarmonyPostfix]
            private static void SetCombatParts(Pawn ___pawn)
            {
                NotifyInCombat(___pawn, IsInCombat(___pawn));
            }
        }
    }
}