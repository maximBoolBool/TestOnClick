using Assets.Scripts.Models.Actions;
using Assets.UnitsCharacteristics;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Services
{
    public interface IActionExecutionService
    {
        void TryExecute(BaseAction action, Vector3Int? target);
    }

    public class ActionExecutionService : IActionExecutionService
    {
        [Inject]
        private readonly IHitService _hitService;

        [Inject]
        private readonly IDamageService _damageService;

        [Inject]
        private readonly IUnitManager _unitManager;

        [Inject]
        private readonly IActionUIService _actionUIService;

        [Inject]
        private readonly IActionClickHandler _actionClickHandler;

        [Inject]
        private readonly IGridService _gridService;

        [Inject]
        private readonly IAnimationService _animationService;

        public void TryExecute(
            BaseAction action,
            Vector3Int? target
        )
        {
            try
            {
                switch (action.Type)
                {
                    case ActionTargetType.UnitPeack:
                    case ActionTargetType.AreaPeack:
                        if (target == null)
                        {
                            Debug.LogError("This action can only be performed with the target");
                            break;
                        }

                        if (!_actionClickHandler.IsCanBeTarget(target.Value))
                        {
                            Debug.LogWarning("Tile can not be target");
                            break;
                        }

                        ExecuteTargetActionIternal(action.Steps, target.Value, action.PointCost);
                        break;
                    case ActionTargetType.SelfPeak:
                    default:
                        throw new NotImplementedException();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error occurred when executing action: {ex.Message}");
            }
            finally
            {
                _actionClickHandler.CancelAction();
            }
        }

        private void ExecuteTargetActionIternal(
            BaseActionStep[] steps,
            Vector3Int target,
            int pointCost
        )
        {
            var currentUnit = _unitManager.Units.FirstOrDefault(x => x.IsSelected);
            if (currentUnit == null)
            {
                throw new InvalidOperationException("No unit is currently selected");
            }
            var targetUnit = _unitManager.Units.FirstOrDefault(x => _gridService.ToGridCordinates(x) == target);
            if (targetUnit == null)
            {
                Debug.LogWarning("No unit found at target position");
                return;
            }
            var random = new System.Random();

            var stepResults = new Dictionary<int, bool>();

            foreach (var step in steps.OrderBy(x => x.Order))
            {
                if (step.NeedCheckPreviousResult)
                {
                    if (!stepResults.TryGetValue(step.BeforeStepResult.Value, out var stepResult))
                    {
                        Debug.LogWarning("Previous step result not found for validation");
                        continue;
                    }

                    if (!stepResult)
                    {
                        continue;
                    }
                }

                switch (step.Type)
                {
                    case ActionStepType.Damage:

                        _animationService.SwitchUnitAnimation(currentUnit, UnitAnimationType.Attack, true);
                        var isHit = _hitService.IsHit(
                            targetUnit.Characteristic.DefendSkill,
                            currentUnit.Characteristic.MeleeSkill,
                            new Dictionary<HitModifierType, int>()
                        );

                        if (isHit)
                        {
                            var damageSetStep = step as ActionDamageStep;
                            _damageService.SetUnitDamage(
                                targetUnit: targetUnit,
                                damagePoints: random.Next(damageSetStep.MinDamageValue, damageSetStep.MaxDamageValue)
                            );
                            stepResults.TryAdd(step.Order, true);
                        }
                        else
                        {
                            stepResults.TryAdd(step.Order, false);
                        }
                        break;
                    case ActionStepType.EffectWithDuration:
                        var effectStep = step as EffectWithDurationStep;
                        var chance = random.Next(0, 100);
                        if (chance > effectStep.BaseChanceToHit)
                        {
                            continue;
                        }

                        if (effectStep.Duration != null)
                        {
                            targetUnit.DuratationConditions.Add((effectStep.Condition, effectStep.Duration.Value));
                        }
                        else
                        {
                            targetUnit.GlobalConditions.Add(effectStep.Condition);
                        }

                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            currentUnit.ActualActionPoints -= pointCost;

            if (currentUnit.Characteristic.Side == SideType.UserSide && currentUnit.IsSelected)
            {
                _actionUIService.SetActionPoints(
                     currentValue: currentUnit.ActualActionPoints,
                     maxValue: currentUnit.Characteristic.ActiveActionPoints
                 );
            }
        }

        private void ExecuteTargetActionIternal(
            BaseActionStep[] steps
        )
        {
            foreach (var step in steps.OrderBy(x => x.Order))
            {
                switch (step.Type)
                {
                    case ActionStepType.Damage:
                        return;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}