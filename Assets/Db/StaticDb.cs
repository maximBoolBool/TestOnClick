using SQLite;
using System;
using Assets.Db.Models;

namespace Assets.Db
{
    public class StaticDb : IDisposable
    {
        private readonly SQLiteConnection _connection;

        public StaticDb(string dbPath)
        {
            _connection = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadOnly | SQLiteOpenFlags.FullMutex);
        }

        #region Tables
        public TableQuery<UnitEntity> Units => _connection.Table<UnitEntity>();
        public TableQuery<WaveEntity> Waves => _connection.Table<WaveEntity>();
        public TableQuery<RoomEntity> Rooms => _connection.Table<RoomEntity>();
        public TableQuery<WaveEnemiesEntity> WaveEnemies => _connection.Table<WaveEnemiesEntity>();
        public TableQuery<WaveRoomEntity> WaveRooms => _connection.Table<WaveRoomEntity>();
        public TableQuery<LocationEntity> Locations => _connection.Table<LocationEntity>();
        public TableQuery<SkillEntity> Skills => _connection.Table<SkillEntity>();
        public TableQuery<UnitSkillEntity> UnitSkills => _connection.Table<UnitSkillEntity>();
        public TableQuery<SkillStepEntity> SkillSteps => _connection.Table<SkillStepEntity>();
        public TableQuery<StepEntity> Steps => _connection.Table<StepEntity>();
        #endregion

        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}