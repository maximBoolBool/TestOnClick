using Assets.Scripts.Models.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;

namespace Assets.Scripts.Services
{
    public interface IUnitConditionService
{
    void ExecuteConditionEffect(Unit unit, ConditionEffectStartType type);

    void ActualizeUnitConditions(Unit unit);
}

public class UnitConditionService : IUnitConditionService
{
    [Inject]
    private readonly IDamageService _damageService;

    public void ExecuteConditionEffect(Unit unit, ConditionEffectStartType type)
    {
        var conditions = unit.DuratationConditions
            .Where(x => x.Condition.StartType == type)
            .ToArray();

        unit.DuratationConditions
            .Except(conditions);

        if (conditions.Length == 0)
        {
            return;
        }

        var updatedConditions = new List<(BaseCondition Condition, int DisappearancesTurn)>();
        foreach(var condition in conditions)
        {
            switch (condition.Condition.Type)
            {
                case ConditionType.Damage:
                    var damageCondition = condition.Condition as DamageCondition;
                    _damageService.SetUnitDamage(
                        targetUnit: unit,
                        damagePoints: damageCondition.Damage
                    );
                    break;
                case ConditionType.HealthPointsRestore:
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public void ActualizeUnitConditions(Unit unit)
    {
        if(unit.DuratationConditions.Count() == 0)
        {
            return;
        }

        unit.DuratationConditions = unit.DuratationConditions
            .Select(x => (x.Condition, DisappearancesTurn : x.DisappearancesTurn - 1))
            .Where(x => x.DisappearancesTurn > 0)
            .ToList();
    }
    }
}
