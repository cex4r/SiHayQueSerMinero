
using Microsoft.Data.Sqlite;

class Program
{
    static void Main()
    {
        Miner minero = new Miner(); 

        string mensaje = minero.Mina("/home/sexarprar/Uni/modelado/proyecto2/pruebas", "/home/sexarprar/Uni/modelado/proyecto2/musica.db");
        Console.WriteLine(mensaje); 

    }
}
