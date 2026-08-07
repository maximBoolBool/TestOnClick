using Assets.Scripts.Enums;
using Assets.Scripts.Managers;
using Assets.Scripts.Managers.UnitManager;
using Zenject;

namespace Assets.Scripts.Services
{
    public interface IGridLayerService
    {
        void LayerUp();
        void LayerDown();
    }

    public class GridLayerService : IGridLayerService
    {
        [Inject]
        private readonly IGridLayersManager _gridLayersManager;

        [Inject]
        private readonly IUnitManager _unitManager;


        public void LayerUp()
        {
            var newLayer = _gridLayersManager.ActualLayer.GetLayerOver();
            var previousLayer = _gridLayersManager.ActualLayer;

            if (newLayer != null && _gridLayersManager.TrySetLayerVisual(newLayer.Value))
            {
                _unitManager.SwitchUnitVisual(
                    actualLayer: newLayer.Value,
                    previousLayer: previousLayer
                );
            }
        }

        public void LayerDown()
        {
            var newLayer = _gridLayersManager.ActualLayer.GetLayerUnder();
            var previousLayer = _gridLayersManager.ActualLayer;

            if (newLayer != null && _gridLayersManager.TrySetLayerVisual(newLayer.Value))
            {
                _unitManager.SwitchUnitVisual(
                    actualLayer: newLayer.Value,
                    previousLayer: previousLayer
                );
            }
        }
    }
}
