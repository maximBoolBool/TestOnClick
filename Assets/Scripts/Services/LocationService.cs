using Assets.Db;
using Assets.Db.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Zenject;

namespace Assets.Scripts.Services
{
    public interface ILocationService
    {
        void WriteLocationInfo();
    }

    public class LocationService : ILocationService
    {
        private const int StartRoomOrder = 0;

        [Inject]
        private readonly StaticDb _staticDb;

        [Inject]
        private readonly ProgressDb _progressDb;

        public void WriteLocationInfo()
        {
            var startedLocation = GetStartLocation();

            var roomCount = UnityEngine.Random.Range(startedLocation.MinRoomCount, startedLocation.MaxRoomCount);

            var roomIds = _staticDb.Rooms
                .Where(x => x.LocationType == startedLocation.Type)
                .Select(x => x.Id)
                .ToList();

            var roomOrder = new HashSet<int>();

            for (var i = roomIds.Count - 1; i > 0; i--)
            {
                var selectedIndex = UnityEngine.Random.Range(0, i + 1);
                roomOrder.Add(roomIds[selectedIndex]);
            }

            _progressDb.Insert(new ProgressDataEntity[]
            {
                new()
                {
                    Type = Db.Enums.ProgressDataType.LastLocation,
                    Value = startedLocation.Type.ToString(),
                },
                new()
                {
                    Type = Db.Enums.ProgressDataType.LocationMaxRoomCount,
                    Value = roomCount.ToString(),
                },
                new()
                {
                    Type = Db.Enums.ProgressDataType.CurrentRoomOrder,
                    Value = StartRoomOrder.ToString(),
                },
                new() 
                {
                    Type = Db.Enums.ProgressDataType.OrderedRoomIds,
                    Value = JsonConvert.SerializeObject(roomOrder),
                },
            });
        }

        private LocationEntity GetStartLocation()
        {
            return _staticDb.Locations.Where(x => x.Type == Db.Enums.LocationType.Desert).First();
        }
    }
}
