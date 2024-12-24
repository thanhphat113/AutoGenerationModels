using System.Data;
using AutoGenerationModels.MiddleWare;
using Dapper;
using Microsoft.Data.SqlClient;
using Spectre.Console;


namespace AutoGenerationModels.DatabaseObjects
{
	public static class Store
	{
		public static void GenerateDetailModelFromProcedure(string connectionString, string output)
		{
			try
			{
				using (var connection = new SqlConnection(connectionString))
				{
					var storedProcedures = connection.Query<string>(
						   "SELECT name FROM sys.objects WHERE type = 'P' ORDER BY name"
					   );
					var list = Generation.ShowList(storedProcedures);
					if (list == null) return;

					GenerateModelFromProcedure(connectionString, output, list);
				}
			}
			catch (SqlException sqlEx)
			{
				Console.WriteLine($"SQL error: {sqlEx.Message}");
			}
			catch (InvalidOperationException opEx)
			{
				Console.WriteLine($"Connection error: {opEx.Message}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Unknown error: {ex.Message}");
			}
		}

		public static void GenerateModelFromProcedure(string connectionString, string output, IEnumerable<string>? proceduceNames = null)
		{
			try
			{
				using (var connection = new SqlConnection(connectionString))
				{
					connection.Open();

					var storedProcedures = proceduceNames ?? connection.Query<string>(
						   "SELECT name FROM sys.objects WHERE type = 'P' ORDER BY name"
					   );

					foreach (var store in storedProcedures)
					{
						AnsiConsole.MarkupLine($"[bold yellow]Executing store procedure: {store}[/]");

						var sqlParams = Generation.GetSqlParameters(connection, store);

						using (var command = new SqlCommand(store, connection))
						{
							command.CommandType = CommandType.StoredProcedure;

							command.Parameters.AddRange(sqlParams);

							Generation.GenerateModel(store, command, output);
						}
					}
				}
			}
			catch (SqlException sqlEx)
			{
				Console.WriteLine($"SQL error: {sqlEx.Message}");
			}
			catch (InvalidOperationException opEx)
			{
				Console.WriteLine($"Connection error: {opEx.Message}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Unknown error: {ex.Message}");
			}
		}
	}
}