using Assets.Scripts.Models.Actions;
using Assets.Scripts.Models.Conditions;
using Assets.Scripts.Models.Equipment;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Helpers
{
    public static class EquipemntHelper
{
    public static ISlotEquipment[] GetStartedEquipment()
    {
        return new ISlotEquipment[]
        {
            new WeaponEquipment()
            {
                Name = "Axe",
                Description = "",
                Conditions = Array.Empty<BaseCondition>(),
                Actions = new []
                {
                    new EnemyUnitTargetAction()
                    {
                        PointCost = 3,
                        Name = "Chop",
                        Description = "Hit enemy. Fust!!",
                        Range = 1,
                        Steps = new BaseActionStep[]
                        {
                            new ActionDamageStep()
                            {
                                Order = 1,
                                MaxDamageValue = 15,
                                MinDamageValue = 25
                            },
                        }
                    }
                },
                ActionsSteps = Array.Empty<BaseActionStep>(),
                SlotType = CharacterEquipmentSlotType.Arm,
                Located = CellType.SharedStorage,
                IconName = "Axe",
                RuneMaxCount = 1,
                Runes = new List<Rune>()
            },
            new WeaponEquipment()
            {
                Name = "Sword",
                Description = "",
                Conditions = Array.Empty<BaseCondition>(),
                Actions = new []
                {
                    new EnemyUnitTargetAction()
                    {
                        PointCost = 2,
                        Name = "Slice",
                        Description = "Hit enemy. Fust!!",
                        Range = 1,
                        Steps = new BaseActionStep[]
                        {
                            new ActionDamageStep()
                            {
                                Order = 1,
                                MaxDamageValue = 10,
                                MinDamageValue = 20
                            },
                        }
                    }
                },
                SlotType = CharacterEquipmentSlotType.Arm,
                Located = CellType.SharedStorage,
                IconName = "Sword",
                RuneMaxCount = 1,
                Runes = new List<Rune>()
            },
            new Rune
            {
                Type = RuneType.Water,
                SlotType = CharacterEquipmentSlotType.Rune
            },
            new Rune
            {
                Type = RuneType.Air,
                SlotType = CharacterEquipmentSlotType.Rune
            }
        };
    }

    public static readonly (CharacterEquipmentSlotType Type, int Count)[] CHARECTER_EQUIPMENT_SLOTS = new (CharacterEquipmentSlotType Type, int Count)[]
    {
        (CharacterEquipmentSlotType.Head, 1),
        (CharacterEquipmentSlotType.Body, 1),
        (CharacterEquipmentSlotType.Arm, 2),
        (CharacterEquipmentSlotType.Legs, 1),
        (CharacterEquipmentSlotType.Bag, 4)
    };

    public static readonly Dictionary<(CharacterEquipmentSlotType Type, int Order), (int xCordinate, int yCordinate)> CHARECTER_EQUIPMENT_CORDINATES = new()
    {
        [(CharacterEquipmentSlotType.Head, 0)] = (0, 65),
        [(CharacterEquipmentSlotType.Arm, 0)] = (-65, 0),
        [(CharacterEquipmentSlotType.Arm, 1)] = (65, 0),
        [(CharacterEquipmentSlotType.Body, 0)] = (0, 0),
        [(CharacterEquipmentSlotType.Legs, 0)] = (0, -65)
    };
    }
}