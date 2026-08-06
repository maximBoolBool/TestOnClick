using Assets.Scripts.Enums;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Managers
{
    public interface IGridLayersManager
    {
        public void SetRoomVisual(GameObject roomVisual);
        public void SetLayerVisual(RoomLayerType layer);
        GameObject? RoomVisual { get; }
        void LayerUp();
        void LayerDown();
        bool HasTileOnLayer(Vector3Int cordinte, RoomLayerType layer);
        TileBase? GetTileOnLayer(Vector3Int cordinate, RoomLayerType layer);
    }

    public class GridLayersManager : IGridLayersManager
    {
        private RoomLayerType? _roomMaxLayer = null;
        private RoomLayerType _actualLastLayer = RoomLayerType.GroundLayer4;
        private GameObject? _currentActiveRoomVisual;
        private readonly Dictionary<string, TilemapRenderer> _cachedLayerRenderers = new();
        private readonly Dictionary<string, Tilemap> _cachedLayerTilemaps = new();

        public GameObject? RoomVisual => _currentActiveRoomVisual;

        //Вызывать при старте сцены
        public void SetRoomVisual(GameObject? roomVisual)
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
            
            SetLayerVisual(_roomMaxLayer.Value);
        }

        public void SetLayerVisual(RoomLayerType layer)
        {
            if (layer > _roomMaxLayer)
            {
                Debug.LogWarning($"пришла слой {layer} хотя максимальный это {_roomMaxLayer}");
                return;
            }

            if (layer == _actualLastLayer)
            {
                Debug.LogWarning("Layer already on");
                return;
            }

            _actualLastLayer = layer;

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

        public void LayerUp()
        {
            var newLayer = _actualLastLayer switch
            {
                RoomLayerType.GroundLayer4 => RoomLayerType.GroundLayer4,
                RoomLayerType.GroundLayer3 => RoomLayerType.GroundLayer4,
                RoomLayerType.GroundLayer2 => RoomLayerType.GroundLayer3,
                RoomLayerType.GroundLayer1 => RoomLayerType.GroundLayer2,
                RoomLayerType.BaseWaterLayer => RoomLayerType.GroundLayer2
            }; 

            SetLayerVisual(newLayer);
        }

        public void LayerDown()
        {
            var newLayer = _actualLastLayer switch
            {
                RoomLayerType.GroundLayer4 => RoomLayerType.GroundLayer3,
                RoomLayerType.GroundLayer3 => RoomLayerType.GroundLayer2,
                RoomLayerType.GroundLayer2 => RoomLayerType.GroundLayer1,
                RoomLayerType.GroundLayer1 => RoomLayerType.GroundLayer1,
                RoomLayerType.BaseWaterLayer => RoomLayerType.GroundLayer1
            };

            SetLayerVisual(newLayer);

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
    }
}
