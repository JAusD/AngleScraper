using Microsoft.Data.Sqlite;

class AlbumTracker
{
    private readonly string _dbPath;

    public AlbumTracker(string dbPath)
    {
        _dbPath = dbPath;
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS ProcessedAlbums (
                ModelName TEXT,
                AlbumName TEXT,
                PRIMARY KEY (ModelName, AlbumName)
            );
        ";
        command.ExecuteNonQuery();
    }

    public bool IsProcessed(string modelName, string albumName)
    {
        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM ProcessedAlbums WHERE ModelName = @modelName AND AlbumName = @albumName";
        command.Parameters.AddWithValue("@modelName", modelName);
        command.Parameters.AddWithValue("@albumName", albumName);

        var count = (long)command.ExecuteScalar();
        return count > 0;
    }

    public void MarkAsProcessed(string modelName, string albumName)
    {
        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "INSERT OR IGNORE INTO ProcessedAlbums (ModelName, AlbumName) VALUES (@modelName, @albumName)";
        command.Parameters.AddWithValue("@modelName", modelName);
        command.Parameters.AddWithValue("@albumName", albumName);
        command.ExecuteNonQuery();
    }
}