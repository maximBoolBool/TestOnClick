using Assets.Db.Enums;
using Assets.Scripts.Models.Conditions;
using Assets.Scripts.Services;
using Assets.UnitsCharacteristics;
using System;
using System.Collections;
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
        public void SceneStart();

        public void EndTurn();

        public void SkipTurn();

        public void ActivateUnit(Unit unit);

        public void DeactivateUnit(Unit unit);
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
        private readonly IAiTurnService _aiTurnService;

        [Inject]
        private readonly IHealthBarService _healthBarService;

        [Inject]
        private readonly IActionUIService _actionUiService;

        [Inject]
        private readonly IGameGlobalStateManager _gameGlobalStateManager;

        [Inject]
        private readonly IRoomService _roomService;

        [Inject]
        private readonly ICameraService _cameraService;

        [Inject]
        private readonly ILocationService _locationService;

        private Coroutine _textAnimationCoroutine;

        public void SceneStart()
        {
            if (_locationService.NeedGenerateLocationInfo())
            {
                _locationService.WriteLocationInfo();
            }

            _roomService.TrySwitchNextRoom(false);

            _unitManager.GenerateUnits();
            // Пока убираем
            //_unitManager.SetStartEquipment();
            units = _unitManager.Units;
            _unitManager.RefreshUnitsActionPoints();
            _unitManager.SetActualHealthPoins();

            if (units.Count > 0)
            {
                turnCount = 1;
                UpdateMoveCounterDisplay();
                ActivateUnit(units[currentUnitIndex]);
            }
        }

        public void EndTurn()
        {
            currentUnitIndex = (currentUnitIndex + 1) % units.Count;

            if (currentUnitIndex == 0)
            {
                turnCount++;
                Debug.LogWarning($"Turn #{turnCount} done");
                _unitManager.RefreshUnitsActionPoints();
                UpdateMoveCounterDisplay();

            }

            ActivateUnit(units[currentUnitIndex]);
        }

        public void SkipTurn()
        {
            if (units[currentUnitIndex].Characterictics.Side == SideType.UserSide)
            {
                DeactivateUnit(units[currentUnitIndex]);
            }
        }

        public void ActivateUnit(Unit unit)
        {
            _conditionService.ExecuteConditionEffect(unit, ConditionEffectStartType.OnTurnStart);

            if (unit.IsDead)
            {
                SkipTurn();
            }

            CheckForGameOver();

            _cameraService.MoveCamera(unit.transform.position);

            switch (unit.Characterictics.Side)
            {
                case SideType.UserSide:
                    ActivateUserUnitIternal(unit);
                    break;
                case SideType.EnemySide:
                    Debug.Log("Ai turn start");
                    ActivateEnemyUnitIternal(unit);
                    Debug.Log("Ai turn start");
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void ActivateUserUnitIternal(Unit unit)
        {
            _healthBarService.SetUnitHelthPoints(
                actualHealthPoints: unit.ActualHealthPoints,
                maxHealthPoints: unit.Characterictics.HealthPoints
            );
            _actionUiService.ShowActions(unit);
            _actionUiService.SetActionPoints(
                currentValue: unit.ActualActionPoints,
                maxValue: unit.Characterictics.ActiveActionPoints
            );
            unit.IsSelected = true;
        }

        private void ActivateEnemyUnitIternal(Unit unit)
        {
            _aiTurnService.ExecuteAiTurn(unit);
        }

        public void DeactivateUnit(Unit unit)
        {
            _actionUiService.HideActions();
            unit.IsSelected = false;
            _gameGlobalStateManager.SelectedUnit = null;
            EndTurn();
        }

        private void UpdateMoveCounterDisplay()
        {
            if (_moveCounterText != null)
            {
                if (_textAnimationCoroutine != null)
                {
                    _moveCounterText.StopCoroutine(_textAnimationCoroutine);
                }

                _textAnimationCoroutine = _moveCounterText.StartCoroutine(SetTurnText($"Turn: {turnCount}"));
            }
            else
            {
                Debug.LogWarning("MoveCounterText не назначен в инспекторе!");
            }
        }

        private void CheckForGameOver()
        {
            var sides = _unitManager.Units
                .GroupBy(x => x.Characterictics.Side)
                .ToDictionary(
                    x => x.Key,
                    x => x.Where(y => !y.IsDead).Any()
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

                    if (_roomService.TrySwitchNextRoom(true))
                    {
                        Debug.Log("Переход в следующую комнату");
                        return;
                    }
                }

                SceneManager.LoadScene("MainMenuScene");
                return;
            }
        }

        IEnumerator SetTurnText(string fullText)
        {
            string currentText = "";

            for (int i = 0; i < fullText.Length; i++)
            {
                currentText += fullText[i];
                _moveCounterText.text = currentText;

                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}

public static class StartConfiguration
{
    public const LocationType LOCATION_TYPE = LocationType.Forest;
}