using Assets.Scripts.Models.Equipment;
using UnityEngine;

public class SlotInfo : MonoBehaviour
{
    public CharacterEquipmentSlotType? EquipmentSetType { get; set; }
    public int Order { get; set; }
    public CellType CellType { get; set; }
}