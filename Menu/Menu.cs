using AutoGenerationModels.DatabaseObjects;
using Microsoft.Data.SqlClient;
using Spectre.Console;

namespace AutoGenerationModels.Menu
{
	public static class Menu
	{
		public static void MainMenu()
		{
			string connectionString;
			while (true)
			{
				connectionString = AnsiConsole.Ask<string>("[bold blue]Enter your connection string (or \"Exit\" to leave):[/]");

				if (connectionString.Equals("Exit", StringComparison.OrdinalIgnoreCase))
				{
					AnsiConsole.MarkupLine("[bold red]Exiting the program... Goodbye![/]");
					Environment.Exit(0);
				}

				if (TestDatabaseConnection(connectionString))
				{
					AnsiConsole.MarkupLine("[bold green]Connection Succeeded![/]");
					break;
				}
				else
				{
					AnsiConsole.MarkupLine("[bold red]Connection failed! Please check your connection string and try again.[/]");
				}
			}

			while (true)
			{
				var MainChoice = AnsiConsole.Prompt(
					new SelectionPrompt<string>()
						.Title("[bold blue]Select the type that you want to create:[/]")
						.AddChoices(new[] {
					"Store Procedure",
					"Function Table",
					"Exit"
						}));

				HandleExitOrBack(MainChoice);

				SubMenu(MainChoice, connectionString);
			}
		}

		public static void SubMenu(string name, string connectionString)
		{
			var actions = new Dictionary<string, Dictionary<string, Action>>{
				{
					"Store Procedure",
					new Dictionary<string, Action>
					{
						{ "All", () => Store.GenerateModelFromProcedure(connectionString) },
						{ "Detail", () => Store.GenerateDetailModelFromProcedure(connectionString)}
					}
				},
				{
					"Function Table", new Dictionary<string, Action>
					{
						{ "All", () => TVFunction.GenerateModelFromTVF(connectionString) },
						{ "Detail", () => TVFunction.GenerateDetailModelFromTVF(connectionString)}
					}
				}
			};

			while (true)
			{
				var SubChoice = AnsiConsole.Prompt(
					new SelectionPrompt<string>()
						.Title($"[bold blue]What do you want to do with {name}:[/]")
						.AddChoices(new[] {
					"All",
					"Detail",
					"Back",
					"Exit"
						}));

				if (HandleExitOrBack(SubChoice))
				{
					return;
				}

				if (actions.ContainsKey(name) && actions[name].ContainsKey(
					SubChoice))
				{
					actions[name][SubChoice]();
				}
				else
				{
					AnsiConsole.MarkupLine("[bold red]Invalid choice or action not implemented. Please try again.[/]");
				}
			}
		}


		private static bool TestDatabaseConnection(string connectionString)
		{
			try
			{
				using (var connection = new SqlConnection(connectionString))
				{
					connection.Open();
					return true;
				}
			}
			catch (SqlException)
			{
				return false;
			}
		}

		private static bool HandleExitOrBack(string value)
		{
			if (value == "Exit")
			{
				AnsiConsole.MarkupLine("[bold red]Exiting the program... Goodbye![/]");
				Environment.Exit(0);
			}
			if (value == "Back")
			{
				AnsiConsole.MarkupLine("[bold yellow]Returning to main menu...[/]");
				return true;
			}
			return false;
		}
	}
}