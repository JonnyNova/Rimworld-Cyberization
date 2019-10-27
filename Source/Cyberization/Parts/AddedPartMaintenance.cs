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
        private bool _disabled = false;

        private AddedPartMaintenanceProperties Props => (AddedPartMaintenanceProperties) props;
        
        public override void CompPostMake()
        {
            _condition = Props.maxCondition;
        }

        private float Percent => _condition * 1.0f / Props.maxCondition;

        public float PartEffectiveness
        {
            get
            {
                if (_disabled) return 0f;

                switch(Condition)
                {
                    case AddedPartCondition.Good: return 1f;
                    case AddedPartCondition.Okay: return 0.66f;
                    case AddedPartCondition.Bad: return 0.33f;
                    case AddedPartCondition.InOperable: return 0f;
                    default: return 1f;
                }
            }
        }

        public AddedPartCondition Condition
        {
            get
            {
                var percent = Percent;
                if (percent >= ConditionGoodFloor) return AddedPartCondition.Good;
                if (percent >= ConditionBadCeiling) return AddedPartCondition.Okay;
                return _condition > 0 ? AddedPartCondition.Bad : AddedPartCondition.InOperable;
            }
        }

        public bool CanBeMaintained => _condition < Props.maxCondition;

        public bool NeedsMaintenance => Percent <= Settings.SeekMaintenancePercent;

        public void SetDisabled(bool disabled)
        {
            _disabled = disabled;
        }

        public void DoMaintenance(int amount)
        {
            var lastCondition = Condition;
            if (_condition + amount > Props.maxCondition) _condition = Props.maxCondition;
            else _condition += amount;
            CheckForCapacityChange(lastCondition);
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if(_condition <= 0) return;
            var lastCondition = Condition;
            _condition -= 1;
            CheckForCapacityChange(lastCondition);
        }

        private void CheckForCapacityChange(AddedPartCondition lastCondition)
        {
            if(lastCondition != Condition)
            {
                Pawn.health.Notify_HediffChanged(parent);
            }
        }

        public override string CompTipStringExtra => 
            "Condition: " + (int) (Percent * 100) + "%" + 
            (Condition != AddedPartCondition.Good 
                ? ", -" + 100 * (1 - PartEffectiveness) + "% " + "PartEfficiency".Translate() 
                : "");

        public override void CompExposeData()
        {
            Scribe_Values.Look(ref _condition, "condition");
        }
    }
}