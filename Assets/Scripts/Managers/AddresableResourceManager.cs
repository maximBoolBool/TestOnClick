using Assets.Db;
using Assets.Db.Models;
using Assets.Scripts.Helpers;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Tilemaps;
using Zenject;

namespace Assets.Scripts.Managers
{
    public static class TilesAdressableResourceNames
    {
        public const string SHADOW_TILE_NAME = "ShadowTile";
        public const string HIGHLIGHT_TILE_NAME = "HighlightTile";
        public const string HOVER_TILE_NAME = "HoverTile";
    }

    public static class PrefabsAdressableResourceNames
    {
        public const string ACTION_BUTTON_PREFAB = "ActionButtonPrefab";
        public const string EQUIPMENT_SLOT_PREFAB = "EquipmentSlotPrefab";
        public const string UNIT_QUEUE_ITEM_PREFAB = "UnitQueueItemPrefab";
    }

    public interface IAddresableResourceManager
    {
        UniTask LoadGameResourceAsync();
        UniTask LoadLevelResourceAsync();
        UniTask FreeLevelResourceAsync();
        UniTask FreeGameResourceAsync();
        public Sprite GetUnitIconSprite(string key);
        public TileBase GetTileBase(string key);
        public AnimatorOverrideController GetUnitOverrideAnimationController(string key);
        public GameObject GetPrefab(string key);
    }

    public class AddresableResourceManager : IAddresableResourceManager
    {
        #region Injections

        [Inject]
        private readonly IGameGlobalStateManager _gameGlobalStateManager;

        [Inject]
        private readonly StaticDb _staticDb;

        #endregion

        #region States (Cache)
        private readonly Dictionary<string, Sprite> _unitIcons = new();
        private readonly Dictionary<string, TileBase> _tiles = new();
        private readonly Dictionary<string, AnimatorOverrideController> _overrideAnimationControllers = new();
        private readonly Dictionary<string, GameObject> _prefabs = new();
        #endregion

        #region Handles (For Memory Management)
        private AsyncOperationHandle<IList<TileBase>> _tilesHandle;
        private AsyncOperationHandle<IList<Sprite>> _unitIconsHandle;
        private AsyncOperationHandle<IList<AnimatorOverrideController>> _animControllersHandle;
        private AsyncOperationHandle<IList<GameObject>> _prefabsHandle;
        #endregion

        #region Public Methods

        public async UniTask LoadGameResourceAsync()
        {
            await LoadTilesInternalAsync();
            await LoadPrefabsAsync();
        }

        public async UniTask LoadLevelResourceAsync()
        {
            await LoadAnimationControllersAsync();
            await LoadUnitIconsAsync();
        }

        public async UniTask FreeGameResourceAsync()
        {
            if (_tilesHandle.IsValid())
            {
                Addressables.Release(_tilesHandle);
            }

            if (_prefabsHandle.IsValid())
            {
                Addressables.Release(_prefabsHandle);
            }

            _prefabs.Clear();
            _tiles.Clear();
        }

        public async UniTask FreeLevelResourceAsync()
        {
            if (_unitIconsHandle.IsValid())
            {
                Addressables.Release(_unitIconsHandle);
            }

            if (_animControllersHandle.IsValid())
            {
                Addressables.Release(_animControllersHandle);
            }

            _unitIcons.Clear();
            _overrideAnimationControllers.Clear();
        }

        public Sprite GetUnitIconSprite(string key)
        {
            return _unitIcons.GetValueOrDefault(key);
        }

        public TileBase GetTileBase(string key)
        {
            return _tiles.GetValueOrDefault(key);
        }

        public AnimatorOverrideController GetUnitOverrideAnimationController(string key)
        {
            return _overrideAnimationControllers.GetValueOrDefault(key);
        }

        public GameObject GetPrefab(string key)
        {
            var z = key;

            return _prefabs.GetValueOrDefault(key);
        }

        #endregion

        #region Private Methods

        private async UniTask LoadTilesInternalAsync()
        {
            var tileNames = new string[]
            {
                TilesAdressableResourceNames.HIGHLIGHT_TILE_NAME,
                TilesAdressableResourceNames.HOVER_TILE_NAME,
                TilesAdressableResourceNames.SHADOW_TILE_NAME
            };

            _tilesHandle = Addressables.LoadAssetsAsync<TileBase>(tileNames, null, Addressables.MergeMode.Union);
            var tiles = await _tilesHandle.Task;

            foreach (var tile in tiles)
            {
                _tiles[tile.name] = tile;
            }
        }

        private async UniTask LoadUnitIconsAsync()
        {
            var iconNames = GetUnits()
                .Select(x => UnitAdressableLoaderHelper.GetUnitIconAddressableName(x.Name, x.Side))
                .Distinct()
                .ToArray();

            if (iconNames.Length == 0) return;

            _unitIconsHandle = Addressables.LoadAssetsAsync<Sprite>(iconNames, null, Addressables.MergeMode.Union);
            var sprites = await _unitIconsHandle.Task;

            foreach (var sprite in sprites)
            {
                _unitIcons[sprite.texture.name] = sprite;
            }
        }

        private async UniTask LoadAnimationControllersAsync()
        {
            var overrideControllerNames = GetUnits()
                .Select(x => UnitAdressableLoaderHelper.GetUnitOverrideAnimationAddressableName(x.Name, x.Side))
                .Distinct()
                .ToArray();

            if (overrideControllerNames.Length == 0) return;

            _animControllersHandle = Addressables.LoadAssetsAsync<AnimatorOverrideController>(overrideControllerNames, null, Addressables.MergeMode.Union);
            var overrideControllers = await _animControllersHandle.Task;

            foreach (var controller in overrideControllers)
            {
                _overrideAnimationControllers[controller.name] = controller;
            }
        }

        private async UniTask LoadPrefabsAsync()
        {
            var prefabNames = new string[]
            {
                PrefabsAdressableResourceNames.ACTION_BUTTON_PREFAB,
                PrefabsAdressableResourceNames.EQUIPMENT_SLOT_PREFAB,
                PrefabsAdressableResourceNames.UNIT_QUEUE_ITEM_PREFAB
            };

            _prefabsHandle = Addressables.LoadAssetsAsync<GameObject>(prefabNames, null, Addressables.MergeMode.Union);
            var prefabs = await _prefabsHandle.Task;

            foreach (var prefab in prefabs)
            {
                _prefabs[prefab.name] = prefab;
            }
        }

        #endregion

        #region regin ToOptimiste

        public Dictionary<int, int> GetEnemyUnitIdCountsPairs(int roomId, int waveOrder)
        {
            var wave = _staticDb.WaveRooms
                .Where(x => x.RoomId == roomId)
                .Where(x => x.Order == waveOrder)
                .Select(x => x.WaveId)
                .First();

            var units = _staticDb
                .WaveEnemies
                .Where(x => x.WaveId == wave)
                .Select(x => new { x.UnitId, x.Count })
                .ToDictionary(x => x.UnitId, x => x.Count);

            return units;
        }

        private UnitEntity[] GetUnitsData(int[] ids)
        {
            return _staticDb.Units.Where(x => ids.Contains(x.Id)).ToArray();
        }

        private static int[] GetUserUnitIds()
        {
            return new[] { 1, 2 };
        }

        private UnitEntity[] GetUnits()
        {
            var enemyids = GetEnemyUnitIdCountsPairs
                (
                    roomId: _gameGlobalStateManager.ActualRoomId,
                    waveOrder: _gameGlobalStateManager.ActualWaveOrder
                )
                .Keys
                .ToArray();

            var userUnitIds = GetUserUnitIds();

            var unitIds = enemyids.Concat(userUnitIds).Distinct().ToArray();

            return GetUnitsData(unitIds);
        }

        #endregion
    }
}