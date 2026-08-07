using Assets.Scripts.Services;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Behaviours
{
    public class LayerBehaviour : MonoBehaviour
    {
        [Inject]
        private readonly IGridLayerService _gridLayersService;

        public void OnLayerUp()
        {
            _gridLayersService.LayerUp();
        }

        public void OnLayerDown() 
        {
            _gridLayersService.LayerDown();
        }
    }
}
