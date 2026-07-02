using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Assets.Scripts.Services
{
    class UnitAnimatorOverrideControllerLoader
    {
        public static AnimatorOverrideController LoadController(string key)
        {
            var controller = Addressables.LoadAssetAsync<AnimatorOverrideController>(key).WaitForCompletion();

            if (controller == null)
            {
                Debug.LogError($"Не удалось синхронно загрузить контроллер: {key}");
            }

            return controller;
        } 
    }
}
