class Analyze
{
    public Song ReadMeData(string path)
    {
        using var file = TagLib.File.Create(path);

        string title = file.Tag.Title;
        string[] artists = file.Tag.Performers;
        uint year = file.Tag.Year;
        string album = file.Tag.Album;
        string[] albumArtists = file.Tag.AlbumArtists;
        string[] genres = file.Tag.Genres;
        uint track = file.Tag.Track;

        Song music = new Song(
            title,
            artists,
            year,
            album,
            albumArtists,
            track,
            genres,
            path
        );

        return music;
    }
}