using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Data
{
    /// <summary>
    /// ScriptableObject для хранения визуальной карты комнаты.
    /// Создается через: Assets → Create → Game Data → Room Layout
    /// </summary>
    [CreateAssetMenu(fileName = "RoomLayout_", menuName = "Game Data/Room Layout", order = 1)]
    public class RoomLayout : ScriptableObject
    {
        [Header("Связь с базой данных")]
        [Tooltip("ID комнаты из таблицы 'rooms' в БД")]
        public int RoomId;

        [Header("Размеры")]
        [Tooltip("Ширина комнаты в тайлах")]
        [Range(5, 50)]
        public int Width = 10;

        [Tooltip("Высота комнаты в тайлах")]
        [Range(5, 50)]
        public int Height = 10;

        [Header("Карта тайлов")]
        [Tooltip("Визуальная карта комнаты (редактируется в Inspector)")]
        [SerializeField]
        private TileType[] _tileMap;

        [Header("Визуальные ресурсы")]
        [Tooltip("Tile для пола")]
        public TileBase FloorTile;

        [Tooltip("Tile для стен")]
        public TileBase WallTile;

        [Tooltip("Tile для воды")]
        public TileBase WaterTile;

        [Header("Точки спавна")]
        [Tooltip("Позиции спавна игрока (относительно комнаты)")]
        public Vector2Int[] PlayerSpawnPoints;

        [Tooltip("Позиции спавна врагов (связь с БД через индекс)")]
        public Vector2Int[] EnemySpawnPoints;

        [Tooltip("Позиции интерактивных объектов (сундуки, ловушки)")]
        public Vector2Int[] InteractablePoints;

        /// <summary>
        /// Получить тайл по координатам
        /// </summary>
        public TileType GetTile(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return TileType.Wall;

            if (_tileMap == null || _tileMap.Length != Width * Height)
                return TileType.Floor;

            int index = y * Width + x;
            return _tileMap[index];
        }

        /// <summary>
        /// Установить тайл по координатам
        /// </summary>
        public void SetTile(int x, int y, TileType tileType)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return;

            if (_tileMap == null || _tileMap.Length != Width * Height)
                InitializeTileMap();

            int index = y * Width + x;
            _tileMap[index] = tileType;
        }

        /// <summary>
        /// Получить всю карту как 2D массив
        /// </summary>
        public TileType[,] GetTileMap2D()
        {
            if (_tileMap == null || _tileMap.Length != Width * Height)
                InitializeTileMap();

            var result = new TileType[Width, Height];
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int index = y * Width + x;
                    result[x, y] = _tileMap[index];
                }
            }
            return result;
        }

        /// <summary>
        /// Инициализировать пустую карту
        /// </summary>
        public void InitializeTileMap()
        {
            _tileMap = new TileType[Width * Height];
            for (int i = 0; i < _tileMap.Length; i++)
            {
                _tileMap[i] = TileType.Floor;
            }
        }

        /// <summary>
        /// Заполнить карту стенами по периметру
        /// </summary>
        public void AddPerimeterWalls()
        {
            if (_tileMap == null || _tileMap.Length != Width * Height)
                InitializeTileMap();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (x == 0 || x == Width - 1 || y == 0 || y == Height - 1)
                    {
                        SetTile(x, y, TileType.Wall);
                    }
                }
            }
        }

        /// <summary>
        /// Валидация при изменении в Inspector
        /// </summary>
        private void OnValidate()
        {
            // Пересоздать массив если размер изменился
            if (_tileMap != null && _tileMap.Length != Width * Height)
            {
                var oldMap = _tileMap;
                _tileMap = new TileType[Width * Height];

                // Копируем старые данные
                for (int i = 0; i < Mathf.Min(oldMap.Length, _tileMap.Length); i++)
                {
                    _tileMap[i] = oldMap[i];
                }
            }
        }
    }

    /// <summary>
    /// Тип тайла
    /// </summary>
    public enum TileType
    {
        Floor = 0,   // Пол (проходимый)
        Wall = 1,    // Стена (непроходимая)
        Water = 2,   // Вода (непроходимая)
        Pit = 3      // Яма (непроходимая)
    }
}
