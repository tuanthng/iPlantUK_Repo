using System;

using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using System.Windows;
using Plossum.CommandLine;

using RootNav.Core.MixtureModels;

namespace RootNavLinux
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			try {

				Console.WriteLine ("RootNav started!");

				int argc = args.Length;
//
//			Console.WriteLine ("Total argc: " + argc.ToString());
//
//			for (int i = 0; i < argc; i++) {
//				Console.WriteLine (args[i]);
//			}

				RootNavOptions options = new RootNavOptions ();
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
					RootNavMain mainRoot = new RootNavMain (options.ImageFile);	

					mainRoot.PresetRootName = options.PresetName;

					if (options.PresetName.Equals ("Custom")) {
						System.Console.WriteLine ("In custom");
						System.Console.WriteLine (options.ToString ());

						mainRoot.CustomEMConfiguration = options.CreateConfiguration ();

					}
					//only update the input and output paths if they are provided and not are the current directory
					if (options.InputPath != null && !options.InputPath.Equals("."))
					{
						mainRoot.InputPath = options.InputPath;
					}
					if (options.OutputPath != null && !options.OutputPath.Equals("."))
					{
						mainRoot.OutputPath = options.OutputPath;
					}

					if (options.InputPointsFile != null && options.InputPointsFile.Length > 0)
					{
						mainRoot.InputPointsFilename = options.InputPointsFile;
					}

					//check measurement
					if (options.DoMeasurement != null && options.DoMeasurement.Equals("true"))
					{
						mainRoot.DoMeasurement = true;
						bool tempBool;
						double tempDouble;

						bool result = double.TryParse(options.ImageResolutionValue, out tempDouble);
						mainRoot.ImageResolutionValue = result ? tempDouble : 0;
						int tempInt;

						result = Int32.TryParse(options.SplineSpacing, out tempInt);
						mainRoot.SplineSpacing = result ? tempInt : 40; //use default when false

						result = Boolean.TryParse(options.CurvatureProfile, out tempBool);
						mainRoot.DoCurvatureProfile = result ? tempBool : true;

						result = Boolean.TryParse(options.MapProfile, out tempBool);
						mainRoot.DoMapProfile = result ? tempBool : true;

						result = Int32.TryParse(options.TravelMap, out tempInt);
						mainRoot.TravelMap = result ? tempInt : 40;

						result = Boolean.TryParse(options.CompleteArch, out tempBool);
						mainRoot.DoCompleteArch = result ? tempBool : true;

						mainRoot.PlantName = options.PlantName;

						result = Boolean.TryParse(options.DoMeasurementTable, out tempBool);
						mainRoot.DoMeasurementTable = result ? tempBool : true;
					}
					else 
					{
						mainRoot.DoMeasurement = false;
					}
					//process the task
					mainRoot.Process ();

					//save data before exists the program
					//mainRoot.saveData();

				} else {
					Console.WriteLine ("Using the default image.");

					RootNavMain mainRoot = new RootNavMain ("0002.jpg");	
					//process the task
					mainRoot.Process ();

				}
			} catch (Exception e) {
				Console.WriteLine ("Exception: " + e.Message);
			}

		}
	}
}
