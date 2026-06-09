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

        bool NeedGenerateLocationInfo();
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

            for (var i = roomIds.Count; i > 0; i--)
            {
                var selectedIndex = UnityEngine.Random.Range(0, i);
                roomOrder.Add(roomIds[selectedIndex]);
                roomIds.RemoveRange(selectedIndex, 1);
            }

            _progressDb.Insert(new ProgressDataEntity[]
            {
                new()
                {
                    Id = 1,
                    Type = Db.Enums.ProgressDataType.CurrentLocation,
                    Value = startedLocation.Type.ToString(),
                },
                new()
                {
                    Id = 2,
                    Type = Db.Enums.ProgressDataType.LocationMaxRoomCount,
                    Value = roomCount.ToString(),
                },
                new()
                {
                    Id = 3,
                    Type = Db.Enums.ProgressDataType.CurrentRoomOrder,
                    Value = StartRoomOrder.ToString(),
                },
                new() 
                {
                    Id = 4,
                    Type = Db.Enums.ProgressDataType.OrderedRoomIds,
                    Value = JsonConvert.SerializeObject(roomOrder),
                },
            });
        }

        public bool NeedGenerateLocationInfo()
        {
            var  progressData = _progressDb.ProgressData
                .Where( x => x.Type == Db.Enums.ProgressDataType.CurrentLocation )
                .ToArray();

            if ( progressData.Any(x => x.Type == Db.Enums.ProgressDataType.CurrentLocation))
            {
                return false;
            }

            return true;
        }

        private LocationEntity GetStartLocation()
        {
            return _staticDb.Locations.Where(x => x.Type == Db.Enums.LocationType.Forest).First();
        }
    }
}
