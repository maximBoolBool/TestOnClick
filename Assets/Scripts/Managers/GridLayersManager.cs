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
    }

    public class GridLayersManager : IGridLayersManager
    {
        private RoomLayerType _maxVisualLayerType = RoomLayerType.GroundLayer4;
        private GameObject? _currentActiveRoomVisual;
        private readonly Dictionary<string, TilemapRenderer> _cachedLayerRenderers = new();

        public GameObject? RoomVisual => _currentActiveRoomVisual;

        public void SetRoomVisual(GameObject? roomVisual)
        {
            _currentActiveRoomVisual = roomVisual;
            _cachedLayerRenderers.Clear();

            if (_currentActiveRoomVisual == null) return;

            var renderers = _currentActiveRoomVisual.GetComponentsInChildren<TilemapRenderer>(true);
            foreach (var renderer in renderers)
            {
                _cachedLayerRenderers[renderer.gameObject.name] = renderer;
            }

            SetLayerVisual(_maxVisualLayerType);
        }

        public void SetLayerVisual(RoomLayerType layer)
        {
            if (layer == _maxVisualLayerType)
            {
                Debug.LogWarning("Layer already on");
                return;
            }

            _maxVisualLayerType = layer;

            var visibleLayers = layer switch
            {
                RoomLayerType.GroundLayer4 => new[]
                {
                    RoomLayerType.GroundLayer4,
                    RoomLayerType.GroundLayer3,
                    RoomLayerType.GroundLayer2,
                    RoomLayerType.GroundLayer1,
                    RoomLayerType.WaterTilemap,
                    RoomLayerType.BaseWaterLayer,
                },
                RoomLayerType.GroundLayer3 => new[]
                {
                    RoomLayerType.GroundLayer3,
                    RoomLayerType.GroundLayer2,
                    RoomLayerType.GroundLayer1,
                    RoomLayerType.WaterTilemap,
                    RoomLayerType.BaseWaterLayer,
                },
                RoomLayerType.GroundLayer2 => new[]
                {
                    RoomLayerType.GroundLayer2,
                    RoomLayerType.GroundLayer1,
                    RoomLayerType.WaterTilemap,
                    RoomLayerType.BaseWaterLayer,
                },
                RoomLayerType.GroundLayer1 => new[]
                {
                    RoomLayerType.GroundLayer1,
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
            var newLayer = _maxVisualLayerType switch
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
            var newLayer = _maxVisualLayerType switch
            {
                RoomLayerType.GroundLayer4 => RoomLayerType.GroundLayer3,
                RoomLayerType.GroundLayer3 => RoomLayerType.GroundLayer2,
                RoomLayerType.GroundLayer2 => RoomLayerType.GroundLayer1,
                RoomLayerType.GroundLayer1 => RoomLayerType.GroundLayer1,
                RoomLayerType.BaseWaterLayer => RoomLayerType.GroundLayer1
            };

            SetLayerVisual(newLayer);

        }
    }
}
