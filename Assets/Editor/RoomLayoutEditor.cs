using Assets.Scripts.Data;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor
{
    /// <summary>
    /// Кастомный редактор для RoomLayout с визуализацией и инструментами рисования
    /// </summary>
    [CustomEditor(typeof(RoomLayout))]
    public class RoomLayoutEditor : UnityEditor.Editor
    {
        private RoomLayout _layout;
        private bool _showTileGrid = true;
        private bool _showSpawnPoints = true;
        private Vector2 _scrollPos;
        private TileType _selectedBrush = TileType.Wall;

        private void OnEnable()
        {
            _layout = (RoomLayout)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Базовые настройки
            EditorGUILayout.LabelField("Настройки комнаты", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();

            _layout.RoomId = EditorGUILayout.IntField("Room ID (из БД)", _layout.RoomId);
            _layout.Width = EditorGUILayout.IntSlider("Ширина", _layout.Width, 5, 50);
            _layout.Height = EditorGUILayout.IntSlider("Высота", _layout.Height, 5, 50);

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_layout);
            }

            EditorGUILayout.Space(10);

            // Инструменты генерации
            DrawGenerationTools();

            EditorGUILayout.Space(10);

            // Инструменты рисования
            DrawBrushSelector();

            EditorGUILayout.Space(10);

            // Визуализация сетки
            _showTileGrid = EditorGUILayout.Foldout(_showTileGrid, "🎨 Редактор карты", true);
            if (_showTileGrid)
            {
                DrawTileGrid();
            }

            EditorGUILayout.Space(10);

            // Точки спавна
            _showSpawnPoints = EditorGUILayout.Foldout(_showSpawnPoints, "📍 Точки спавна", true);
            if (_showSpawnPoints)
            {
                DrawSpawnPointsEditor();
            }

            EditorGUILayout.Space(10);

            // Визуальные ресурсы
            EditorGUILayout.LabelField("Визуальные ресурсы", EditorStyles.boldLabel);
            _layout.FloorTile = (UnityEngine.Tilemaps.TileBase)EditorGUILayout.ObjectField(
                "Floor Tile", _layout.FloorTile, typeof(UnityEngine.Tilemaps.TileBase), false);
            _layout.WallTile = (UnityEngine.Tilemaps.TileBase)EditorGUILayout.ObjectField(
                "Wall Tile", _layout.WallTile, typeof(UnityEngine.Tilemaps.TileBase), false);
            _layout.WaterTile = (UnityEngine.Tilemaps.TileBase)EditorGUILayout.ObjectField(
                "Water Tile", _layout.WaterTile, typeof(UnityEngine.Tilemaps.TileBase), false);

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(_layout);
            }
        }

        private void DrawGenerationTools()
        {
            EditorGUILayout.LabelField("Инструменты генерации", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("🔧 Пустая комната", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Подтверждение", 
                    "Создать новую пустую комнату? Текущие данные будут удалены.", "Создать", "Отмена"))
                {
                    _layout.InitializeTileMap();
                    EditorUtility.SetDirty(_layout);
                }
            }

            if (GUILayout.Button("🧱 Стены по периметру", GUILayout.Height(30)))
            {
                _layout.AddPerimeterWalls();
                EditorUtility.SetDirty(_layout);
            }

            GUILayout.EndHorizontal();
        }

        private void DrawBrushSelector()
        {
            EditorGUILayout.LabelField("Кисть для рисования", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();

            if (GUILayout.Toggle(_selectedBrush == TileType.Floor, "⬜ Пол", "Button", GUILayout.Height(30)))
                _selectedBrush = TileType.Floor;

            if (GUILayout.Toggle(_selectedBrush == TileType.Wall, "⬛ Стена", "Button", GUILayout.Height(30)))
                _selectedBrush = TileType.Wall;

            if (GUILayout.Toggle(_selectedBrush == TileType.Water, "🌊 Вода", "Button", GUILayout.Height(30)))
                _selectedBrush = TileType.Water;

            if (GUILayout.Toggle(_selectedBrush == TileType.Pit, "🕳️ Яма", "Button", GUILayout.Height(30)))
                _selectedBrush = TileType.Pit;

            GUILayout.EndHorizontal();

            EditorGUILayout.HelpBox($"Выбрана кисть: {GetTileEmoji(_selectedBrush)}", MessageType.Info);
        }

        private void DrawTileGrid()
        {
            if (_layout.Width <= 0 || _layout.Height <= 0)
            {
                EditorGUILayout.HelpBox("Установите размеры комнаты", MessageType.Warning);
                return;
            }

            EditorGUILayout.HelpBox(
                $"Размер: {_layout.Width}x{_layout.Height}\n" +
                $"Кликните на клетку, чтобы нарисовать выбранным тайлом",
                MessageType.Info
            );

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(400));

            float cellSize = 25f;

            GUILayout.BeginVertical();

            // Рисуем сверху вниз (Y от высоты к 0)
            for (int y = _layout.Height - 1; y >= 0; y--)
            {
                GUILayout.BeginHorizontal();

                for (int x = 0; x < _layout.Width; x++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    TileType currentTile = _layout.GetTile(x, y);

                    string emoji = GetTileEmoji(currentTile);
                    Color bgColor = GetTileColor(currentTile);

                    // Проверка на точки спавна
                    if (IsPlayerSpawn(pos))
                    {
                        emoji = "🟦";
                        bgColor = new Color(0.5f, 0.7f, 1f);
                    }
                    else if (IsEnemySpawn(pos))
                    {
                        emoji = "🟥";
                        bgColor = new Color(1f, 0.5f, 0.5f);
                    }
                    else if (IsInteractablePoint(pos))
                    {
                        emoji = "🟨";
                        bgColor = new Color(1f, 1f, 0.5f);
                    }

                    GUI.backgroundColor = bgColor;

                    // Кнопка для каждой клетки
                    if (GUILayout.Button(emoji, GUILayout.Width(cellSize), GUILayout.Height(cellSize)))
                    {
                        _layout.SetTile(x, y, _selectedBrush);
                        EditorUtility.SetDirty(_layout);
                    }
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();

            EditorGUILayout.EndScrollView();
            GUI.backgroundColor = Color.white;
        }

        private void DrawSpawnPointsEditor()
        {
            EditorGUILayout.LabelField("🟦 Точки спавна игрока:", EditorStyles.boldLabel);
            DrawVector2IntArray(ref _layout.PlayerSpawnPoints);

            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField("🟥 Точки спавна врагов:", EditorStyles.boldLabel);
            DrawVector2IntArray(ref _layout.EnemySpawnPoints);

            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField("🟨 Интерактивные объекты:", EditorStyles.boldLabel);
            DrawVector2IntArray(ref _layout.InteractablePoints);
        }

        private void DrawVector2IntArray(ref Vector2Int[] array)
        {
            if (array == null)
                array = new Vector2Int[0];

            int newSize = EditorGUILayout.IntField("Количество", array.Length);
            if (newSize != array.Length)
            {
                System.Array.Resize(ref array, newSize);
                EditorUtility.SetDirty(_layout);
            }

            for (int i = 0; i < array.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"  [{i}]", GUILayout.Width(40));
                array[i] = EditorGUILayout.Vector2IntField("", array[i]);
                EditorGUILayout.EndHorizontal();
            }
        }

        private string GetTileEmoji(TileType tile)
        {
            return tile switch
            {
                TileType.Floor => "⬜",
                TileType.Wall => "⬛",
                TileType.Water => "🌊",
                TileType.Pit => "🕳️",
                _ => "❓"
            };
        }

        private Color GetTileColor(TileType tile)
        {
            return tile switch
            {
                TileType.Floor => Color.white,
                TileType.Wall => new Color(0.3f, 0.3f, 0.3f),
                TileType.Water => new Color(0.5f, 0.7f, 1f),
                TileType.Pit => new Color(0.2f, 0.1f, 0.1f),
                _ => Color.magenta
            };
        }

        private bool IsPlayerSpawn(Vector2Int pos)
        {
            return _layout.PlayerSpawnPoints != null &&
                   System.Array.IndexOf(_layout.PlayerSpawnPoints, pos) >= 0;
        }

        private bool IsEnemySpawn(Vector2Int pos)
        {
            return _layout.EnemySpawnPoints != null &&
                   System.Array.IndexOf(_layout.EnemySpawnPoints, pos) >= 0;
        }

        private bool IsInteractablePoint(Vector2Int pos)
        {
            return _layout.InteractablePoints != null &&
                   System.Array.IndexOf(_layout.InteractablePoints, pos) >= 0;
        }
    }
}
