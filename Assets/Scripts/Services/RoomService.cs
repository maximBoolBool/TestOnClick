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
        bool TrySwitchNextRoom(bool withSkip);
    }

    public class RoomService : IRoomService
    {
        [Inject]
        private readonly IRoomLoaderService _roomLoaderService;

        [Inject]
        private readonly ProgressDb _progressDb;

        [Inject]
        private readonly IGameGlobalStateManager _gameGlobalStateManager;

        public bool TrySwitchNextRoom(bool withSkip)
        {
            var progressData = _progressDb.ProgressData
                .Where(x => x.Type == ProgressDataType.OrderedRoomIds || x.Type == ProgressDataType.CurrentRoomOrder)
                .ToArray();

            var roomIds = JsonConvert.DeserializeObject<int[]>(progressData.First(x => x.Type == ProgressDataType.OrderedRoomIds).Value);
            var currentRoomOrderData = progressData.FirstOrDefault(x => x.Type == ProgressDataType.CurrentRoomOrder);

            var newRoomOrder = currentRoomOrderData != null 
                ? withSkip
                    ? int.Parse(currentRoomOrderData.Value) + 1 
                    : int.Parse(currentRoomOrderData.Value)
                : 0;

            if (newRoomOrder > roomIds.Length - 1)
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

            if (newRoomOrder != 0)
            {
                _roomLoaderService.ClearRoom(AdvancedRoomLoader.LoadRoomSync($"Room{roomIds[newRoomOrder - 1]}"));
            }

            _roomLoaderService.LoadRoom(AdvancedRoomLoader.LoadRoomSync($"Room{currentRoomId}"));
            _gameGlobalStateManager.ActualRoomId = currentRoomId;
            _gameGlobalStateManager.ActualWaveId = 1;            

            return true;
        }
    }
}
