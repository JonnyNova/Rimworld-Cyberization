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
        private const float RowHeight = 60f;

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

        private static void DrawIcon(Listing_Standard section, int size)
        {
            
        }

        private static void DrawLabel(Listing_Standard section, ILabeled labeled)
        {
            section.Label(labeled.Label);
        }

        private static void DrawPowerSlider(Listing_Standard section, IPowerConsumer consumer)
        {
            
        }

        private void RenderProvider(Listing_Standard list, IPowerProvider provider)
        {
            var section = list.BeginSection(RowHeight);
            DrawIcon(section, 32);
            DrawLabel(section, provider);
            
            section.Label(FormatDischarge(provider));
            
            list.EndSection(section);
            list.Gap();
        }

        private void RenderConsumer(Listing_Standard list, IPowerConsumer consumer)
        {
            var section = list.BeginSection(RowHeight);
            DrawIcon(section, 32);
            DrawLabel(section, consumer);
            DrawPowerSlider(section, consumer);
            
            if (!consumer.Essential && SelPawn.Faction == Faction.OfPlayer)
            {
                var enabled = consumer.Enabled;
                section.CheckboxLabeled("enabled", ref enabled);
                consumer.Enabled = enabled;
            }
            else
            {
                section.Label("cant be disabled");
            }
            
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