using TMPro;
using UnityEngine.UI;
using Zenject;

namespace Assets.Scripts.Services
{
    public interface IUnitPanelBarService
    {
        void SetUnitHealthPoints(int actualHealthPoints, int maxHealthPoints);

        void SetUnitActionPoints(int actualActionPoint, int maxActionPoint);
    }

    public class UnitPanelBarService : IUnitPanelBarService
    {
        private readonly Slider _healthBarSlider;
        private readonly TextMeshProUGUI _healthBarText;

        private readonly Slider _actionPointsBarSlider;
        private readonly TextMeshProUGUI _actionPointsBarText;

        public UnitPanelBarService(
            [Inject(Id = Constants.HEALTH_BAR_SLIDER)] Slider healthBarSlider,
            [Inject(Id = Constants.HEALTH_BAR_TEXT)] TextMeshProUGUI healthBarText,
            [Inject(Id = Constants.ACTION_POINTS_BAR_SLIDER)] Slider actionPointsBarSlider,
            [Inject(Id = Constants.ACTION_POINTS_BAR_TEXT)] TextMeshProUGUI actionPointsBarText
        )
        {
            _healthBarSlider = healthBarSlider;
            _healthBarText = healthBarText;

            _actionPointsBarSlider = actionPointsBarSlider;
            _actionPointsBarText = actionPointsBarText;
        }

        public void SetUnitHealthPoints(int actualHealthPoints, int maxHealthPoints)
        {
            UpdateBarIternal(
                actualPoints: actualHealthPoints,
                maxPointsValue: maxHealthPoints,
                slider: _healthBarSlider,
                text: _healthBarText
            );
        }

        public void SetUnitActionPoints(int actualActionPoint, int maxActionPoint)
        {
            UpdateBarIternal(
                actualPoints: actualActionPoint,
                maxPointsValue: maxActionPoint,
                slider: _actionPointsBarSlider,
                text: _actionPointsBarText
            );
        }

        private void UpdateBarIternal(
            int actualPoints,
            int maxPointsValue,
            Slider slider,
            TextMeshProUGUI text
        )
        {
            slider.maxValue = maxPointsValue;
            slider.value = actualPoints;
            text.text = $"{actualPoints}/{maxPointsValue}";
        }
    }
}