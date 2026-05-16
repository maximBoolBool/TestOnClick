using Assets.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Assets.Scripts.Services
{
    public interface IEnemyPanelService
{
    public void ShowUnitInfo(Unit unit);

    public void HideUnitInfo();
}

public class EnemyPanelService : IEnemyPanelService
{
    private const string HEALTH_BAR_SLIDER_NAME = "HealthBar";
    private const string ACTION_BAR_SLIDER_NAME = "ActionBar";
    private const string NAME = "UnitName";

    private readonly GameObject _enemyPanel;

    private readonly Slider _healthPointSliderSlider;
    private readonly Slider _actionPointsSlider;
    private readonly TextMeshProUGUI _nameText;

    public EnemyPanelService([Inject(Id = Constants.EnemyInfoPanel)] GameObject enemyPanel)
    {
        _enemyPanel = enemyPanel;

        var panelTranform =  _enemyPanel.transform;
        _healthPointSliderSlider = panelTranform.Find(HEALTH_BAR_SLIDER_NAME).GetComponent<Slider>();
        _actionPointsSlider = panelTranform.Find(ACTION_BAR_SLIDER_NAME).GetComponent<Slider>();
        _nameText = panelTranform.Find(NAME).GetComponent<TextMeshProUGUI>();
    }
    
    void IEnemyPanelService.ShowUnitInfo(Unit unit)
    {
        if (unit.IsSelected)
        {
            return;
        }

        _nameText.text = unit.Name;

        _healthPointSliderSlider.maxValue = unit.Characterictics.HealthPoints;
        _healthPointSliderSlider.value = unit.ActualHealthPoints;

        _actionPointsSlider.maxValue = unit.Characterictics.ActiveActionPoints;
        _actionPointsSlider.value = unit.ActualActionPoints;

        var worldPosition = unit.transform.position;
        var screenPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, worldPosition);
        _enemyPanel.transform.position = screenPosition;
        _enemyPanel.SetActive(true);
    }

    public void HideUnitInfo()
    {
        _enemyPanel.SetActive(false);
    }
    }
}