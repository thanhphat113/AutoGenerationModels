using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Spectre.Console;


namespace AutoGenerationModels.MiddleWare
{
	public static class Generation
	{
		public static void CreateClass(string ClassName, string outputString, IDictionary<string, string> list)
		{
			var nameSpace = outputString.Replace("/", ".");
			string rootDirectoryName = new DirectoryInfo(Directory.GetCurrentDirectory()).Name;
			StringBuilder classCode = new StringBuilder();
			classCode.AppendLine("using System;");
			classCode.AppendLine();
			classCode.AppendLine($"namespace {rootDirectoryName}.{nameSpace} {{");
			classCode.AppendLine($"\tpublic class {ClassName} {{");
			foreach (var item in list)
			{
				classCode.AppendLine($"\t\tpublic {item.Value} {item.Key} {{ get; set; }} {(item.Value.Equals("string") ? "= string.Empty;" : "")} ");
			}
			classCode.AppendLine("\t}");
			classCode.AppendLine("}");

			string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), outputString);

			Directory.CreateDirectory(directoryPath);

			string filePath = Path.Combine(directoryPath, $"{ClassName}.cs");
			if (File.Exists(filePath))
			{
				var overwrite = AnsiConsole.Prompt(new ConfirmationPrompt($"[bold blue]The file {ClassName}.cs already exists. Do you want to overwrite it?[/]"));
				if (!overwrite)
				{
					AnsiConsole.MarkupLine("[bold red]Operation aborted.[/]");
					return;
				}
			}

			File.WriteAllText(filePath, classCode.ToString());
			AnsiConsole.MarkupLine($"[bold green]File {ClassName}.cs has been successfully created[/]");
		}

		public static string MapSqlTypeToCSharpType(string sqlType)
		{
			return sqlType.ToLower() switch
			{
				"int32" => "int",
				"int64" => "long",
				"decimal" => "decimal",
				"double" => "double",
				"float" => "float",
				"bit" => "bool",
				"datetime" => "DateTime",
				"string" => "string",
				"date" => "DateTime",
				"guid" => "Guid",
				"binary" => "byte[]",
				"char" => "char",
				_ => throw new ArgumentException($"Kiểu dữ liệu '{sqlType}' không được hỗ trợ.")
			};
		}

		public static object GetDefaultValue(string type)
		{
			return type.ToLower() switch
			{
				"int" => 0,
				"float" => 0f,
				"nvarchar" or "varchar" => string.Empty,
				"decimal" => 0.0m,
				"date" => DateTime.Now.Date,
				"datetime" => DateTime.Now,
				"bit" => false,
				_ => throw new ArgumentException($"Kiểu dữ liệu '{type}' không được hỗ trợ.")
			};
		}

		public static void GenerateModel(string className, SqlCommand command, string output)
		{
			var list = new Dictionary<string, string>();
			using (var reader = command.ExecuteReader())
			{
				int columnCount = reader.FieldCount;

				for (int i = 0; i < columnCount; i++)
				{
					var columnName = reader.GetName(i);
					var columnType = reader.GetFieldType(i);
					list[columnName] = MapSqlTypeToCSharpType(columnType.Name);
				}

				CreateClass(className, output, list);
			}
		}

		public static SqlParameter[] GetSqlParameters(SqlConnection connection, string className)
		{
			var parameters = connection.Query<dynamic>(
						$"SELECT SPECIFIC_NAME, PARAMETER_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.PARAMETERS WHERE SPECIFIC_NAME = '{className}'")
							.ToList();

			return parameters.Select(parameter =>
				new SqlParameter(parameter.PARAMETER_NAME, GetDefaultValue(parameter.DATA_TYPE)
					 ?? DBNull.Value)).ToArray();
		}

		public static List<string> ShowList(IEnumerable<string> listNames, bool isStore = false)
		{
			AnsiConsole.MarkupLine($"[bold yellow]All {(isStore ? "stored procedures" : "functions")} in your database:[/]");
			foreach (var item in listNames)
			{
				AnsiConsole.MarkupLine($"[bold yellow]{item}.[/]");
			}
			var result = AnsiConsole.Ask<string>($"[bold blue]Please enter the exact name(s) of the {(isStore ? "stored procedures(s)" : "functions(s)")} you want to create. If entering multiple names, use a semicolon \";\" as a separator (or \"Back\" to return):[/]");
			if (result.Equals("Back", StringComparison.OrdinalIgnoreCase)) return null;
			return StringToList(result);
		}

		public static List<string> StringToList(string value)
		{
			return value.Split(';')
					.Select(item => item.Trim())
					 .Where(item => !string.IsNullOrEmpty(item))
					 .ToList();
			;
		}
	}

}