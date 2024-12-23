using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AutoGenerationModels.MiddleWare;
using Dapper;
using Microsoft.Data.SqlClient;
using Spectre.Console;

namespace AutoGenerationModels.DatabaseObjects
{
	public static class TVFunction
	{
		public static void GenerateDetailModelFromTVF(string connectionString)
		{
			try
			{
				using (var connection = new SqlConnection(connectionString))
				{
					var storedProcedures = connection.Query<string>(
						   "SELECT name FROM sys.objects WHERE type IN ('FN', 'IF') ORDER BY name"
					   );
					var list = Generation.ShowList(storedProcedures);
					if (list == null) return;

					GenerateModelFromTVF(connectionString, list);
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

		public static void GenerateModelFromTVF(string connectionString, IEnumerable<string>? functionNames = null)
		{
			try
			{
				string output;
				output = AnsiConsole.Ask<string>("[bold blue]Please enter the folder path you wish to save. Example: Models or Models/example/... ( or \"Back\" to return):[/]");

				if (output.Equals("Back", StringComparison.OrdinalIgnoreCase))
				{
					return;
				}

				using (var connection = new SqlConnection(connectionString))
				{
					connection.Open();

					var functions = functionNames ?? connection.Query<string>(
						   "SELECT name FROM sys.objects WHERE type IN ('FN', 'IF') ORDER BY name"
					   );

					foreach (var func in functions)
					{
						AnsiConsole.MarkupLine($"[bold yellow]Executing TVFunction: {func}[/]");

						var sqlParams = Generation.GetSqlParameters(connection, func);

						using (var command = new SqlCommand($"select * FROM dbo.{func}({string.Join(",", sqlParams.Select(p => p.ParameterName))}) ", connection))
						{
							command.CommandType = CommandType.Text;

							command.Parameters.AddRange(sqlParams);

							Generation.GenerateModel(func, command, output);
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