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

public enum ConditionEffectStartType
{
    OnTurnStart = 0,
    OnTurnEnd = 1,
    OnEnemyDeath = 2,
    OnAlliedDeath = 3,
    OnSucsesHit = 4,
    OnEnemyKill = 5
}

public enum ConditionType
{
    Damage = 0,
    HealthPointsRestore = 1,
    MaxHealthPointsIncrement = 2,
    MaxHealthPointsReduce = 3,
    MaxActionPointsIncrement = 4,
    MaxActionPointsReduce = 5,
}
