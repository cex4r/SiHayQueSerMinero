using Microsoft.Data.Sqlite; 

class AddDataBase
{
    private readonly string connectionString;

    public AddDataBase(string dataBasePath)
    {
        connectionString = $"Data Source = {dataBasePath}";
    }

    public void TestConnection()
    {
        using var connection = new SqliteConnection(connectionString);

        connection.Open();

        Console.WriteLine("Hubo conexión :D"); 


    }
}