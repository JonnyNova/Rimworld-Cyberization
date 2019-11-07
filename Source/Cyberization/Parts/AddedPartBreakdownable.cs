using RimWorld;
using RimWorld.Planet;
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
        private bool _disabled;

        public bool IsBrokenDown => _brokenDown;
        public bool CanBreakdown => !IsBrokenDown && !parent.pawn.IsWorldPawn();

        public AddedPartBreakdownableProperties Props => (AddedPartBreakdownableProperties) props;

        public float PartEffectiveness =>
            IsBrokenDown || _disabled ? Props.partEffectiveness : 1f;

        public override string CompTipStringExtra => IsBrokenDown
            ? AddedPartEffectivenessModifierUtils.EffectivenessString("BrokenDown".Translate(), PartEffectiveness) 
            : null;

        public override void CompPostMake()
        {
            IncidentWorker_AddedPartBreakdown.Add(this);
        }

        public override void CompPostPostRemoved()
        {
            IncidentWorker_AddedPartBreakdown.Remove(this);
        }

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

        public void SetDisabled(bool disabled)
        {
            _disabled = disabled;
            Pawn.health.Notify_HediffChanged(parent);
        }

        public override string ToString()
        {
            return base.ToString() + " in " + parent;
        }

        public override void CompExposeData()
        {
            Scribe_Values.Look(ref _brokenDown, "brokenDown");

            if(Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
                IncidentWorker_AddedPartBreakdown.Add(this);
        }
    }
}