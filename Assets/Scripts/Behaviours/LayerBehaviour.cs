using Assets.Scripts.Services;
using Cysharp.Threading.Tasks;
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
            _gridLayersService.LayerUpAsync().Forget();
        }

        public void OnLayerDown() 
        {
            _gridLayersService.LayerDownAsync().Forget();
        }
    }
}
