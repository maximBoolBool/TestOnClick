using UnityEngine;
using UnityEngine.EventSystems;

public class EquipmentSlotBehaviour : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (transform.Find("Image") != null)
        {
            var draggableItem = eventData.pointerDrag.GetComponent<DraggbleItemBehaviour>();
            if (draggableItem != null)
            {
                draggableItem.parentAfterDrag = transform;
            }
        }
    }
}