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
            _connection = new SQLiteConnection(connectionString);
        }

        #region Tables
            
        public TableQuery<ProgressDataEntity> ProgressData => _connection.Table<ProgressDataEntity>();

        #endregion

        public void InitTables()
        {
            _connection.CreateTable<ProgressDataEntity>();
        }

        public void Insert<T>(T[] data) where T : ProgressDataEntity
        {
            _connection.InsertOrReplace(data);
        }

        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}