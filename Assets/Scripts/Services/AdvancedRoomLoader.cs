using Assets.Scripts.Data;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Assets.Scripts.Services
{
    class AdvancedRoomLoader
    {
        // Асинхронная загрузка, чтобы игра не зависала во время чтения файла
        public static RoomLayout LoadRoomSync(string roomKey)
        {
            // Вызываем асинхронный метод, но заставляем игру остановиться и дождаться результата
            RoomLayout room = Addressables.LoadAssetAsync<RoomLayout>(roomKey).WaitForCompletion();

            if (room == null)
            {
                Debug.LogError($"Не удалось синхронно загрузить комнату: {roomKey}");
            }

            return room;
        }
    }
}