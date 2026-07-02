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
        HeakthPointMaxValue = 0,
        ActionPointMaxValue = 1,
        AttackSkill = 2,
        DeafendSkill = 3
    }

    public enum CharecteristicsModifierOperation
    {
        ValueAdd = 0,
        ValueSubtract = 1,
        ValueMultiply = 2,
        ValueDivide = 3,
    }
}