using System.Collections.Generic;

namespace Assets.Scripts.Models.Conditions
{
    public abstract class BaseCondition
    {
        public string Name { get; set; }
        public virtual ConditionType Type { get;} 
        public ConditionEffectStartType StartType { get; set; }
        public bool IsFolding { get; set; }
    }

    public class DamageCondition : BaseCondition
    {
        public override ConditionType Type => ConditionType.Damage;
        public int Damage { get; set; }
    }

    public class HealthPointsRestore : BaseCondition
    {
        public override ConditionType Type => ConditionType.HealthPointsRestore;

        public int HealthPoints;
    }

    public class CharacteristicModifier : BaseCondition
    {
        public override ConditionType Type => ConditionType.CharecteristicsModifier;

        public Dictionary<CharecteristicsModifier,(double Value, CharecteristicsModifierOperation Operation)> Modifications { get; set; }
    }

    public enum ConditionEffectStartType
    {
        OnTurnStart = 0,
        OnTurnEnd = 1,
        OnEnemyDeath = 2,
        OnAlliedDeath = 3,
        OnSucsesHit = 4,
        OnEnemyKill = 5,
        OnConditionEffect = 6
    }

    public enum ConditionType
    {
        Damage = 0,
        HealthPointsRestore = 1,
        CharecteristicsModifier = 2,
    }

    public enum CharecteristicsModifier
    {
        HeakthPointMaxValue,
        ActionPointMaxValue,
        AttackSkill,
        DeafendSkill
    }

    public enum CharecteristicsModifierOperation
    {
        ValueAdd,
        ValueSubtract,
        ValueMultiply,
        ValueDivide,
    }
}