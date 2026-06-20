using Assets.Scripts.Enums;
using Assets.Scripts.Managers;
using Assets.Scripts.Models.BotTurnSteps;
using Assets.Scripts.Models.Conditions;
using Assets.Scripts.Services.BotStrategy;
using System.Collections;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Services
{
    public interface IBotExecutionTurnService
    {
        public void ExecuteBotTurn(Unit unit);
    }

    public class BotExecutionTurnService : IBotExecutionTurnService
    {
        [Inject]
        private readonly IAggressiveStrategyBotService _aggressiveStrategyBotService;

        [Inject]
        private readonly IDefensiveStrategyBotService _defensiveStrategyBotService;

        [Inject]
        private readonly ISupportStrategyService _supportStrategyService;

        [Inject]
        private readonly ITurnManager _turnManager;

        [Inject]
        private readonly IConditionService _conditionService;

        [Inject]
        private readonly IMoveService _moveService;

        [Inject]
        private readonly IActionExecutionService _executeActionService;

        public void ExecuteBotTurn(Unit unit)
        {
            unit.StartCoroutine(ExecuteBotTurnCoroutine(unit));
        }

        private IEnumerator ExecuteBotTurnCoroutine(Unit unit)
        {
            var strategyService = GetStrategyService(GetBotStrategyType(unit));

            if (unit.ActualActionPoints <= 0 || unit.IsDead)
            {
                DeselectUnit(unit);
                yield break;
            }

            while (true)
            {
                var step = strategyService.GetNextCommand(unit);

                switch (step)
                {
                    case MoveBotCommand moveBotCommand:
                        yield return _moveService.MovePath(unit, moveBotCommand.Path);
                        break;
                    case ExecuteActionBotCommand executeActionBotCommand:
                        // Переработать
                        _executeActionService.TryExecuteAction(
                            executor: unit,
                            executeActionBotCommand.Action,
                            executeActionBotCommand.TargetCordinate
                        );
                        yield return new WaitForSeconds(1f);
                        break;
                    case SkipBotCommand _:
                        DeselectUnit(unit);
                        yield break;
                }
            }

            yield return null;
        }

        private BotStrategyType GetBotStrategyType(Unit unit)
        {
            if(unit.Name == "Warrior")
            {
                return BotStrategyType.Aggressive;
            }

            if(unit.Name == "Monk")
            {
                return BotStrategyType.Support;
            }

            return BotStrategyType.Defensive;
        }

        private IStrtegyBotService GetStrategyService(BotStrategyType strategyType)
        {
            return strategyType switch
            {
                BotStrategyType.Aggressive => _aggressiveStrategyBotService,
                BotStrategyType.Defensive => _defensiveStrategyBotService,
                BotStrategyType.Support => _supportStrategyService,
                _ => throw new System.ArgumentException($"Invalid strategy type: {strategyType}"),
            };
        }

        private void DeselectUnit(Unit unit)
        {
            _conditionService.ExecuteConditionEffect(unit, ConditionEffectStartType.OnTurnEnd);
            _conditionService.ActualizeUnitConditions(unit);
            _turnManager.EndTurn();
        }
    }
}
