 class Program
{
    static void Main(string[] args)
    {


        AddDataBase añadidor = new AddDataBase(); 
        long p1 = añadidor.GetOrCreatePerformer("Daft Punk", "/home/sexarprar/Uni/modelado/proyecto2/musica.db");
        long p2 = añadidor.GetOrCreatePerformer("Daft Punk", "/home/sexarprar/Uni/modelado/proyecto2/musica.db");

        Console.WriteLine(p1); 
        Console.WriteLine(p2);
    }
}