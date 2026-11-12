using Microsoft.Data.Sqlite;
using MusicApp.Model;

namespace MusicApp.Controller;

/// <summary>
/// Abre una conexión, ejecuta la operación y la cierra, traduciendo excepciones a Result.
/// Centraliza el manejo de errores para que Finder y LibraryController no lo dupliquen.
/// </summary>
internal static class DbAccess
{
    public static Result<T> Run<T>(string dbPath, Func<SqliteConnection, T> operation)
    {
        try
        {
            using var connection = Database.Open(dbPath);
            return Result<T>.Ok(operation(connection));
        }
        catch (DatabaseUnavailableException e)
        {
            return Result<T>.Fail(ErrorKind.Connection, e.Message);
        }
        catch (DuplicateRecordException e)
        {
            return Result<T>.Fail(ErrorKind.Duplicate, e.Message);
        }
        catch (RecordNotFoundException e)
        {
            return Result<T>.Fail(ErrorKind.NotFound, e.Message);
        }
        catch (SqliteException e)
        {
            return Result<T>.Fail(ErrorKind.Database, $"Error de base de datos: {e.Message}");
        }
    }
}
