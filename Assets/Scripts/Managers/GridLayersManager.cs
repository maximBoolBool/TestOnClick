using Assets.Scripts.Enums;
using Assets.Scripts.Services;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;

namespace Assets.Scripts.Managers
{
    public interface IGridLayersManager
    {
        Vector3Int GetRoomCordinateFromGridCordinate(Vector3Int cordinate);
        Vector3Int GetRoomCordinateFromGlobalCordinate(Vector3 cordinate);
        RoomLayerType GetCordinateRoomLayerType(Vector3Int cordinate);        
        UniTask SetRoomVisualAsync(GameObject roomVisual);
        UniTask<bool> TrySetLayerVisualAsync(RoomLayerType layer);
        GameObject? RoomVisual { get; }
        bool HasTileOnLayer(Vector3Int cordinte, RoomLayerType layer);
        TileBase? GetTileOnLayer(Vector3Int cordinate, RoomLayerType layer);
        Vector3Int[] GetCordinatesToIgnoreSpriteRool(RoomLayerType layer);
        RoomLayerType ActualLayer { get; }
    }

    public class GridLayersManager : IGridLayersManager
    {
        private RoomLayerType? _roomMaxLayer = null;
        private RoomLayerType _actualLayer = RoomLayerType.GroundLayer4;
        private GameObject? _currentActiveRoomVisual;
        private readonly Dictionary<string, TilemapRenderer> _cachedLayerRenderers = new();
        private readonly Dictionary<string, Tilemap> _cachedLayerTilemaps = new();
        private HashSet<Vector3Int> _cordinatesToShadow = new();

        public RoomLayerType ActualLayer => _actualLayer;

        public GameObject? RoomVisual => _currentActiveRoomVisual;

        [Inject(Id = Constants.HIGHLIGHT_TILEMAP)]
        private readonly Tilemap _highlightTilemap;

        [Inject]
        private readonly IGridService _gridService;

        [Inject]
        private readonly IAddresableResourceManager _addresableResourceManager;

        //Порядоек важен
        private static readonly RoomLayerType[] _roomLayerTypes = new RoomLayerType[]
        {
            RoomLayerType.GroundLayer1,
            RoomLayerType.GroundLayer2,
            RoomLayerType.GroundLayer3,
            RoomLayerType.GroundLayer4,
        };

        //Вызывать при старте сцены
        public async UniTask SetRoomVisualAsync(GameObject? roomVisual)
        {
            _currentActiveRoomVisual = roomVisual;
            _cachedLayerRenderers.Clear();
            _cachedLayerTilemaps.Clear();

            if (_currentActiveRoomVisual == null)
            {
                _roomMaxLayer = null;
                return;
            }

            var renderers = _currentActiveRoomVisual.GetComponentsInChildren<TilemapRenderer>(true);
            foreach (var renderer in renderers)
            {
                _cachedLayerRenderers[renderer.gameObject.name] = renderer;
            }

            var tileMaps = _currentActiveRoomVisual.GetComponentsInChildren<Tilemap>(true);

            foreach(var tileMap in tileMaps)
            {
                _cachedLayerTilemaps[tileMap.gameObject.name] = tileMap;
            }

            _roomMaxLayer = _currentActiveRoomVisual
                .GetComponentsInChildren<Tilemap>(true)
                .Where(x => x.GetUsedTilesCount() > 0)
                .Select(x => RoomLayerTypeHelper.GetRoomLayerType(x.gameObject.name))
                .OrderByDescending(x => x)
                .First();
            
            await TrySetLayerVisualAsync(_roomMaxLayer.Value);
        }

        public async UniTask<bool> TrySetLayerVisualAsync(RoomLayerType layer)
        {
            if (layer > _roomMaxLayer)
            {
                Debug.LogWarning($"пришла слой {layer} хотя максимальный это {_roomMaxLayer}");
                return false;
            }

            if (layer == _actualLayer)
            {
                Debug.LogWarning("Layer already on");
                return false;
            }

            _actualLayer = layer;

            var visibleLayers = layer switch
            {
                RoomLayerType.GroundLayer4 => new[]
                {
                    RoomLayerType.GroundLayer4,
                    RoomLayerType.CliffLayer4,
                    RoomLayerType.GroundLayer3,
                    RoomLayerType.CliffLayer3,
                    RoomLayerType.GroundLayer2,
                    RoomLayerType.CliffLayer2,
                    RoomLayerType.GroundLayer1,
                    RoomLayerType.CliffLayer1,
                    RoomLayerType.WaterTilemap,
                    RoomLayerType.BaseWaterLayer,
                },
                RoomLayerType.GroundLayer3 => new[]
                {
                    RoomLayerType.GroundLayer3,
                    RoomLayerType.CliffLayer3,
                    RoomLayerType.GroundLayer2,
                    RoomLayerType.CliffLayer2,
                    RoomLayerType.GroundLayer1,
                    RoomLayerType.CliffLayer1,
                    RoomLayerType.WaterTilemap,
                    RoomLayerType.BaseWaterLayer,
                },
                RoomLayerType.GroundLayer2 => new[]
                {
                    RoomLayerType.GroundLayer2,
                    RoomLayerType.CliffLayer2,
                    RoomLayerType.GroundLayer1,
                    RoomLayerType.CliffLayer1,
                    RoomLayerType.WaterTilemap,
                    RoomLayerType.BaseWaterLayer,
                },
                RoomLayerType.GroundLayer1 => new[]
                {
                    RoomLayerType.GroundLayer1,
                    RoomLayerType.CliffLayer1,
                    RoomLayerType.WaterTilemap,
                    RoomLayerType.BaseWaterLayer,
                },
                RoomLayerType.BaseWaterLayer => new[]
                {
                    RoomLayerType.GroundLayer1,
                    RoomLayerType.WaterTilemap,
                    RoomLayerType.BaseWaterLayer,

                },
                _ => System.Array.Empty<RoomLayerType>()
            };

            SetVisualLayers(visibleLayers);
            ClearShadowTiles();
            await SetShadowTilesAsync(layer);
            return true;
        }

        public bool HasTileOnLayer(Vector3Int cordinte, RoomLayerType layer)
        {
            return GetTileOnLayer(cordinte, layer) != null;
        }

        public TileBase? GetTileOnLayer(Vector3Int cordinate, RoomLayerType layer)
        {
            var layerName = layer.GetRoomLayerGridName();
            _cachedLayerTilemaps.TryGetValue(layerName, out Tilemap tilemap);
            var tile = tilemap!.GetTile(cordinate);
            return tile;
        }

        public Vector3Int GetRoomCordinateFromGlobalCordinate(Vector3 cordinate)
        {
            return GetRoomCordinateFromGridCordinate(_gridService.ToGridCordinates(cordinate));
        }

        public Vector3Int GetRoomCordinateFromGridCordinate(Vector3Int cordinate)
        {
            var hightGap = 0;
            var i = 0;

            while (true)
            {
                var nextLayer = _roomLayerTypes[i].GetLayerOver();

                if (nextLayer == null)
                {
                    break;
                }

                var hasTile = HasTileOnLayer(new Vector3Int(cordinate.x, cordinate.y + hightGap, cordinate.z), nextLayer.Value);

                if (!hasTile)
                {
                    break;
                }

                hightGap++;
                i++;
            }

            return new Vector3Int(cordinate.x, cordinate.y + hightGap, cordinate.z);
        }

        public RoomLayerType GetCordinateRoomLayerType(Vector3Int cordinate)
        {
            foreach (var layer in _roomLayerTypes.OrderByDescending(x => x))
            {
                if (HasTileOnLayer(cordinate, layer))
                {
                    return layer;
                }
            }

            return RoomLayerType.GroundLayer1;
        }

        private void SetVisualLayers(RoomLayerType[] visibleLayers)
        {
            if (_currentActiveRoomVisual == null)
            {
                Debug.LogError("Комната не сгенерирована или не установлена в GridLayersManager");
                return;
            }

           var visibleNames = new HashSet<string>(visibleLayers.Select(x => x.GetRoomLayerGridName()));

            foreach (var (layerName, renderer) in _cachedLayerRenderers)
            {
                if (renderer == null) continue;

                bool shouldBeVisible = visibleNames.Contains(layerName);

                renderer.enabled = shouldBeVisible;
            }
        }

        public Vector3Int[] GetCordinatesToIgnoreSpriteRool(RoomLayerType layer)
        {
            var shadowLayers = layer.GetLayerShadowLayers();

            var result = new HashSet<Vector3Int>();

            foreach (var shadowLayer in shadowLayers)
            {
                _cachedLayerTilemaps.TryGetValue(shadowLayer.GetRoomLayerGridName(), out Tilemap tilemap);
                tilemap.CompressBounds();

                foreach (Vector3Int position in tilemap.cellBounds.allPositionsWithin)
                {
                    if (tilemap.HasTile(position))
                    {
                        result.Add(position);
                    }
                }
            }

            if (result.Count == 0)
            {
                return Array.Empty<Vector3Int>();
            }

            return result
                .Where(x => !result.Any(z => z.x == x.x && z.y > x.y))
                .Distinct()
                .ToArray();
        }

        private async UniTask SetShadowTilesAsync(RoomLayerType actualLayer)
        {
            var shadowLayers = actualLayer.GetLayerShadowLayers();

            foreach (var layer in shadowLayers)
            {
                _cachedLayerTilemaps.TryGetValue(layer.GetRoomLayerGridName(), out Tilemap tilemap);
                tilemap.CompressBounds();

                foreach (Vector3Int position in tilemap.cellBounds.allPositionsWithin)
                {
                    if (tilemap.HasTile(position))
                    {
                        _cordinatesToShadow.Add(position);
                    }
                }
            }

            if (!_cordinatesToShadow.Any())
            {
                return;
            }
            // Находим координаты Y тех точек, у которых при том же X нет точки выше (z.y > x.y)
            var cordinatesToIgnor = _cordinatesToShadow
                .Where(x => !_cordinatesToShadow.Any(z => z.x == x.x && z.y > x.y))
                .Distinct()
                .ToArray();

            _cordinatesToShadow = _cordinatesToShadow.Where(x => !cordinatesToIgnor.Contains(x)).ToHashSet();
            // вынести в Manager
            TileBase shadowTile = _addresableResourceManager.GetTileBase("ShadowTile");

            foreach (var cordinate in _cordinatesToShadow)
            {
                _highlightTilemap.SetTile(cordinate, shadowTile);
            }
        }

        private void ClearShadowTiles()
        {
            foreach (var cordinate in _cordinatesToShadow)
            {
                _highlightTilemap.SetTile(cordinate, null);
            }
            _cordinatesToShadow.Clear();
        }
    }
}
