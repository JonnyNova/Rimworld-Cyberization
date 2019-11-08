using Verse;

namespace FrontierDevelopments.Cyberization.Parts
{
    public class AddedPartMaintenanceProperties : HediffCompProperties
    {
        public long maxCondition;
        
        public AddedPartMaintenanceProperties()
        {
            compClass = typeof(AddedPartMaintenance);
        }
    }

    public enum AddedPartCondition
    {
        Good,
        Okay,
        Bad,
        InOperable
    }
    
    public class AddedPartMaintenance : HediffComp, AddedPartEffectivenessModifier
    {
        private const float ConditionGoodFloor = 0.66f;
        private const float ConditionBadCeiling = 0.33f;
        
        private long _condition;
        private bool _maintainedLastTick;
        private bool? _overrideState;

        private AddedPartMaintenanceProperties Props => (AddedPartMaintenanceProperties) props;
        
        public override void CompPostMake()
        {
            _condition = Props.maxCondition;
        }

        private long Condition => Mod.Settings.UsePartMaintenance ? _condition : Props.maxCondition;

        public float Percent => Condition * 1.0f / Props.maxCondition;

        public float PartEffectiveness
        {
            get
            {
                if (_overrideState != null) return _overrideState.Value ? 1f : 0f;

                switch(Status)
                {
                    case AddedPartCondition.Good: return 1f;
                    case AddedPartCondition.Okay: return 0.66f;
                    case AddedPartCondition.Bad: return 0.33f;
                    case AddedPartCondition.InOperable: return 0f;
                    default: return 1f;
                }
            }
        }

        public AddedPartCondition Status
        {
            get
            {
                var percent = Percent;
                if (percent >= ConditionGoodFloor) return AddedPartCondition.Good;
                if (percent >= ConditionBadCeiling) return AddedPartCondition.Okay;
                return _condition > 0 ? AddedPartCondition.Bad : AddedPartCondition.InOperable;
            }
        }

        public bool CanBeMaintained => Condition < Props.maxCondition - 1;

        // check if this is being repaired
        // used to keep the patient in the bed until finished
        public bool NeedsMaintenance => Percent <= (_maintainedLastTick
                                            ? Mod.Settings.SatisfiedMaintenancePercent
                                            : Mod.Settings.SeekMaintenancePercent);

        public void OverrideEffectivenessState(bool? state)
        {
            _overrideState = state;
        }

        public void DoMaintenance(int amount)
        {
            var lastCondition = Status;
            if (_condition + amount > Props.maxCondition) _condition = Props.maxCondition;
            else _condition += amount;
            CheckForCapacityChange(lastCondition);
            _maintainedLastTick = true;
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if(!Mod.Settings.UsePartMaintenance || _condition <= 0) return;
            var lastPartCondition = Status;
            _condition -= 1;
            CheckForCapacityChange(lastPartCondition);
            _maintainedLastTick = false;
        }

        private void CheckForCapacityChange(AddedPartCondition lastCondition)
        {
            if(lastCondition != Status)
            {
                Pawn.health.Notify_HediffChanged(parent);
            }
        }

        public override string CompTipStringExtra => 
            "Condition: " + (int) (Percent * 100) + "%" + 
            (Status != AddedPartCondition.Good 
                ? ", -" + 100 * (1 - PartEffectiveness) + "% " + "PartEfficiency".Translate().ToString()
                : "");

        public override void CompExposeData()
        {
            Scribe_Values.Look(ref _condition, "condition");
            Scribe_Values.Look(ref _maintainedLastTick, "maintainedLastTick");
        }
    }
}
