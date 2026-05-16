using Assets.Scripts;
using Zenject;

public interface IUiService
{
    public void RefreshUnitUi(Unit unit);
}

public class UiService : IUiService
{
    [Inject]
    private readonly IActionUIService _actionUIService;

    public void RefreshUnitUi(Unit unit)
    {
        _actionUIService.HideActions();
        _actionUIService.ShowActions(unit);
    }
}