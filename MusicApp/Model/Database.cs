using Microsoft.Data.Sqlite;

namespace MusicApp.Model;

public sealed class DatabaseUnavailableException(string message) : Exception(message);

public static class Database
{
    /// <summary>
    /// Abre la base existente. Mode=ReadWrite evita que SQLite cree un archivo vacío si la ruta es incorrecta,
    /// y ForeignKeys=true activa PRAGMA foreign_keys (SQLite lo trae apagado por conexión).
    /// </summary>
    public static SqliteConnection Open(string path)
    {
        if (!File.Exists(path))
            throw new DatabaseUnavailableException($"No existe la base de datos: {path}");

        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = path,
            Mode = SqliteOpenMode.ReadWrite,
            ForeignKeys = true
        }.ToString();

        var connection = new SqliteConnection(connectionString);
        try
        {
            connection.Open();
            return connection;
        }
        catch
        {
            connection.Dispose();
            throw;
        }
    }
}
