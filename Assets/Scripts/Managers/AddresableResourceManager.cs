using Assets.Scripts.Helpers;
using Assets.Scripts.Managers.UnitManager;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Tilemaps;
using Zenject;

namespace Assets.Scripts.Managers
{
    public interface IAddresableResourceManager
    {
        UniTask LoadGameResourceAsync();

        UniTask LoadLevelResourceAsync();

        UniTask FreeLevelResourceAsync();

        UniTask FreeGameResourceAsync();
    }

    public class AddresableResourceManager : IAddresableResourceManager
    {
        #region Inject

        [Inject]
        private readonly IUnitManager _unitManager;

        #endregion

        #region States

        private Dictionary<string, Sprite> _unitIcons = new();

        private Dictionary<string, Tile> _tiles = new();

        private Dictionary<string, AnimatorOverrideController> _overrideAnimationControllers = new();

        #endregion

        #region Public Methodes

        public async UniTask FreeGameResourceAsync()
        {

        }

        public async UniTask FreeLevelResourceAsync()
        {

        }

        public async UniTask LoadGameResourceAsync()
        {
            await LoadTilesInternalAsync();
        }

        public async UniTask LoadLevelResourceAsync()
        {
            await LoadAnimationControllersAsync();
            await LoadUnitIconsAsync();
        }

        #endregion
        
        #region Private Methodes

        private async UniTask LoadTilesInternalAsync()
        {

        }

        private async UniTask LoadUnitIconsAsync()
        {
            var iconNames = _unitManager.Units
                .Select(x => UnitAdressableLoaderHelper.GetUnitIconAddressableName(x.Name, x.Characteristic.Side))
                .Distinct()
                .ToArray();

            var sprites = await Addressables.LoadAssetsAsync<Sprite>(
                iconNames,
                callback: null,
                Addressables.MergeMode.Union
            );

            foreach (var sprite in sprites)
            {
                _unitIcons[sprite.texture.name] = sprite;
            }
        }

        private async UniTask LoadAnimationControllersAsync()
        {            
            var overrideControllerNames = _unitManager.Units
                .Select(x => UnitAdressableLoaderHelper.GetUnitOverrideAnimationAddressableName(x.Name, x.Characteristic.Side))
                .Distinct()
                .ToArray();
            
            var overrideControllers = await Addressables.LoadAssetsAsync<AnimatorOverrideController>(
                overrideControllerNames,
                callback: null,
                Addressables.MergeMode.Union
            );

            foreach (var controller in overrideControllers)
            {
                _overrideAnimationControllers[controller.name] = controller;
            }
        }

        #endregion
    }
}
