using Assets.UnitsCharacteristics;
using System;

namespace Assets.Scripts.Helpers
{
    public static class UnitAdressableLoaderHelper
    {
        public static string GetUnitIconAddressableName(string unitName, SideType side)
        {
            var sidePrefix = side switch
            {
                SideType.UserSide => "Blue",
                SideType.EnemySide => "Red",
                _ => throw new InvalidOperationException($"Unknown side type: {side}")
            };
            return $"{sidePrefix}{unitName}";
        }

        public static string GetUnitOverrideAnimationAddressableName(string unitName, SideType side)
        {
            if (side == SideType.EnemySide)
            {
                return unitName == "Monk"
                    ? "RedMonkAnimatorOverrideController"
                    : "RedWarriorAnimatorOverrideContoller";
            }

            return unitName == "Monk"
                ? "BlueMonkAnimatorController"
                : "BlueWarriorAnimatorController";
        }
    }
}
