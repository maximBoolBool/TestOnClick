using Assets.Scripts.Managers;
using Assets.Scripts.Services;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Behaviours
{
    public class EquipmentBehaviour : MonoBehaviour
    {        
        [Inject]
        private readonly IEquipmentService _equipmentService;

        [Inject]
        private readonly IEquipmentUiManager _equipmentUiManager;

        public void OnEquipmentButtonCick()
        {
            _equipmentUiManager.SetEquipmentPanelActive(!_equipmentUiManager.IsEquipmentPanelActive);
            _equipmentService.SetEquipemtPanelActive(_equipmentUiManager.IsEquipmentPanelActive);
        }
    }
}