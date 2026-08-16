using Assets.Scripts.Data;
using Assets.Scripts.Services;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Behaviours
{
    /// <summary>
    /// Компонент для тестовой загрузки комнат на сцену.
    /// Прикрепи к любому GameObject и назначь RoomLayout в Inspector.
    /// </summary>
    public class RoomLoaderBehaviour : MonoBehaviour
    {
        [Header("Настройки")]
        [Tooltip("ScriptableObject с картой комнаты")]
        [SerializeField]
        private RoomLayout _roomLayout;

        [Tooltip("Смещение комнаты на сцене")]
        [SerializeField]
        private Vector3Int _offset = Vector3Int.zero;

        [Tooltip("Загрузить комнату при старте")]
        [SerializeField]
        private bool _loadOnStart = true;

        [Inject]
        private IRoomLoaderService _roomLoaderService;

        private void Start()
        {
            if (_loadOnStart && _roomLayout != null)
            {
                LoadRoom();
            }
        }

        /// <summary>
        /// Загрузить комнату (можно вызвать из Inspector или кода)
        /// </summary>
        [ContextMenu("Load Room")]
        public void LoadRoom()
        {
            if (_roomLayout == null)
            {
                Debug.LogError("[RoomLoaderBehaviour] RoomLayout не назначен!");
                return;
            }

            if (_roomLoaderService == null)
            {
                Debug.LogError("[RoomLoaderBehaviour] RoomLoaderService не инжектирован! Проверь Zenject.");
                return;
            }

            //PRT-9 убрать хардкод
            _roomLoaderService.NewLoadRoomAsync("Room_1_1");

            //_roomLoader.LoadRoom(_roomLayout, _offset);
            Debug.Log($"[RoomLoaderBehaviour] Загружена комната: {_roomLayout.name}");
        }

        /// <summary>
        /// Очистить комнату (можно вызвать из Inspector)
        /// </summary>
        [ContextMenu("Clear Room")]
        public void ClearRoom()
        {
            if (_roomLayout == null || _roomLoaderService == null)
                return;

            _roomLoaderService.ClearRoom(_roomLayout, _offset);
            Debug.Log($"[RoomLoaderBehaviour] Комната очищена: {_roomLayout.name}");
        }

        /// <summary>
        /// Визуализация в редакторе
        /// </summary>
        private void OnDrawGizmos()
        {
            if (_roomLayout == null) return;

            // Рисуем границы комнаты
            Vector3 center = new Vector3(
                _offset.x + _roomLayout.Width / 2f,
                _offset.y + _roomLayout.Height / 2f,
                0
            );
            Vector3 size = new Vector3(_roomLayout.Width, _roomLayout.Height, 0);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(center, size);

            // Рисуем точки спавна
            if (_roomLayout.PlayerSpawnPoints != null)
            {
                Gizmos.color = Color.blue;
                foreach (var point in _roomLayout.PlayerSpawnPoints)
                {
                    Vector3 pos = new Vector3(point.x + _offset.x, point.y + _offset.y, 0);
                    Gizmos.DrawSphere(pos, 0.3f);
                }
            }

            if (_roomLayout.EnemySpawnPoints != null)
            {
                Gizmos.color = Color.red;
                foreach (var point in _roomLayout.EnemySpawnPoints)
                {
                    Vector3 pos = new Vector3(point.x + _offset.x, point.y + _offset.y, 0);
                    Gizmos.DrawSphere(pos, 0.3f);
                }
            }
        }
    }
}
