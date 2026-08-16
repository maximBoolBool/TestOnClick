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

        UniTask EndTurnAsync();

        UniTask SkipTurnAsync();

        UniTask ActivateUnitAsync(Unit unit);

        UniTask DeactivateUnitAsync(Unit unit);
    }

    public class TurnManager : ITurnManager
    {
        private int currentUnitIndex = 0;
        private int turnCount = 0;
        private List<Unit> units;

        [Inject(Id = Constants.TurnCountText)]
        private readonly TextMeshProUGUI _moveCounterText;

        [Inject]
        private readonly IUnitManager _unitManager;

        [Inject]
        private readonly IConditionService _conditionService;

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

        public async UniTask SceneStart()
        {
            _uiAnimationService.ShakeCamera();
            _uiAnimationService.MoveVeils();
            await _roomService.TrySwitchNextRoom(false);

            await _unitManager.GenerateUnits();
            // Пока убираем
            //_unitManager.SetStartEquipment();
            units = _unitManager.Units;
            _unitManager.RefreshUnitsActionPoints();
            _unitManager.SetActualHealthPoins();

            if (units.Count > 0)
            {
                turnCount = 1;
                UpdateMoveCounterDisplay();
                await ActivateUnitAsync(units[currentUnitIndex]);
            }
        }

        public async UniTask EndTurnAsync()
        {
            currentUnitIndex = (currentUnitIndex + 1) % units.Count;

            if (currentUnitIndex == 0)
            {
                turnCount++;
                Debug.LogWarning($"Turn #{turnCount} done");
                _unitManager.RefreshUnitsActionPoints();
                UpdateMoveCounterDisplay();

            }

            await ActivateUnitAsync(units[currentUnitIndex]);
        }

        public async UniTask SkipTurnAsync()
        {
            if (units[currentUnitIndex].Characteristic.Side == SideType.UserSide)
            {
                await DeactivateUnitAsync(units[currentUnitIndex]);
            }
        }

        public async UniTask ActivateUnitAsync(Unit unit)
        {
            _conditionService.ExecuteConditionEffect(unit, ConditionEffectStartType.OnTurnStart);

            if (unit.IsDead)
            {
                await DeactivateUnitAsync(units[currentUnitIndex]);
                return;
            }

            await CheckForGameOverAsync();

            _cameraService.MoveCamera(unit.transform.position);

            switch (unit.Characteristic.Side)
            {
                case SideType.UserSide:
                    ActivateUserUnitIternal(unit);
                    break;
                case SideType.EnemySide:
                    Debug.Log("Ai turn start");
                    ActivateEnemyUnitIternalAsync(unit).Forget();
                    Debug.Log("Ai turn start");
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void ActivateUserUnitIternal(Unit unit)
        {
            _uiAnimationService.SwitchPanelUnitIconAsync($"Blue{unit.Name}");

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

        private async UniTask ActivateEnemyUnitIternalAsync(Unit unit)
        {
            await _botExecutionTurnService.ExecuteBotTurnAsync(unit);
        }

        public async UniTask DeactivateUnitAsync(Unit unit)
        {
            _actionUiService.HideActions();
            unit.IsSelected = false;
            _gameGlobalStateManager.SelectedUnit = null;
            await EndTurnAsync();
        }

        private void UpdateMoveCounterDisplay()
        {
            SetTurnTextAsync($"Turn: {turnCount}").Forget();
        }

        private async UniTask CheckForGameOverAsync()
        {
            var sides = _unitManager.Units
                .GroupBy(x => x.Characteristic.Side)
                .ToDictionary(
                    x => x.Key,
                    x => x.Any(y => !y.IsDead)
                );

            foreach (var side in sides)
            {
                Debug.Log($"{side.Key} {side.Value}");
            }

            if (sides.Values.Any(x => !x))
            {
                if (sides.ContainsKey(SideType.UserSide) && !sides[SideType.UserSide])
                {
                    Debug.Log("Все юниты игрока мертвы! Поражение.");
                }
                else if (sides.ContainsKey(SideType.EnemySide) && !sides[SideType.EnemySide])
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
                        return;
                    }
                }

                SceneManager.LoadScene("MainMenuScene");
                return;
            }
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