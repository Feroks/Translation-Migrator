using System;
using System.Threading.Tasks;
using CommandLine;

namespace TranslationsMigrator
{
	internal static class Program
	{
		private static void Main(string[] args)
		{
			Parser
				.Default
				.ParseArguments<Options>(args)
				.MapResult(
					options => Task.CompletedTask, 
					_ => Task.CompletedTask);

			Console.ReadLine();
		}
	}
}