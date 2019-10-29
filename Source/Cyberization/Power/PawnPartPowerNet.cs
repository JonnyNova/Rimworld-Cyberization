using System.Collections.Generic;
using System.Linq;
using FrontierDevelopments.General;
using FrontierDevelopments.General.Energy;
using Harmony;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Cyberization.Power
{
    public class PawnPartPowerNetProperties : CompProperties
    {
        public PawnPartPowerNetProperties()
        {
            compClass = typeof(PawnPartPowerNet);
        }
    }

    public class PawnPartPowerNet : ThingComp, IEnergyNet
    {
        public static PawnPartPowerNet Get(Pawn pawn)
        {
            return pawn.TryGetComp<PawnPartPowerNet>();
        }
        
        private readonly EnergyNet _energyNet = new EnergyNet();

        public float AmountAvailable => _energyNet.AmountAvailable;
        public float TotalAvailable => _energyNet.TotalAvailable;
        public float RateAvailable => _energyNet.RateAvailable;
        public float MaxRate => _energyNet.MaxRate;

        public IEnumerable<IEnergyNode> Nodes => _energyNet.Nodes;
        
        public void Connect(IEnergyNode node)
        {
            _energyNet.Connect(node);
        }

        public void Disconnect(IEnergyNode node)
        {
            _energyNet.Disconnect(node);
        }

        public float Provide(float amount)
        {
            return _energyNet.Provide(amount);
        }

        public float Consume(float amount)
        {
            return _energyNet.Consume(amount);
        }
    }

    [HarmonyPatch(typeof(Pawn), nameof(Pawn.SpawnSetup))]
    static class ApparelAdded
    {
        [HarmonyPostfix]
        static void InitializePawnPartPowerNetwork(Pawn __instance)
        {
            var net = __instance.TryGetComp<PawnPartPowerNet>();
            
            // ThingComps
            __instance.AllComps
                .OfType<IEnergyNode>()
                .Where(node => net != node)
                .Do(node => net.Connect(node));
            
            // HediffComps
            __instance.health?.hediffSet.hediffs
                .OfType<HediffWithComps>()
                .SelectMany(hediff => hediff.comps)
                .OfType<IEnergyNode>()
                .Do(node => net.Connect(node));
            
            // Apparel ThingComps
            __instance.apparel?.WornApparel
                .SelectMany(apparel => apparel.AllComps)
                .OfType<IEnergyNode>()
                .Do(node => net.Connect(node));
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
                .Do(node => net.Connect(node));
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
                .Do(node => net.Disconnect(node));
        }
    }

}