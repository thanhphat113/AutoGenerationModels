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
		public static void GenerateDetailModelFromTVF(string connectionString, string output)
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

					GenerateModelFromTVF(connectionString, output, list);
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

		public static void GenerateModelFromTVF(string connectionString, string output, IEnumerable<string>? functionNames = null)
		{
			try
			{
				using (var connection = new SqlConnection(connectionString))
				{
					connection.Open();

					var functions = (functionNames == null || !functionNames.Any()) ? connection.Query<string>(
						   "SELECT name FROM sys.objects WHERE type IN ('FN', 'IF') ORDER BY name"
					   ) : functionNames;

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