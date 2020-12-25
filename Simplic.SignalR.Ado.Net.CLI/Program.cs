using Dapper;
using Simplic.SignalR.Ado.Net.Client;
using System;
using System.Diagnostics;

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

                    // Add parameter
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "sampleParam";
                    parameter.Value = "Max";
                    parameter.DbType = System.Data.DbType.String;

                    command.CommandText = "SELECT cast(:sampleParam as varchar) t";

                    Console.WriteLine($"Output of: {command.CommandText}: {command.ExecuteScalar()}");

                    Console.WriteLine("Make some dapper tests... (scalar)");
                    Console.WriteLine($"Scalar: {connection.ExecuteScalar<int>("select :iv", new { iv = 1234 })}");

                    command.CommandText = "SELECT COUNT(*) FROM ESS_MS_Intern_Exception";

                    var integer = command.ExecuteScalar();

                    Console.WriteLine($"User-Count {integer}; {command.CommandText}");


                    Console.WriteLine($"Scalar: {connection.Query<bool>("select :iv", new { iv = 1234 })}");

                    var timer = Stopwatch.StartNew();
                    command.CommandText = "select * from IT_Document";
                    var reader = command.ExecuteReader();
                    var ms = timer.ElapsedMilliseconds;

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        Console.WriteLine($"Field type: {i}/{reader.GetName(i)} => {reader.GetDataTypeName(i)}/{reader.GetFieldType(i)}");
                    }

                    Console.WriteLine($"Reader execution time: {ms}ms");


                    timer = Stopwatch.StartNew();
                    for (int k = 0; k < 100; k++)
                    {
                        command.CommandText = "select * from it_document";
                        command.ExecuteScalar();
                    }
                    ms = timer.ElapsedMilliseconds;
                    Console.WriteLine($"Execution time {ms}ms");

                    while (true)
                    {
                        command.CommandText = Console.ReadLine();
                        if (command.CommandText == "" || command.CommandText == "exit")
                            break;

                        try
                        {
                            timer = Stopwatch.StartNew();
                            var dt = command.ExecuteScalar();
                            ms = timer.ElapsedMilliseconds;
                            Console.WriteLine($"Result: {command.ExecuteScalar()}. Execution time: {ms}ms");
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
