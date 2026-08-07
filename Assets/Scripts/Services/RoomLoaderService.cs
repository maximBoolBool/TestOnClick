using Assets.Scripts.Data;
using Assets.Scripts.Managers;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Tilemaps;
using Zenject;

namespace Assets.Scripts.Services
{
    /// <summary>
    /// Сервис для загрузки и отображения комнат на сцене
    /// </summary>
    public interface IRoomLoaderService
    {
        void ClearRoom(RoomLayout layout, Vector3Int offset = default);
        void NewLoadRoom(string roomKey);
    }

    public class RoomLoaderService : IRoomLoaderService
    {
        [Inject]
        private readonly IGridLayersManager _gridLayersManager;
       
        [Inject(Id = Constants.HighlightTilemap)]
        private readonly Tilemap _highlightTilemap;

        [Inject(Id = Constants.Grid)]
        private readonly Grid _grid;

        [Inject]
        private readonly IGameGlobalStateManager _gameGlobalStateManager;

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
                    _highlightTilemap?.SetTile(position, null);
                }
            }

            Debug.Log($"[RoomLoader] Комната очищена");
        }

        public void NewLoadRoom(string roomKey)
        {
            UnloadCurrentRoom();

            // Запускаем асинхронную операцию
            var handle = Addressables.InstantiateAsync(roomKey);

            // Блокируем главный поток до завершения загрузки и инстанцирования
            GameObject roomInstance = handle.WaitForCompletion();

            // Вызываем обработчик вручную, передавая уже завершенный хэндл
            OnRoomVisualLoaded(handle);
        }

        private void OnRoomVisualLoaded(AsyncOperationHandle<GameObject> handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                handle.Result.transform.position = Vector3.zero; // Сбрасываем позицию в 0,0,0
                handle.Result.transform.SetParent(_grid.transform);
                _gridLayersManager.SetRoomVisual(handle.Result);
                _gameGlobalStateManager.InitignoreCordinates();
            }
            else
            {
                Debug.LogError("Не удалось загрузить визуал комнаты через Addressables!");
            }
        }

        private void UnloadCurrentRoom()
        {
            if (_gridLayersManager.RoomVisual != null)
            {
                // Очень важно! Addressables требует освобождать память именно через этот метод
                Addressables.ReleaseInstance(_gridLayersManager.RoomVisual);
                _gridLayersManager.SetRoomVisual(null);
            }
        }

    }
}
