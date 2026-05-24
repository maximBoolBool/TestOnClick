using Assets.Db;
using System.Collections.Generic;
using System.Linq;
using Zenject;

namespace Assets.Scripts.Services
{
    public interface IRoomSpawnService
    {
        /// <summary>
        /// Returns a dictionary where the key is the unit ID and the value is the count of units of that type to spawn
        /// </summary>
        /// <param name="roomId">The room identifier</param>
        /// <param name="waveOrder">The wave order</param>
        /// <returns>Dictionary with unit IDs as keys and spawn counts as values</returns>
        public Dictionary<int, int> GetEnemyUnitIdCountsPairs(int roomId, int waveOrder);
    }

    public class RoomSpawnService : IRoomSpawnService
    {
        [Inject]
        private readonly StaticDb _staticDb;

        public Dictionary<int, int> GetEnemyUnitIdCountsPairs(int roomId, int waveOrder)
        {
            var wave = _staticDb.WaveRooms
                .Where(x => x.RoomId == roomId)
                .Where(x => x.Order == waveOrder)
                .Select(x => x.WaveId)
                .First();

            var units = _staticDb
                .WaveEnemies
                .Where(x => x.WaveId == wave)
                .Select(x => new { x.UnitId, x.Count })
                .ToDictionary(x => x.UnitId, x => x.Count);

            return units;
        }
    }
}