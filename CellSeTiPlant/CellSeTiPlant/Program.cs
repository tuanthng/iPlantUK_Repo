using System;
using Plossum.CommandLine;
using System.Windows;

namespace CellSeTiPlant
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.WriteLine ("CellSetiPlant started!");

			int argc = args.Length;

			CellSetOptions options = new CellSetOptions ();
			CommandLineParser parser = new CommandLineParser (options);
			parser.Parse ();

			Console.WriteLine (parser.UsageInfo.GetHeaderAsString (78));
			if (options.Help) {
				Console.WriteLine (parser.UsageInfo.GetOptionsAsString (78));
				//return 0;
				return;
			} else if (parser.HasErrors) {
				Console.WriteLine (parser.UsageInfo.GetErrorsAsString (78));
				//return -1;
				return;
			}

			if (argc > 0) {
				CellSeTMain mainCellSet = new CellSeTMain ();

				mainCellSet.Process ();

			}
		}
	}
}
