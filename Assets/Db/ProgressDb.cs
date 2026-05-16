using SQLite;
using System;

namespace Assets.Db
{
    class ProgressDb : IDisposable
{
    private SQLiteConnection _connection;

    public ProgressDb()
    {
        _connection = new SQLiteConnection(GetDataBasePath());
    }

    public string GetDataBasePath()
    {
        const string dbName = "progress.db";
        string path = System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, dbName);
        return path;
    }

    public void Dispose()
    {
        _connection?.Close();
        _connection?.Dispose();
    }
    }
}