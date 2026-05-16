using UnityEngine;
using Zenject;

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