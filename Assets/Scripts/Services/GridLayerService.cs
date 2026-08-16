using Assets.Scripts.Enums;
using Assets.Scripts.Managers;
using Assets.Scripts.Managers.UnitManager;
using Cysharp.Threading.Tasks;
using Zenject;

namespace Assets.Scripts.Services
{
    public interface IGridLayerService
    {
        UniTask LayerUpAsync();
        UniTask LayerDownAsync();
    }

    public class GridLayerService : IGridLayerService
    {
        [Inject]
        private readonly IGridLayersManager _gridLayersManager;

        [Inject]
        private readonly IUnitManager _unitManager;


        public async UniTask LayerUpAsync()
        {
            var newLayer = _gridLayersManager.ActualLayer.GetLayerOver();
            var previousLayer = _gridLayersManager.ActualLayer;

            if (newLayer != null && await _gridLayersManager.TrySetLayerVisualAsync(newLayer.Value))
            {
                _unitManager.SwitchUnitVisual(
                    actualLayer: newLayer.Value,
                    previousLayer: previousLayer
                );
            }
        }

        public async UniTask LayerDownAsync()
        {
            var newLayer = _gridLayersManager.ActualLayer.GetLayerUnder();
            var previousLayer = _gridLayersManager.ActualLayer;

            if (newLayer != null && await _gridLayersManager.TrySetLayerVisualAsync(newLayer.Value))
            {
                _unitManager.SwitchUnitVisual(
                    actualLayer: newLayer.Value,
                    previousLayer: previousLayer
                );
            }
        }
    }
}
