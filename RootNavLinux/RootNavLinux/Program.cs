﻿using System;

using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using System.Windows;
using Plossum.CommandLine;

namespace RootNavLinux
{
//	[CommandLineManager(ApplicationName = "RootNavLinux", Copyright = "University of Nottingham")]
//	class Options
//	{
//
//		[CommandLineOption(Description = "Displays this help text")]
//		public bool Help = false;
//
//		[CommandLineOption(Description = "Specifies the input file", MinOccurs = 0)]
//		public string Name
//		{
//			get { return mName; }
//			set
//			{
//				if (String.IsNullOrEmpty(value))
//					throw new InvalidOptionValueException("The name must not be empty", false);
//				mName = value;
//			}
//		}
//
//		private string mName;
//	}

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


			/*Options options = new Options();
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
			Console.WriteLine("Hello {0}!", options.Name);
			*/

			if (argc > 0) {
				RootNavMain mainRoot = new RootNavMain (args [0]);	

			} else {
				Console.WriteLine ("Please provide an image file");
			}


		}
	}
}