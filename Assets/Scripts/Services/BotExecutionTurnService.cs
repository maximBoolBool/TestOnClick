using Assets.Scripts.Enums;
using Assets.Scripts.Models.BotTurnSteps;
using Assets.Scripts.Models.Conditions;
using Assets.Scripts.Services.BotStrategy;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Services
{
    public interface IBotExecutionTurnService
    {
        UniTask ExecuteBotTurnAsync(Unit unit);
    }

    public class BotExecutionTurnService : IBotExecutionTurnService
    {
        [Inject] private readonly IAggressiveStrategyBotService _aggressiveStrategyBotService;
        [Inject] private readonly IDefensiveStrategyBotService _defensiveStrategyBotService;
        [Inject] private readonly ISupportStrategyService _supportStrategyService;
        [Inject] private readonly IUnitConditionService _conditionService;
        [Inject] private readonly IMoveService _moveService;
        [Inject] private readonly IActionExecutionService _executeActionService;

        public async UniTask ExecuteBotTurnAsync(Unit unit)
        {
            var strategyService = GetStrategyService(GetBotStrategyType(unit));

            if (unit.ActualActionPoints <= 0 || unit.IsDead)
            {
                FinishBotTurn(unit);
                return;
            }

            while (true)
            {
                var step = strategyService.GetNextCommand(unit);

                switch (step)
                {
                    case MoveBotCommand moveBotCommand:
                        await _moveService.MovePathAsync(unit, moveBotCommand.Path);
                        break;

                    case ExecuteActionBotCommand executeActionBotCommand:
                        _executeActionService.TryExecuteAction(
                            executor: unit,
                            executeActionBotCommand.Action,
                            executeActionBotCommand.TargetCordinate
                        );
                        await UniTask.Delay(TimeSpan.FromSeconds(1f));
                        break;

                    case SkipBotCommand _:
                        FinishBotTurn(unit);
                        return;
                }
            }
        }

        private static BotStrategyType GetBotStrategyType(Unit unit)
        {
            if (unit.Name == "Warrior") return BotStrategyType.Aggressive;
            if (unit.Name == "Monk") return BotStrategyType.Support;
            return BotStrategyType.Defensive;
        }

        private IStrtegyBotService GetStrategyService(BotStrategyType strategyType)
        {
            return strategyType switch
            {
                BotStrategyType.Aggressive => _aggressiveStrategyBotService,
                BotStrategyType.Defensive => _defensiveStrategyBotService,
                BotStrategyType.Support => _supportStrategyService,
                _ => throw new ArgumentException($"Invalid strategy type: {strategyType}"),
            };
        }

        private void FinishBotTurn(Unit unit)
        {
            _conditionService.ExecuteConditionEffect(unit, ConditionEffectStartType.OnTurnEnd);
            _conditionService.ActualizeUnitConditions(unit);
        }
    }
}