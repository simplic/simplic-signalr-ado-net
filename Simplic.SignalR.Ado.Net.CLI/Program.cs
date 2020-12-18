using Simplic.SignalR.Ado.Net.Client;
using System;

namespace Simplic.SignalR.Ado.Net.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var connection = new SignalRDbConnection())
            {
                System.Threading.Thread.Sleep(2000);
                connection.ConnectionString = "{driver=Sap.Data.SQLAnywhere;url=http://localhost:5000/};UID=admin;PWD=school;Server=setup;dbn=simplic;ASTART=No;links=tcpip";

                connection.Open();

                Console.WriteLine($"DataSource: {connection.DataSource} Database: {connection.Database} ConnectionTimeout: {connection.ConnectionTimeout} ServerVersion: {connection.ServerVersion}");

                var transaction = connection.BeginTransaction();
                transaction.Commit();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE ESS_MS_Intern_User SET UserName = 'Test' WHERE 1 = 2";
                    var affectedData = command.ExecuteNonQuery();

                    Console.WriteLine($"Affected {affectedData}; {command.CommandText}");

                    command.CommandText = "SELECT COUNT(*) FROM ESS_MS_Intern_Exception";

                    var integer = command.ExecuteScalar();

                    Console.WriteLine($"User-Count {integer}; {command.CommandText}");

                    while (true)
                    {
                        command.CommandText = Console.ReadLine();
                        if (command.CommandText == "" || command.CommandText == "exit")
                            break;

                        try
                        {
                            var dt = command.ExecuteScalar();

                            Console.WriteLine($"Result: {command.ExecuteScalar()}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"FAILED: {ex}");
                        }
                    }
                }

                Console.WriteLine("Press enter to close connection");
                Console.ReadLine();
            }

            Console.ReadLine();
        }
    }
}
