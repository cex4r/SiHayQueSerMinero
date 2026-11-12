namespace MusicApp.Model;

/// <summary>Valores iguales a los ids de la tabla types.</summary>
public enum ArtistType
{
    Person = 0,
    Group = 1,
    Unknown = 2
}

/// <summary>Datos de la tabla persons.</summary>
public sealed class PersonInfo
{
    public string? StageName { get; set; }
    public string? RealName { get; set; }
    public string? BirthDate { get; set; }
    public string? DeathDate { get; set; }
}

/// <summary>Intérprete (tabla performers + types). Person solo se llena cuando Type es Person.</summary>
public sealed class Artist
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public ArtistType Type { get; set; } = ArtistType.Unknown;
    public PersonInfo? Person { get; set; }
}
