using Assets.Scripts.Managers.UnitManager;
using Assets.Scripts.Models.Conditions;
using Assets.Scripts.Services;
using Assets.UnitsCharacteristics;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Assets.Scripts.Managers
{
    public interface ITurnManager
    {
        UniTask SceneStart();
        UniTask SkipTurnAsync();
        void EndCurrentTurn();
    }

    public class TurnManager : ITurnManager
    {
        private int currentUnitIndex = 0;
        private int turnCount = 0;
        private List<Unit> units;

        private UniTaskCompletionSource _turnCompletionSource;
        private bool _isGameEnded = false;

        [Inject(Id = Constants.TurnCountText)]
        private readonly TextMeshProUGUI _moveCounterText;

        [Inject] 
        private readonly IUnitManager _unitManager;
        [Inject]
        private readonly IUnitConditionService _conditionService;
        [Inject]
        private readonly IBotExecutionTurnService _botExecutionTurnService;
        [Inject]
        private readonly IUnitPanelBarService _unitPanelBarService;
        [Inject]
        private readonly IActionUIService _actionUiService;
        [Inject]
        private readonly IGameGlobalStateManager _gameGlobalStateManager;
        [Inject]
        private readonly IRoomService _roomService;
        [Inject]
        private readonly ICameraService _cameraService;
        [Inject]
        private readonly IUIAnimationService _uiAnimationService;
        [Inject]
        private readonly IActionClickHandler _actionClickHandler;
        [Inject]
        private readonly IUnitQueueUiService _unitQueueUiService;

        public async UniTask SceneStart()
        {
            await _roomService.TrySwitchNextRoom(false);

            await _unitManager.GenerateUnits();
            await _unitQueueUiService.SetUniticonsAsync();
            units = _unitManager.Units;
            _unitManager.RefreshUnitsActionPoints();
            _unitManager.SetActualHealthPoins();

            _uiAnimationService.ShakeCamera();
            _uiAnimationService.MoveVeils();

            if (units.Count > 0)
            {
                turnCount = 1;
                UpdateMoveCounterDisplay();
                StartTurnLoopAsync().Forget();
            }
        }

        /// <summary>
        /// Главный линейный цикл смены ходов (избавляет от рекурсии)
        /// </summary>
        private async UniTask StartTurnLoopAsync()
        {
            while (!_isGameEnded)
            {
                if (units == null || units.Count == 0) return;

                var currentUnit = units[currentUnitIndex];

                _conditionService.ExecuteConditionEffect(currentUnit, ConditionEffectStartType.OnTurnStart);

                if (currentUnit.IsDead)
                {
                    AdvanceToNextUnit();
                    continue;
                }

                if (await CheckForGameOverAsync())
                {
                    _isGameEnded = true;
                    break;
                }

                _turnCompletionSource = new UniTaskCompletionSource();

                await ProcessUnitTurnAsync(currentUnit);

                await _turnCompletionSource.Task;

                DeactivateUnitInternal(currentUnit);

                AdvanceToNextUnit();
            }
        }

        private async UniTask ProcessUnitTurnAsync(Unit unit)
        {
            _cameraService.MoveCamera(unit.transform.position);

            switch (unit.Characteristic.Side)
            {
                case SideType.UserSide:
                    ActivateUserUnitInternal(unit);
                    break;

                case SideType.EnemySide:
                    Debug.Log("AI turn start");
                    await _botExecutionTurnService.ExecuteBotTurnAsync(unit);
                    Debug.Log("AI turn end");
                    EndCurrentTurn();
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Вызывается UI-кнопками или событиями окончания ходов
        /// </summary>
        public void EndCurrentTurn()
        {
            _turnCompletionSource?.TrySetResult();
            var currentUnit = units[currentUnitIndex];

            if (currentUnit.Characteristic.Side == SideType.UserSide)
            {
                _actionClickHandler.CancelAction();
            }
        }

        public async UniTask SkipTurnAsync()
        {
            var currentUnit = units[currentUnitIndex];
            if (currentUnit.Characteristic.Side == SideType.UserSide)
            {
                EndCurrentTurn();
            }
            await UniTask.CompletedTask;
        }

        private void AdvanceToNextUnit()
        {
            currentUnitIndex = (currentUnitIndex + 1) % units.Count;

            if (currentUnitIndex == 0)
            {
                turnCount++;
                Debug.LogWarning($"Turn #{turnCount} done");
                _unitManager.RefreshUnitsActionPoints();
                UpdateMoveCounterDisplay();
            }
        }

        private void ActivateUserUnitInternal(Unit unit)
        {
            _uiAnimationService.SwitchPanelUnitIconAsync($"Blue{unit.Name}").Forget();

            _unitPanelBarService.SetUnitHealthPoints(
                actualHealthPoints: unit.ActualHealthPoints,
                maxHealthPoints: unit.Characteristic.HealthPoints
            );

            _actionUiService.ShowActions(unit);

            _unitPanelBarService.SetUnitActionPoints(
                actualActionPoint: unit.ActualActionPoints,
                maxActionPoint: unit.Characteristic.ActiveActionPoints
            );

            unit.IsSelected = true;
        }

        private void DeactivateUnitInternal(Unit unit)
        {
            _actionUiService.HideActions();
            unit.IsSelected = false;
            _gameGlobalStateManager.SelectedUnit = null;
        }

        private void UpdateMoveCounterDisplay()
        {
            SetTurnTextAsync($"Turn: {turnCount}").Forget();
        }

        private async UniTask<bool> CheckForGameOverAsync()
        {
            var sides = _unitManager.Units
                .GroupBy(x => x.Characteristic.Side)
                .ToDictionary(
                    x => x.Key,
                    x => x.Any(y => !y.IsDead)
                );

            if (sides.Values.Any(hasAlive => !hasAlive))
            {
                if (sides.TryGetValue(SideType.UserSide, out var userAlive) && !userAlive)
                {
                    Debug.Log("Все юниты игрока мертвы! Поражение.");
                }
                else if (sides.TryGetValue(SideType.EnemySide, out var enemyAlive) && !enemyAlive)
                {
                    Debug.Log("Все враги мертвы! Победа.");

                    if (await _roomService.TrySwitchNextRoom(true))
                    {
                        Debug.Log("Переход в следующую комнату");

                        await _unitManager.GenerateWaveUnits(
                            roomId: _gameGlobalStateManager.ActualRoomId,
                            waveOrder: _gameGlobalStateManager.ActualWaveOrder,
                            withDeleteActual: true
                        );

                        units = _unitManager.Units;
                        currentUnitIndex = 0;
                        return false;
                    }
                }

                SceneManager.LoadScene("MainMenuScene");
                return true;
            }

            return false;
        }

        private async UniTask SetTurnTextAsync(string fullText)
        {
            string currentText = "";
            for (int i = 0; i < fullText.Length; i++)
            {
                currentText += fullText[i];
                _moveCounterText.text = currentText;
                await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
            }
        }
    }
}