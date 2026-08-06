using Assets.UnitsCharacteristics;
using Zenject;

namespace Assets.Scripts.Services
{
    public interface IDamageService
    {
        bool SetUnitDamage(Unit targetUnit, int damagePoints);
    }

    public class DamageService : IDamageService
    {
        [Inject]
        private readonly IUnitPanelBarService _healthBarService;

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

            // Update the health bar UI if this is the currently selected player-controlled unit
            if(targetUnit.IsSelected && targetUnit.Characteristic.Side == SideType.UserSide)
            {
                _healthBarService.SetUnitHealthPoints(targetUnit.ActualHealthPoints, targetUnit.Characteristic.HealthPoints);
            }

            return isKillDamage;
        }
    }
}
