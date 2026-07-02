using Assets.Scripts.Helpers;
using Assets.Scripts.Models.Actions;
using Assets.Scripts.Models.BotTurnSteps;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Services.BotStrategy
{
    public interface ISupportStrategyService : IStrtegyBotService { }

    public class SupportStrategyService : ISupportStrategyService
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
            return GoFromEnemyUnitCommand(unit)
                ?? GetBuffCommand(unit)
                ?? GoToAlianceCommand(unit)
                ?? AttackEnemyCommand(unit)
                ?? GoToEnemyCommand(unit)
                ?? BotStratagyHelper.GetSharedFinishStep();
        }

        private BaseBotCommand? GetBuffCommand(Unit unit)
        {
            var buffs = unit.Actions
               .Where(x => x.Type == Db.Enums.ActionTargetType.SideUnitPeack)
               .Cast<AlianceUnitTargetAction>();

            var conditions = buffs.SelectMany(
               action => action.Steps.Where(setp => setp.Type == Db.Enums.ActionStepType.EffectWithDuration)
               .Cast<EffectWithDurationStep>()
               .Select(x => new { Action = action, x.Condition })
            );

            if (!conditions.Any())
            {
                return null;
            }

            var condition = conditions.First();

            var alianceUnits = _unitManager.Units.Where(x => x.Characteristic.Side == unit.Characteristic.Side);

            //пока 1 бафф с 1 condition-ом поэтому пофиг            
            var alianceUniteWithoutBuff = alianceUnits.Where(unit =>
                unit.DuratationConditions.Any(x => x.Condition.Name != condition.Condition.Name)
                && unit.GlobalConditions.Any(x => x.Name != condition.Condition.Name)
            );

            var currentUnitGridCordinate = _gridService.ToGridCordinates(unit);

            var nearestAlianceCordinate = alianceUniteWithoutBuff
                .Select(x => (Unit: x, GridCordinate: _gridService.ToGridCordinates(x)))
                .OrderBy(x => AdjacentHelper.GetGridDistante(currentUnitGridCordinate, _gridService.ToGridCordinates(x.GridCordinate)))
                .First();

            return new ExecuteActionBotCommand()
            {
                Action = condition.Action,
                TargetCordinate = nearestAlianceCordinate.GridCordinate
            };
        }

        private BaseBotCommand GoFromEnemyUnitCommand(Unit unit)
        {
            var unitGridCordinate = _gridService.ToGridCordinates(unit);

            return null;
        }

        private BaseBotCommand? GoToAlianceCommand(Unit unit)
        {
            var buffs = unit.Actions
                .Where(x => x.Type == Db.Enums.ActionTargetType.SideUnitPeack)
                .Cast<AlianceUnitTargetAction>();

             var conditions = buffs.SelectMany(
                action => action.Steps.Where(setp => setp.Type == Db.Enums.ActionStepType.EffectWithDuration)
                .Cast<EffectWithDurationStep>()
                .Select(x => new { Action = action, x.Condition })
             );

            if (!conditions.Any())
            {
                return null;
            }

            var condition = conditions.First();

            var alianceUnits = _unitManager.Units.Where(x => x.Characteristic.Side == unit.Characteristic.Side);

            //пока 1 бафф с 1 condition-ом поэтому пофиг            
            var alianceUniteWithoutBuff = alianceUnits.Where(unit =>
                unit.DuratationConditions.Any(x => x.Condition.Name != condition.Condition.Name) 
                && unit.GlobalConditions.Any(x => x.Name != condition.Condition.Name)
            );

            var currentUnitGridCordinate = _gridService.ToGridCordinates(unit);

            var nearestAlianceCordinate = alianceUniteWithoutBuff
                .Select(x => (Unit: x, GridCordinate: _gridService.ToGridCordinates(x)))
                .OrderBy(x => AdjacentHelper.GetGridDistante(currentUnitGridCordinate, _gridService.ToGridCordinates(x.GridCordinate)))
                .First();

            var resultPostion = _sharedBotStrategyService.FindBestTileNearTarget(targetPos: nearestAlianceCordinate.GridCordinate, currentUnitGridCordinate, unit);

            var path = _sharedBotStrategyService.FindPath(
                start: currentUnitGridCordinate,
                end: resultPostion,
                currentUnit: unit
            );

            return new MoveBotCommand() 
            {
                Path = path,
            };
        }

        private BaseBotCommand? GoToEnemyCommand(Unit unit)
        {
            var unitGridCordinate = _gridService.ToGridCordinates(unit);

            var anyAlianceUnits = _unitManager.Units
                .Where(x => x.Characteristic.Side == unit.Characteristic.Side)
                .Where(x => !x.IsDead)
                .Where(x => _gridService.ToGridCordinates(x) != unitGridCordinate)
                .Any();

            if (!anyAlianceUnits)
            {
                return null;
            }

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

        private BaseBotCommand? AttackEnemyCommand(Unit unit)
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

            if (!adjacentedActions.Any())
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
    }
}