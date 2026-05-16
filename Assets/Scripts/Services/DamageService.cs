using Assets.Scripts;
using Assets.UnitsCharacteristics;
using Zenject;

public interface IDamageService
{
    bool SetUnitDamage(Unit targetUnit, int damagePoints);
}

public class DamageService : IDamageService
{
    [Inject]
    private readonly IHealthBarService _healthBarService;

    public bool SetUnitDamage(Unit targetUnit, int damagePoints)
    {
        var isKillDamage = damagePoints >= targetUnit.ActualHealthPoints;

        if (isKillDamage)
        {
            targetUnit.ActualHealthPoints = 0;
        }
        else
        {
            targetUnit.ActualHealthPoints -= damagePoints;
        }

        // Если целю урона является актуальный unit,
        // под упарвлением игрока меняем отображаем актуальное HP
        if(targetUnit.IsSelected && targetUnit.Characterictics.Side == SideType.UserSide)
        {
            _healthBarService.SetUnitHelthPoints(targetUnit.ActualHealthPoints, targetUnit.Characterictics.HealthPoints);
        }

        return isKillDamage;
    }
}
