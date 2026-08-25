using Assets.Scripts.Data;
using Assets.Scripts.Managers;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
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
        UniTask NewLoadRoomAsync(string roomKey);
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
                    _highlightTilemap.SetTile(position, null);
                }
            }

            Debug.Log($"[RoomLoader] Комната очищена");
        }

        public async UniTask NewLoadRoomAsync(string roomKey)
        {
            UnloadCurrentRoom();

            GameObject roomInstance = await Addressables.InstantiateAsync(roomKey);

            roomInstance.transform.position = Vector3.zero;
            roomInstance.transform.SetParent(_grid.transform);

            // Передаем скомпилированный объект далее
            await _gridLayersManager.SetRoomVisualAsync(roomInstance);
            _gameGlobalStateManager.InitignoreCordinates();
        }

        private void UnloadCurrentRoom()
        {
            if (_gridLayersManager.RoomVisual != null)
            {
                // Очень важно! Addressables требует освобождать память именно через этот метод
                Addressables.ReleaseInstance(_gridLayersManager.RoomVisual);
                _gridLayersManager.SetRoomVisualAsync(null);
            }
        }

    }
}
