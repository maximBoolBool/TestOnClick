using Assets.Scripts.Managers;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Behaviours
{
    public class LayerBehaviour : MonoBehaviour
    {
        [Inject]
        private readonly IGridLayersManager _gridLayersManager;

        public void OnLayerUp()
        {
            _gridLayersManager.LayerUp();
        }

        public void OnLayerDown() 
        {
            _gridLayersManager.LayerDown();
        }
    }
}
