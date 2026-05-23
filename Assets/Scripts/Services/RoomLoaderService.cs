using Assets.Scripts.Data;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;

namespace Assets.Scripts.Services
{
    /// <summary>
    /// Сервис для загрузки и отображения комнат на сцене
    /// </summary>
    public interface IRoomLoaderService
    {
        void LoadRoom(RoomLayout layout, Vector3Int offset = default);
        void ClearRoom(RoomLayout layout, Vector3Int offset = default);
    }

    public class RoomLoaderService : IRoomLoaderService
    {
        [Inject(Id = Constants.GroundTilemap)]
        private readonly Tilemap _groundTilemap;

        [Inject(Id = Constants.HighlightTilemap)]
        private readonly Tilemap _highlightTilemap;

        /// <summary>
        /// Загрузить комнату на Tilemap
        /// </summary>
        /// <param name="layout">ScriptableObject с картой комнаты</param>
        /// <param name="offset">Смещение комнаты (по умолчанию 0,0,0)</param>
        public void LoadRoom(RoomLayout layout, Vector3Int offset = default)
        {
            if (layout == null)
            {
                Debug.LogError("[RoomLoader] Layout is null!");
                return;
            }

            Debug.Log($"[RoomLoader] Загрузка комнаты (RoomId: {layout.RoomId}, Size: {layout.Width}x{layout.Height})");

            // Получаем карту тайлов
            var tileMap = layout.GetTileMap2D();

            // Отрисовываем каждый тайл
            for (int x = 0; x < layout.Width; x++)
            {
                for (int y = 0; y < layout.Height; y++)
                {
                    Vector3Int position = new Vector3Int(x, y, 0) + offset;
                    TileType tileType = tileMap[x, y];

                    // Выбираем тайл в зависимости от типа
                    TileBase tile = GetTileForType(layout, tileType);

                    if (tile != null)
                    {
                        // Стены рисуем на слое Highlight, пол - на Ground
                        if (tileType == TileType.Wall)
                        {
                            _highlightTilemap?.SetTile(position, tile);
                        }
                        else
                        {
                            _groundTilemap.SetTile(position, tile);
                        }
                    }
                }
            }

            // Визуализируем точки спавна (опционально)
            DrawSpawnPoints(layout, offset);

            Debug.Log($"[RoomLoader] Комната загружена успешно!");
        }

        /// <summary>
        /// Очистить комнату с карты
        /// </summary>
        public void ClearRoom(RoomLayout layout, Vector3Int offset = default)
        {
            if (layout == null) return;

            for (int x = 0; x < layout.Width; x++)
            {
                for (int y = 0; y < layout.Height; y++)
                {
                    Vector3Int position = new Vector3Int(x, y, 0) + offset;
                    _groundTilemap.SetTile(position, null);
                    _highlightTilemap?.SetTile(position, null);
                }
            }

            Debug.Log($"[RoomLoader] Комната очищена");
        }

        /// <summary>
        /// Получить TileBase для типа тайла
        /// </summary>
        private TileBase GetTileForType(RoomLayout layout, TileType tileType)
        {
            return tileType switch
            {
                TileType.Floor => layout.FloorTile,
                TileType.Wall => layout.WallTile,
                TileType.Water => layout.WaterTile,
                TileType.Pit => layout.FloorTile, // Яма использует пол как базу
                _ => null
            };
        }

        /// <summary>
        /// Визуализировать точки спавна (для отладки)
        /// </summary>
        private void DrawSpawnPoints(RoomLayout layout, Vector3Int offset)
        {
            // Рисуем гизмо для точек спавна (видны только в редакторе)
            if (layout.PlayerSpawnPoints != null)
            {
                foreach (var point in layout.PlayerSpawnPoints)
                {
                    Vector3Int pos = new Vector3Int(point.x, point.y, 0) + offset;
                    Debug.Log($"[RoomLoader] 🟦 Player spawn at: {pos}");
                }
            }

            if (layout.EnemySpawnPoints != null)
            {
                foreach (var point in layout.EnemySpawnPoints)
                {
                    Vector3Int pos = new Vector3Int(point.x, point.y, 0) + offset;
                    Debug.Log($"[RoomLoader] 🟥 Enemy spawn at: {pos}");
                }
            }
        }
    }
}
