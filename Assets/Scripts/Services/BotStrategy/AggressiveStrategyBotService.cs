using Assets.Scripts.Helpers;
using Assets.Scripts.Models.Actions;
using Assets.Scripts.Models.BotTurnSteps;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Services.BotStrategy
{
    public interface IAggressiveStrategyBotService : IStrtegyBotService { }

    public class AggressiveStrategyBotService : IAggressiveStrategyBotService
    {
        [Inject]
        private readonly IActionCostService _actionCostService;

        [Inject]
        private readonly IUnitManager _unitManager;

        [Inject]
        private readonly IGridService _gridService;

        [Inject]
        private readonly ISharedBotStrategyService _sharedBotStrategyService;

        public BaseBotCommand GetNextCommand(Unit unit)
        {
            return GetAttackCommand(unit)
                ?? GetMoveToEnemyCommand(unit)
                ?? BotStratagyHelper.GetSharedFinishStep();
        }

        private ExecuteActionBotCommand? GetAttackCommand(Unit unit)
        {
            var enemyPeackActions = unit
                .Actions
                .Where(x => x.Type == Db.Enums.ActionTargetType.OtherSideUnitPeacks)
                .Cast<EnemyUnitTargetAction>()
                .Where(x => _actionCostService.IsActionAvaliable(unit.ActualActionPoints, x.PointCost))
                .ToArray();

            if (enemyPeackActions.Length == 0)
            {
                return null;
            }

            var enemyUnitCordinates = _unitManager.Units
                .Where(x => x.Characteristic.Side != unit.Characteristic.Side)
                .Where(x => !x.IsDead)
                .Select(x => _gridService.ToGridCordinates(x));

            var adjacentedActions = new List<(EnemyUnitTargetAction Action, Vector3Int[] Cordinates)>();

            foreach (var action in enemyPeackActions)
            {
                var enemiesAdjacentVectors = enemyUnitCordinates
                    .Where(
                        x => AdjacentHelper.IsAdjacent(
                            _gridService.ToGridCordinates(unit),
                            x,
                            action.Range
                        )
                    );

                if (enemiesAdjacentVectors.Count() != 0)
                {
                    adjacentedActions.Add((action, enemiesAdjacentVectors.ToArray()));
                }
            }

            if(!adjacentedActions.Any())
            {
                return null;
            }

            var (choosenAction, enemyCordinates) = adjacentedActions.First();

            return new ExecuteActionBotCommand()
            {
                TargetCordinate = enemyCordinates.First(),
                Action = choosenAction
            };
        }

        private MoveBotCommand? GetMoveToEnemyCommand(Unit unit)
        {
            var enemyUnitCordinates = _unitManager.Units
                .Where(x => x.Characteristic.Side != unit.Characteristic.Side)
                .Where(x => !x.IsDead)
                .Select(x => _gridService.ToGridCordinates(x));

            var enemiesAdjacentVectors = enemyUnitCordinates
                .Where(
                    x => AdjacentHelper.IsAdjacent(
                        _gridService.ToGridCordinates(unit),
                        x,
                        AdjacentHelper.CloseActionRange
                    )
                );

            //Временное решение
            if (enemiesAdjacentVectors.Any())
            {
                return null;
            }

            var targetUnit = _sharedBotStrategyService.FindNearestEnemyOnGrid(unit);

            var targetPosition = _gridService.ToGridCordinates(targetUnit);
            var bestTile = _sharedBotStrategyService.FindBestTileNearTarget(
                targetPosition,
                _gridService.ToGridCordinates(unit),
                unit
            );
            var path = _sharedBotStrategyService.FindPath(_gridService.ToGridCordinates(unit), bestTile, unit);

            return new MoveBotCommand() { Path = path };
        }
    }
}
