using RimWorld;
using Verse;

namespace FrontierDevelopments.Cyberization.Parts
{
    public class AddedPartBreakdownableProperties : HediffCompProperties
    {
        public float partEffectiveness = 1.0f;
        
        public AddedPartBreakdownableProperties()
        {
            compClass = typeof(AddedPartBreakdownable);
        }
    }

    public class AddedPartBreakdownable : HediffComp, AddedPartEffectivenessModifier
    {
        private bool _brokenDown;

        public bool IsBrokenDown => _brokenDown;
        public bool CanBreakdown => !IsBrokenDown;

        public AddedPartBreakdownableProperties Props => (AddedPartBreakdownableProperties) props;

        public float PartEffectiveness =>
            IsBrokenDown ? Props.partEffectiveness : parent.def.addedPartProps.partEfficiency;

        public override string CompTipStringExtra => IsBrokenDown
            ? AddedPartEffectivenessModifierUtils.EffectivenessString("BrokenDown".Translate(), PartEffectiveness) 
            : null;

        public bool BreakDown()
        {
            if (_brokenDown) return false;
            _brokenDown = true;
            Pawn.health.Notify_HediffChanged(parent);
            if (Pawn.Faction == Faction.OfPlayer) 
                Find.LetterStack.ReceiveLetter(
                    "Cyberization.AddPartBreakdown.Label".Translate(),
                    "Cyberization.AddPartBreakdown.Text".Translate()
                        .Replace("{0}", Pawn.Name.ToStringFull)
                        .Replace("{1}", parent.Part.Label),
                    LetterDefOf.NegativeEvent, 
                    new TargetInfo(Pawn));
            return true;
        }

        public bool Repair()
        {
            if (!_brokenDown) return false;
            _brokenDown = false;
            Pawn.health.Notify_HediffChanged(parent);
            return true;
        }

        public override void CompExposeData()
        {
            Scribe_Values.Look(ref _brokenDown, "brokenDown");
        }
    }
}