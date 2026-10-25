 class Program
{
    static void Main(string[] args)
    {

        Search buscador = new Search();
        var rutas = buscador.FindMp3Files("/home/sexarprar/Uni/modelado/proyecto2/pruebas");

        foreach(string ruta in rutas)
        {
            Console.WriteLine(ruta);
        }
    }
}