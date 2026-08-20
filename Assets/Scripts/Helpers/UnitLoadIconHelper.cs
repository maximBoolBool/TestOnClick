using Assets.UnitsCharacteristics;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Assets.Scripts.Helpers
{
    public static class UnitLoadIconHelper
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

        public static async UniTask<Sprite> LoadUnitIconAsync(string unitName, SideType side)
        {
            return await Addressables.LoadAssetAsync<Sprite>(GetUnitIconAddressableName(unitName, side));
        }
    }
}
