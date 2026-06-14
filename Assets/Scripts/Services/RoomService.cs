using Assets.Db;
using Assets.Db.Enums;
using Assets.Db.Models;
using Assets.Scripts.Managers;
using Newtonsoft.Json;
using System.Linq;
using Zenject;

namespace Assets.Scripts.Services
{
    public interface IRoomService
    {
        bool TrySwitchNextRoom();
    }

    public class RoomService : IRoomService
    {
        [Inject]
        private readonly IRoomLoaderService _roomLoaderService;

        [Inject]
        private readonly ProgressDb _progressDb;

        [Inject]
        private readonly IGameGlobalStateManager _gameGlobalStateManager;

        public bool TrySwitchNextRoom()
        {
            var progressData = _progressDb.ProgressData
                .Where(x => x.Type == ProgressDataType.OrderedRoomIds || x.Type == ProgressDataType.CurrentRoomOrder)
                .ToArray();

            var roomIds = JsonConvert.DeserializeObject<int[]>(progressData.First(x => x.Type == ProgressDataType.OrderedRoomIds).Value);
            var currentRoomOrderData = progressData.First(x => x.Type == ProgressDataType.CurrentRoomOrder).Value;

            var newRoomOrder = currentRoomOrderData != null ? int.Parse(currentRoomOrderData) + 1 : 0;

            if (newRoomOrder > roomIds.Length)
            {
                return false;
            }

            _progressDb.InsertOrUpdate(new ProgressDataEntity[]
            {
                new()
                {
                    Id = 4,
                    Type = ProgressDataType.CurrentRoomOrder,
                    Value = newRoomOrder.ToString()
                }
            });

            var currentRoomId = roomIds[newRoomOrder];

            _roomLoaderService.LoadRoom(AdvancedRoomLoader.LoadRoomSync($"Room{currentRoomId}"));
            _gameGlobalStateManager.ActualRoomId = currentRoomId;
            _gameGlobalStateManager.ActualWaveId = 1;            

            return true;
        }
    }
}
