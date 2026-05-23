namespace Assets.Scripts.Models.Equipment
{
    public class Rune : ISlotEquipment
{
    public RuneType Type { get; set; }
    public CharacterEquipmentSlotType SlotType { get; set; }
}

    public enum RuneType
    {
        Water,
        Fire,
        Earth,
        Air,
    }
}