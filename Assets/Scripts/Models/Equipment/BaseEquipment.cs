using System.Collections.Generic;
using Assets.Scripts.Models.Actions;
using Assets.Scripts.Models.Conditions;

namespace Assets.Scripts.Models.Equipment
{
    public interface ISlotEquipment
{
    CharacterEquipmentSlotType SlotType { get; set; }
}

public abstract class BaseEquipment : ISlotEquipment
{
    public string Name { get; set; }

    public string Description { get; set; }

    public BaseCondition[] Conditions { get; set; }

    public BaseAction[] Actions { get; set; }

    public BaseActionStep[] ActionsSteps { get; set; }

    public CharacterEquipmentSlotType SlotType { get; set; }

    public CellType Located { get; set; }

    public string IconName { get; set; }

    public int RuneMaxCount { get; set; }

    public List<Rune> Runes { get; set; }
}

public class WeaponEquipment: BaseEquipment{}

public enum CharacterEquipmentSlotType
{
    Head = 0,
    Body = 1,
    Arm = 2,
    Legs = 3,
    Bag = 4,
    Rune = 5,
}

    public enum CellType
    {
        Unit = 0,
        SharedStorage = 1
    }
}