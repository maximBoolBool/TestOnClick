using Assets.Db;
using Assets.Db.Enums;
using Assets.Db.Models;
using Assets.Scripts.Managers;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;
using Zenject;

namespace Assets.Scripts.Services
{
    public interface IRoomService
    {
        UniTask<bool> TrySwitchNextRoom(bool withSkip);
    }

    public class RoomService : IRoomService
    {
        [Inject]
        private readonly IRoomLoaderService _roomLoaderService;

        [Inject]
        private readonly ProgressDb _progressDb;

        [Inject]
        private readonly IGameGlobalStateManager _gameGlobalStateManager;

        public async UniTask<bool> TrySwitchNextRoom(bool withSkip)
        {
            var progressData = _progressDb.ProgressData
                .Where(x => x.Type == ProgressDataType.OrderedRoomIds || x.Type == ProgressDataType.CurrentRoomOrder)
                .ToArray();

            var orderedRoomIds = JsonConvert.DeserializeObject<int[]>(progressData.First(x => x.Type == ProgressDataType.OrderedRoomIds).Value);
            var currentRoomOrderData = progressData.FirstOrDefault(x => x.Type == ProgressDataType.CurrentRoomOrder);

            var newRoomOrder = currentRoomOrderData != null 
                ? withSkip
                    ? int.Parse(currentRoomOrderData.Value) + 1 
                    : int.Parse(currentRoomOrderData.Value)
                : 0;

            if (newRoomOrder > orderedRoomIds.Length - 1)
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

            var currentRoomId = orderedRoomIds[newRoomOrder];
 
            //PRT-9
            await _roomLoaderService.NewLoadRoomAsync("Room_1_1");
            //_roomLoaderService.LoadRoom(AdvancedRoomLoader.LoadRoomSync($"Room{currentRoomId}"));
            _gameGlobalStateManager.ActualRoomId = currentRoomId;
            _gameGlobalStateManager.ActualWaveOrder = 1;            

            return true;
        }
    }
}
