using Microsoft.Data.Sqlite;

namespace MusicApp.Model;

internal static class ReaderExtensions
{
    public static string? GetStringOrNull(this SqliteDataReader r, int i) => r.IsDBNull(i) ? null : r.GetString(i);
    public static int? GetIntOrNull(this SqliteDataReader r, int i) => r.IsDBNull(i) ? null : r.GetInt32(i);
}
