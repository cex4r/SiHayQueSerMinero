namespace MusicApp.Controller;

/// <summary>Distingue fallos técnicos y de validación de los resultados normales (p. ej. lista vacía).</summary>
public enum ErrorKind
{
    None,
    Validation,
    Connection,
    Database,
    Duplicate,
    NotFound,
    Miner
}

public class Result
{
    public ErrorKind Error { get; protected init; }
    public string? Message { get; protected init; }
    public bool Success => Error == ErrorKind.None;
}

public sealed class Result<T> : Result
{
    public T? Value { get; private init; }

    public static Result<T> Ok(T value) => new() { Value = value };
    public static Result<T> Fail(ErrorKind error, string message) => new() { Error = error, Message = message };

    /// <summary>Reenvía el error de otro resultado con otro tipo de valor.</summary>
    public static Result<T> FailFrom(Result other) => Fail(other.Error, other.Message ?? "Error desconocido.");
}
