using SQLite;
using System;
using System.IO;
using UnityEngine;

public class StaticDb : IDisposable
{
    private SQLiteConnection _connection;

    public StaticDb()
    {
        _connection = new SQLiteConnection(GetDataBasePath());
    }

    private string GetDataBasePath()
    {
        const string dbName = "game_data.db";
        string persistentPath = Path.Combine(Application.persistentDataPath, dbName);
        string streamingPath = Path.Combine(Application.streamingAssetsPath, dbName);

        // 1. Если файл в рабочей папке УЖЕ ЕСТЬ — просто отдаем путь
        if (File.Exists(persistentPath))
        {
            Debug.Log($"[Db] Работаю с существующей базой: {persistentPath}");
            return persistentPath;
        }

        Debug.Log($"[Db] Файл не найден в PersistentData. Ищу в StreamingAssets: {streamingPath}");

        if (File.Exists(streamingPath))
        {
            File.Copy(streamingPath, persistentPath);
            Debug.Log("[Db] База успешно скопирована из StreamingAssets.");
        }

        return persistentPath;
    }

    #region Tables

    public TableQuery<UnitEntity> Units => _connection.Table<UnitEntity>();

    #endregion

    public void Dispose()
    {
        _connection?.Close();
        _connection?.Dispose();        
    }
}