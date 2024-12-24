
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Text;
using System.Diagnostics;
using Dapper;
using Newtonsoft.Json.Linq;
using AutoGenerationModels.Menu;
using AutoGenerationModels.DatabaseObjects;


class Program
{
	static void Main(string[] args)
	{
		if (args.Length == 0) Menu.MainMenu();

		string appsettingsPath = "appsettings.Development.json";

		if (!File.Exists(appsettingsPath))
		{
			Console.WriteLine($"Error: File not found at {appsettingsPath}");
		}

		string connectionString = "";

		try
		{
			string jsonContent = File.ReadAllText(appsettingsPath);

			var jsonObject = JObject.Parse(jsonContent);

			var connectionStrings = jsonObject["ConnectionStrings"];
			if (connectionStrings == null)
			{
				Menu.MainMenu();
			}

			connectionString = connectionStrings["Connection"]?.ToString();

			if (string.IsNullOrEmpty(connectionString))
			{
				Console.WriteLine("Error: Connection string with key 'default' not found.");
			}

		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error reading the appsettings file: {ex.Message}");
		}

		List<string> functions = new List<string>();
		List<string> store = new List<string>();
		List<string> actions = new List<string>();
		string output = "Models";
		string typeMain = "";

		foreach (var item in args)
		{
			if (item.StartsWith("-"))
			{
				typeMain = item;
				actions.Add(item);
				continue;
			}

			switch (typeMain)
			{
				case "-o":
					output = item;
					break;
				case "-f":
					functions.Add(item);
					break;
				case "-s":
					store.Add(item);
					break;
				default:
					break;
			}
		}

		if (actions.Contains("-f"))
		{
			TVFunction.GenerateModelFromTVF(connectionString, output, functions);
		}

		if (actions.Contains("-s"))
		{
			Store.GenerateModelFromProcedure(connectionString, output, store);
		}
	}
}
