 class Program
{
    static void Main(string[] args)
    {


        AddDataBase añadidor = new AddDataBase(); 
        long p1 = añadidor.GetOrCreatePerformer("Daft Punk", "/home/sexarprar/Uni/modelado/proyecto2/musica.db");
        long p2 = añadidor.GetOrCreatePerformer("Daft Punk", "/home/sexarprar/Uni/modelado/proyecto2/musica.db");

        Console.WriteLine(p1); 
        Console.WriteLine(p2);

        long p3 = añadidor.GetOrCreateAlbum("Fobia", "/home/sexarprar/Uni/modelado/proyecto2/musica.db", "/home/sexarprar/Uni/modelado/proyecto2/pruebaPathAlbum", 2000);
        Console.WriteLine(p3);

        
    }
}