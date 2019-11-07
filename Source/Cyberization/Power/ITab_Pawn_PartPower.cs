using System.Linq;
using FrontierDevelopments.General;
using FrontierDevelopments.General.Energy;
using FrontierDevelopments.General.UI;
using RimWorld;
using UnityEngine;
using Verse;

namespace FrontierDevelopments.Cyberization.Power
{
    public class ITab_Pawn_PartPower : ITab
    {
        private const float ViewMargin = 20f;
        private const float ProviderHeight = 72f;
        private const float ConsumerHeight = 144f;

        private Vector2 scrollPosition = Vector2.zero;
        
        
        public ITab_Pawn_PartPower()
        {
            size = new Vector2(630f, 430f);
            labelKey = "Cyberization.ITab.Power.Name";
        }

        public override bool IsVisible => SelPawn?.needs.TryGetNeed<PartEnergyNeed>() != null;

        private static string FormatDischarge(IEnergyProvider node)
        {
            return node.MaxRate - node.RateAvailable + "/" + node.MaxRate;
        }

        private static string FormatCapacity(IEnergyProvider node)
        {
            return node.AmountAvailable + "/" + node.TotalAvailable;
        }

        private static void DrawIcon(Listing_Standard section, int size)
        {
            
        }

        private static void DrawLabel(Listing_Standard section, ILabeled labeled)
        {
            section.Label(labeled.Label);
        }

        private static void DrawPowerSlider(Listing_Standard section, IPowerConsumer consumer)
        {
            consumer.RateWanted = Mathf.RoundToInt(section.Slider(Mathf.RoundToInt(consumer.RateWanted), 0f, consumer.MatRate));
            section.Gap();
        }

        private void RenderProvider(Listing_Standard list, IPowerProvider provider)
        {
            var section = list.BeginSection(ProviderHeight);
            DrawIcon(section, 32);
            DrawLabel(section, provider);

            section.Label(FormatDischarge(provider));
            section.Label(FormatCapacity(provider));

            list.EndSection(section);
            list.Gap();
        }

        private void RenderConsumer(Listing_Standard list, IPowerConsumer consumer)
        {
            var section = list.BeginSection(ConsumerHeight);
            DrawIcon(section, 32);
            DrawLabel(section, consumer);
            
            if (!consumer.Essential && SelPawn.Faction == Faction.OfPlayer)
            {
                var enableWhileDrafted = consumer.EnableWhileDrafted;
                var enableWhileNotDrafted = consumer.EnabledWhileNotDrafted;
                var enableInCombat = consumer.EnabledInCombat;
                var enableOutOfCombat = consumer.EnabledOutOfCombat;
                section.CheckboxLabeled("Cyberization.ITab.Power.WhileDrafted".Translate(), ref enableWhileDrafted);
                section.CheckboxLabeled("Cyberization.ITab.Power.WhileNotDrafted".Translate(), ref enableWhileNotDrafted);
                section.CheckboxLabeled("Cyberization.ITab.Power.InCombat".Translate(), ref enableInCombat);
                section.CheckboxLabeled("Cyberization.ITab.Power.OutOfCombat".Translate(), ref enableOutOfCombat);
                consumer.EnableWhileDrafted = enableWhileDrafted;
                consumer.EnabledWhileNotDrafted = enableWhileNotDrafted;
                consumer.EnabledInCombat = enableInCombat;
                consumer.EnabledOutOfCombat = enableOutOfCombat;
            }
            else
            {
                section.Label("cant be disabled");
            }
            DrawPowerSlider(section, consumer);
            
            list.EndSection(section);
            list.Gap();
        }
        
        protected override void FillTab()
        {
            var outRect = new Rect(0, 0, size.x, size.y);
            var inRect = new Rect(ViewMargin, ViewMargin, size.x - ViewMargin, 1000);
            
            Widgets.BeginScrollView(outRect, ref scrollPosition, inRect, true);

            var list = new Listing_Standard();
            list.Begin(inRect);

            var net = PawnPartPowerNet.Get(SelPawn);
            var providers = net.Nodes.OfType<IPowerProvider>().ToList();
            var consumers = net.Nodes.OfType<IPowerConsumer>().ToList();

            list.Gap();
            
            Text.Font = GameFont.Medium;
            list.Label("Total energy: " + FormatDischarge(net));
            Text.Font = GameFont.Small;

            Elements.Heading(list, "Providers");
            providers.ForEach(provider => RenderProvider(list, provider));

            Elements.Heading(list, "Consumers");
            consumers.ForEach(consumer => RenderConsumer(list, consumer));

            list.End();

            Widgets.EndScrollView();
        }
    }
}