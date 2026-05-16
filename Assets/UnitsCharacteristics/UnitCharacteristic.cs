using System.Collections.Generic;
using UnityEngine;

namespace Assets.UnitsCharacteristics
{
    public class UnitCharacteristic
{
    public int HealthPoints { get; set; }
    public int ActiveActionPoints { get; set; }

    public int ReactionActionPoints { get; set; }
    public int Agility { get; set; }

    public int MeleeSkill { get; set; }
    public int DefendSkill { get; set; }
    public SideType Side { get; set; }
}

public static class SideTypeExtension
{
    private static readonly Dictionary<SideType, HashSet<SideType>> sideTypePairs = new() 
    {
        [SideType.UserSide] = new HashSet<SideType> { SideType.EnemySide },
        [SideType.EnemySide] = new HashSet<SideType> {SideType.UserSide},
    };

    public static bool IsEnemyType(this SideType currentType, SideType checkType)
    {
        if (sideTypePairs.TryGetValue(currentType, out var enemieTypes))
        {
            return enemieTypes.Contains(checkType);
        }

        Debug.LogWarning($"Forgot add enemies types for type {currentType}");
        return false;
    }
}

    public enum SideType
    {
        UserSide = 0,
        EnemySide = 1,
    }
}