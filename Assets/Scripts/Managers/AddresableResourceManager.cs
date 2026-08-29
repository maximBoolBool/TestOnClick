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
        public const string WRONG_TILE_NAME = "WrongTile";
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
        private readonly List<AsyncOperationHandle> _tilesHandles = new();
        private readonly List<AsyncOperationHandle> _unitIconsHandles = new();
        private readonly List<AsyncOperationHandle> _animControllersHandles = new();
        private readonly List<AsyncOperationHandle> _prefabsHandles = new();
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
            foreach (var handle in _tilesHandles)
            {
                if (handle.IsValid()) Addressables.Release(handle);
            }
            _tilesHandles.Clear();

            foreach (var handle in _prefabsHandles)
            {
                if (handle.IsValid()) Addressables.Release(handle);
            }
            _prefabsHandles.Clear();

            _prefabs.Clear();
            _tiles.Clear();
        }

        public async UniTask FreeLevelResourceAsync()
        {
            foreach (var handle in _unitIconsHandles)
            {
                if (handle.IsValid()) Addressables.Release(handle);
            }
            _unitIconsHandles.Clear();

            foreach (var handle in _animControllersHandles)
            {
                if (handle.IsValid()) Addressables.Release(handle);
            }
            _animControllersHandles.Clear();

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

            var tasks = tileNames.Select(name =>
            {
                var handle = Addressables.LoadAssetAsync<TileBase>(name);
                _tilesHandles.Add(handle);
                return handle.ToUniTask();
            });

            var tiles = await UniTask.WhenAll(tasks);

            foreach (var tile in tiles)
            {
                if (tile != null)
                {
                    _tiles[tile.name] = tile;
                }
            }
        }

        private async UniTask LoadUnitIconsAsync()
        {
            var iconNames = GetUnits()
                .Select(x => UnitAdressableLoaderHelper.GetUnitIconAddressableName(x.Name, x.Side))
                .Distinct()
                .ToArray();

            if (iconNames.Length == 0) return;

            var tasks = iconNames.Select(name =>
            {
                var handle = Addressables.LoadAssetAsync<Sprite>(name);
                _unitIconsHandles.Add(handle);
                return handle.ToUniTask();
            });

            var sprites = await UniTask.WhenAll(tasks);

            foreach (var sprite in sprites)
            {
                if (sprite != null)
                {
                    _unitIcons[sprite.texture.name] = sprite;
                }
            }
        }

        private async UniTask LoadAnimationControllersAsync()
        {
            var overrideControllerNames = GetUnits()
                .Select(x => UnitAdressableLoaderHelper.GetUnitOverrideAnimationAddressableName(x.Name, x.Side))
                .Distinct()
                .ToArray();

            if (overrideControllerNames.Length == 0) return;

            var tasks = overrideControllerNames.Select(name =>
            {
                var handle = Addressables.LoadAssetAsync<AnimatorOverrideController>(name);
                _animControllersHandles.Add(handle);
                return handle.ToUniTask();
            });

            var overrideControllers = await UniTask.WhenAll(tasks);

            foreach (var controller in overrideControllers)
            {
                if (controller != null)
                {
                    _overrideAnimationControllers[controller.name] = controller;
                }
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

            var tasks = prefabNames.Select(name =>
            {
                var handle = Addressables.LoadAssetAsync<GameObject>(name);
                _prefabsHandles.Add(handle);
                return handle.ToUniTask();
            });

            var prefabs = await UniTask.WhenAll(tasks);

            foreach (var prefab in prefabs)
            {
                if (prefab != null)
                {
                    _prefabs[prefab.name] = prefab;
                }
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