using Assets.Scripts.Enums;
using System;
using UnityEngine;

namespace Assets.Scripts.Helpers
{
    public static class UnitExtension
    {
        private const string UnitAnimatorPath = "UnitAnimator";
        private const string UnitIcon = "UnitIcon";

        public static GameObject GetUnitAnimator(this Unit unit)
        {
            return unit.transform.Find(UnitAnimatorPath).gameObject;
        }

        public static GameObject GetUnitIcon(this Unit unit)
        {
            return unit.transform.Find(UnitIcon).gameObject;
        }

        public static void SwitchUnitVisual(this Unit unit, UnitVisualType visualType)
        {
            var unitIcon = unit.GetUnitIcon();
            var unitAnimator = unit.GetUnitAnimator();

            switch (visualType)
            {
                case UnitVisualType.Animation:
                    unitIcon.SetActive(false);
                    unitAnimator.SetActive(true);
                    break;
                case UnitVisualType.Icon:
                    unitIcon.SetActive(true);
                    unitAnimator.SetActive(false);
                    break;
                default:
                    throw new NotImplementedException($"Visual type {visualType} is not implemented.");
            }
        }
    }
}
