using Assets.Scripts.Models.Animations;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Services
{
    public interface IMoveService
    {
        UniTask MovePathAsync(Unit unit, Vector3Int[] path);
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

        public async UniTask MovePathAsync(Unit unit, Vector3Int[] path)
        {
            if (path.Length <= 1)
            {
                return;
            }
            
            var direction = path[1] - path[0];

            // Включаем анимацию движения
            _animationService.SwitchUnitAnimation(
                unit,
                new MoveAnimation()
                {
                    IsActive = true,
                    Direction = direction
                }
            );

            for (int i = 1; i < path.Length; i++)
            {
                var newDirection = path[i] - path[i - 1];

                if (newDirection.x != direction.x)
                {
                    direction = newDirection;
                    _animationService.SwitchUnitAnimation(
                        unit,
                        new MoveAnimation
                        {
                            IsActive = true,
                            Direction = direction
                        }
                    );
                }

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
                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;
                    unit.transform.position = Vector3.Lerp(startPos, worldTarget, t);

                    await UniTask.Yield(PlayerLoopTiming.Update);
                }

                unit.transform.position = worldTarget;
            }

            // Выключаем анимацию движения
            _animationService.SwitchUnitAnimation(
                unit,
                new MoveAnimation() 
                {
                    IsActive = false,
                    Direction = null
                }
            );
        }
    }
}
