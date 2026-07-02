using Assets.Db.Models;
using SQLite;
using System;

namespace Assets.Db
{
    public class ProgressDb : IDisposable
    {
        private readonly SQLiteConnection _connection;

        public ProgressDb(string connectionString)
        {
            _connection = new SQLiteConnection(connectionString, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex);
        }

        #region Tables
            
        public TableQuery<ProgressDataEntity> ProgressData => _connection.Table<ProgressDataEntity>();

        #endregion

        public void InitTables()
        {
            _connection.CreateTable<ProgressDataEntity>();
        }

        public void InsertOrUpdate<T>(T[] data) where T : ProgressDataEntity
        {
            if (data == null || data.Length == 0)
            {
                return;
            }

            _connection.RunInTransaction(() =>
            {
                foreach (var item in data)
                {
                    _connection.InsertOrReplace(item);
                }
            });
        }

        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}