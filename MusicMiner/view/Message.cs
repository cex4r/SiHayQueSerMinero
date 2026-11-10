
class Message
{
    public void ShowResult(MiningResult result)
    {
        Console.WriteLine("Minado terminado.");
        Console.WriteLine();

        Console.WriteLine(
            $"Archivos encontrados: {result.Found}"
        );

        Console.WriteLine(
            $"Canciones registradas: {result.Registered}"
        );

        Console.WriteLine(
            $"Canciones omitidas: {result.Skipped}"
        );

        if (result.Errors.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("Motivo:");

            foreach (string error in result.Errors)
            {
                Console.WriteLine($"- {error}");
            }
        }
    }
}
