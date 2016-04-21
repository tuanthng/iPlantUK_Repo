using System;

using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using System.Windows;
using Plossum.CommandLine;

namespace RootNavLinux
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.WriteLine ("RootNav started!");

			int argc = args.Length;
//
//			Console.WriteLine ("Total argc: " + argc.ToString());
//
//			for (int i = 0; i < argc; i++) {
//				Console.WriteLine (args[i]);
//			}


			RootNavOptions options = new RootNavOptions();
			CommandLineParser parser = new CommandLineParser(options);
			parser.Parse();

			Console.WriteLine(parser.UsageInfo.GetHeaderAsString(78));
			if (options.Help)
			{
				Console.WriteLine(parser.UsageInfo.GetOptionsAsString(78));
				//return 0;
				return;
			}
			else if (parser.HasErrors)
			{
				Console.WriteLine(parser.UsageInfo.GetErrorsAsString(78));
				//return -1;
				return;
			}

			Console.ReadLine ();

			if (argc > 0) {
				RootNavMain mainRoot = new RootNavMain (args [0]);	
				//process the task
				mainRoot.Process ();
			} else {
				Console.WriteLine ("Using the default image.");

				RootNavMain mainRoot = new RootNavMain ("0002.jpg");	
				//process the task
				mainRoot.Process ();

			}


		}
	}
}
