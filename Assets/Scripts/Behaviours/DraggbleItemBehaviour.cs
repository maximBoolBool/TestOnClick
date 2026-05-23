using Assets.Scripts.Models.Equipment;
using Assets.Scripts.Services;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Behaviours
{
    public class DraggbleItemBehaviour : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Transform parentAfterDrag;
    private Transform originalParent;
    private CanvasGroup _canvasGroup;
    private Image _image;
    private GameObject dragIcon;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        var image = GetComponent<Image>();

        if (image == null)
        {
            return;
        }

        _image = image;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!_image.gameObject.activeSelf)
        { 
            return;
        }

        EquipmentService.Instance.ChangeCellBackGroundColor(GetCellTypeToPaint(), true);

        originalParent = transform.parent;
        parentAfterDrag = originalParent;

        _canvasGroup.blocksRaycasts = false;
        _image.color = new Color(1, 1, 1, 0.5f);

        dragIcon = Instantiate(gameObject, transform.root);
        dragIcon.transform.SetAsLastSibling();
        var iconCanvasGroup = dragIcon.GetComponent<CanvasGroup>();
        if (iconCanvasGroup != null)
        {
            iconCanvasGroup.blocksRaycasts = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
        {
            dragIcon.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
        {
            Destroy(dragIcon);
        }

        _image.color = new Color(1, 1, 1, 1f);
        _canvasGroup.blocksRaycasts = true;

        EquipmentService.Instance.ChangeCellBackGroundColor(GetCellTypeToPaint(), false);

        if (parentAfterDrag != originalParent)
        {
            EquipmentService.Instance.OnCellDrop(originalParent.gameObject, parentAfterDrag.gameObject);
        }
    }

    private CharacterEquipmentSlotType GetCellTypeToPaint()
    {
        var slotInfo = transform.parent.gameObject.GetComponent<SlotInfo>();
        if (slotInfo.EquipmentSetType != null)
        {
            return slotInfo.EquipmentSetType.Value;
        }
        else
        {
            return UnitManager.Instance
                .SharedEquipment
                .FirstOrDefault(x => x.Order == slotInfo.Order)
                .Equipment
                .SlotType;
        }
    }
    }
}
