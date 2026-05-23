using Assets.Scripts.Models.Equipment;
using System;

namespace Assets.Scripts.Helpers
{
    public static class RuneHelper
{
    public static void GetRuneEffect()
    {
    }

    public static string GetRuneIconName(RuneType type)
    {
        return type switch 
        { 
            RuneType.Fire => "RuneFire",
            RuneType.Air => "RuneAir",
            RuneType.Water => "RuneWater",
            RuneType.Earth => "RuneEarth",
            _ => throw new NotImplementedException()
        };
    }

    public static bool IsRunCanBeSet(CharacterEquipmentSlotType type)
    {
        return type switch
        {
            CharacterEquipmentSlotType.Head => true,
            CharacterEquipmentSlotType.Body => true,
            CharacterEquipmentSlotType.Arm => true,
            CharacterEquipmentSlotType.Legs => false,
            CharacterEquipmentSlotType.Rune => false,
            CharacterEquipmentSlotType.Bag => false,
            _ => throw new NotImplementedException()

        };
    }
    }
}