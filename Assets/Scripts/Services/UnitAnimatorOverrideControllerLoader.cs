using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Assets.Scripts.Services
{
    class UnitAnimatorOverrideControllerLoader
    {
        public static async UniTask<AnimatorOverrideController> LoadAnimatorController(string key)
        {
            var controller = await Addressables.LoadAssetAsync<AnimatorOverrideController>(key).ToUniTask();

            if (controller == null)
            {
                Debug.LogError($"Не удалось синхронно загрузить контроллер: {key}");
            }

            return controller;
        } 
    }
}
