using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;

namespace Assets.Scripts.Services
{
    public interface IMoveService
    {
        IEnumerator MovePath(Unit unit, Vector3Int[] path);
    }

    public class MoveService : IMoveService
    {
        public float moveSpeed = 3f;

        [Inject(Id = Constants.Grid)]
        private readonly Grid _grid;

        [Inject]
        private readonly IAnimationService _animationService;

        [Inject]
        private readonly IMovementCostService _movementCostService;

        public IEnumerator MovePath(Unit unit, Vector3Int[] path)
        {
            if (path.Length <= 1)
            {
                yield break;
            }

            // Включаем анимацию движения
            _animationService.SwitchUnitAnimation(unit, UnitAnimationType.Move, true);

            for (int i = 1; i < path.Length; i++)
            {
                Vector3Int step = path[i];
                Vector3Int prevStep = path[i - 1];
                Vector3Int dir = step - prevStep;
                int stepCost = _movementCostService.GetMovementCost(step, dir);
                unit.ActualActionPoints -= stepCost;

                var worldTarget = _grid.GetCellCenterWorld(step);
                float distance = Vector3.Distance(unit.transform.position, worldTarget);
                float duration = distance / moveSpeed;
                float elapsed = 0;
                Vector3 startPos = unit.transform.position;

                while (elapsed < duration)
                {
                    unit.transform.position = Vector3.Lerp(startPos, worldTarget, elapsed / duration);
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                unit.transform.position = worldTarget;
            }

            // Выключаем анимацию движения
            _animationService.SwitchUnitAnimation(unit, UnitAnimationType.Move, false);
        }
    }
}
