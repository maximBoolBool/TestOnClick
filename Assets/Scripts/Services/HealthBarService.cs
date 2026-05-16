using TMPro;
using UnityEngine.UI;
using Zenject;

public interface IHealthBarService
{
    void SetUnitHelthPoints(int actualHealthPoints, int maxHealthPoints);
}

public class HealthBarService : IHealthBarService
{
    private readonly Slider _slider;

    private readonly TextMeshProUGUI _healthBarText;

    public HealthBarService(
        [Inject(Id = Constants.HealthBarSlider)] Slider slider,
        [Inject(Id = Constants.HealthBarText)] TextMeshProUGUI healthBarText
    )
    {
        _slider = slider ?? throw new System.ArgumentNullException(nameof(slider));
        _healthBarText = healthBarText ?? throw new System.ArgumentException(nameof(healthBarText));
    }

    public void SetUnitHelthPoints(int actualHealthPoints, int maxHealthPoints)
    {
        _slider.maxValue = maxHealthPoints;
        _slider.value = actualHealthPoints;
        _healthBarText.text = $"{actualHealthPoints}/{maxHealthPoints}";
    }
}