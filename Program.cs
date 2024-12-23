
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Text;
using System.Diagnostics;
using Dapper;
using AutoGenerationModels.Menu;


class Program
{
	static void Main(string[] args)
	{
		Menu.MainMenu();


		// string connectionString = args[0];
		// string outputString = "Models";

		// Console.WriteLine($"Chuỗi kết nối: {connectionString}");
		// Console.WriteLine($"Thư mục đầu ra: {outputString}");

		// RunMyFunction(connectionString, outputString);
	}
}
