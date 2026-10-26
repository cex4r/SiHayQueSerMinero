
    class Song
{
    public string Title { get; }
    public string[] Artist { get; }
    public uint? Year { get; }
    public string Album { get; }
    public string[] AlbumArtist { get; }
    public uint? Track { get; }
    public string[] Genre { get; }
    public string Path { get; }

    public Song(
        string title,
        string[] artist,
        uint? year,
        string album,
        string[] albumArtist,
        uint? track,
        string[] genre,
        string path)
    {
        Title = title;
        Artist = artist;
        Year = year;
        Album = album;
        AlbumArtist = albumArtist;
        Track = track;
        Genre = genre;
        Path = path;
    }
}