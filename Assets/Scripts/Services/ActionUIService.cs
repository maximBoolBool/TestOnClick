using System.Collections.Generic;
using UnityEngine;
using Zenject;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Assets.Scripts.Models.Actions;
using Assets.Scripts;
using Assets.Scripts.Models.Equipment;

public interface IActionUIService
{
    void ShowActions(Unit unit);

    void HideActions();

    void SetActionPoints(int currentValue, int maxValue);

    void HideActionPointCostText();

    BaseAction[] GetUnitActions(Unit unit);
}

public class ActionUIService : IActionUIService
{
    [Inject(Id = Constants.ActionButtonPanel)]
    private readonly GameObject _actionButtonPanel;

    [Inject(Id = Constants.ActionButtonPrefab)]
    private readonly GameObject _actionButtonPrefab;

    [Inject(Id = Constants.ActionPointText)]
    private readonly TextMeshProUGUI _actionPointsText;

    [Inject]
    private readonly IActionClickHandler _actionClickHandler;

    private readonly List<GameObject> _spawnedButtons = new();

    public void ShowActions(Unit unit)
    {
        foreach(var action in GetUnitActions(unit))
        {
            var buttonObj = Object.Instantiate(
                original: _actionButtonPrefab,
                parent: _actionButtonPanel.transform
            );

            var button = buttonObj.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => _actionClickHandler.OnClick(action));
            var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();

            if (buttonText != null)
            {
                buttonText.text = action.Name;
            }
            else
            {
                Debug.Log($"Бля не нашел текст для {action.Name}");
            }

            _spawnedButtons.Add(buttonObj);
        }
    }

    public void HideActions()
    {
        foreach (var btn in _spawnedButtons)
        {
            if (btn != null) 
            { 
                Object.Destroy(btn);
            }
        }
        _spawnedButtons.Clear();
    }

    public void SetActionPoints(int currentValue, int maxValue)
    {
        _actionPointsText.text = $"{currentValue}/{maxValue}";
    }

    public void HideActionPointCostText()
    {
        _actionPointsText.text = string.Empty;
    }

    public BaseAction[] GetUnitActions(Unit unit)
    {
        var resultActions = new List<BaseAction>();

        resultActions.AddRange(unit.Actions);

        var equipmentActions = unit.EqupmentSlots
            .Where(x => x.IsEquipped)
            .Select(x => x.Equipment)
            .OfType<BaseEquipment>()
            .SelectMany(x => x.Actions)
            .ToArray();

        resultActions.AddRange(equipmentActions);

        if(resultActions.Count == 0)
        {
            resultActions.AddRange(ActionHelper.GetUserUnitTestActions());
        }

        return resultActions.ToArray();
    }
}