 class Program
{
    static void Main(string[] args)
    {

        Search buscador = new Search();
        var rutas = buscador.FindMp3Files("/home/sexarprar/Uni/modelado/proyecto2/pruebas");
        Analyze analizador = new Analyze(); 

        foreach(string ruta in rutas)
        {
            Console.WriteLine(ruta);
            Song cancion = analizador.ReadMeData(ruta);
            Console.WriteLine(cancion.Title);
            Console.WriteLine(cancion.Artist);
            Console.WriteLine(cancion.Year);
            Console.WriteLine(cancion.Album);
            Console.WriteLine(cancion.AlbumArtist);
            Console.WriteLine(cancion.Track);
            Console.WriteLine(cancion.Genre);
            Console.WriteLine(cancion.Path);
        }

        

        
    }
}