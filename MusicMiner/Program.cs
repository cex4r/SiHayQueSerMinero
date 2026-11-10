class Program
{
    static void Main()
    {
        Miner minero = new Miner();
        Message vista = new Message();

        MiningResult resultado = minero.Mina(
            "/home/sexarprar/Uni/modelado/proyecto2/pruebas",
            "/home/sexarprar/Uni/modelado/proyecto2/musica.db"
        );

        vista.ShowResult(resultado);
    }
}