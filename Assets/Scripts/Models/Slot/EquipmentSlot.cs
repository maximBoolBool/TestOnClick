public class EquipmentSlot
{
    public CharacterEquipmentSlotType Type { get; set; }

    public ISlotEquipment? Equipment { get; set; }

    public int Order { get; set; }

    public bool IsEquipped => Equipment != null;
}