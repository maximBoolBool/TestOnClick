using Assets.Scripts.Services;
using Assets.Scripts.Services;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Behaviours
{
    public class EquipmentBehaviour : MonoBehaviour
{
    private bool isPannelShow = false;

    [Inject]
    private readonly IEquipmentService _equipmentService;

    public void OnEquipmentButtonCick()
    {
        isPannelShow = !isPannelShow;
        _equipmentService.SetEquipemtPanelActive(isPannelShow);
    }
    }
}