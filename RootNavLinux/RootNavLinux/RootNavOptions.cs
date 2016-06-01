using System;

using Plossum.CommandLine;
using System.Collections.Generic;

using RootNav.Core.MixtureModels;

namespace RootNavLinux
{
	[CommandLineManager(ApplicationName = "RootNavLinux", Copyright = "University of Nottingham")]
	public class RootNavOptions
	{
		public RootNavOptions ()
		{
			//Weights = new List<double> ();
		}

		[CommandLineOption(Description = "Displays this help text")]
		public bool Help = false;

		[CommandLineOption(Description = "Specifies the preset root", MinOccurs = 1)]
		public string PresetName
		{
			get { return presetName; }
			set
			{
				if (String.IsNullOrEmpty(value))
					throw new InvalidOptionValueException("The preset name must not be empty", false);
				presetName = value;
			}
		}

		[CommandLineOption(Description = "Specifies the image file", MinOccurs = 1)]
		public string ImageFile
		{
			get { return imageFile; }
			set
			{
				if (String.IsNullOrEmpty(value))
					throw new InvalidOptionValueException("The image file must not be empty", false);
				imageFile = value;
			}
		}

		private string presetName;
		private string imageFile;

		[CommandLineOption(Description = "Specifies the Initial Class Count", MinOccurs = 0)]
		public int InitialClassCount { get; set; }

		[CommandLineOption(Description = "Specifies the Maximum Class Count", MinOccurs = 0)]
		public int MaximumClassCount { get; set; }

		[CommandLineOption(Description = "Specifies the Expected Root Class Count", MinOccurs = 0)]
		public int ExpectedRootClassCount { get; set; }

		[CommandLineOption(Description = "Specifies the Patch Size", MinOccurs = 0)]
		public int PatchSize { get; set; }

		[CommandLineOption(Description = "Specifies the Background Percentage", MinOccurs = 0)]
		public double BackgroundPercentage { get; set; }

		[CommandLineOption(Description = "Specifies the Background Excess Sigma", MinOccurs = 0)]
		public double BackgroundExcessSigma { get; set; }

		[CommandLineOption(Description = "Specifies the Weights", RequireExplicitAssignment= true, MinOccurs = 0)]
		public string Weights { get; set; }

		[CommandLineOption(Description = "Specifies the input path", RequireExplicitAssignment= true, MinOccurs = 0)]
		public string InputPath { get; set; }

		[CommandLineOption(Description = "Specifies the output path", RequireExplicitAssignment= true, MinOccurs = 0)]
		public string OutputPath { get; set; }

		[CommandLineOption(Description = "Specifies the input nodes (primary, source, lateral)", RequireExplicitAssignment= true, MinOccurs = 0)]
		public string InputPointsFile { get; set; }

		[CommandLineOption(Description = "Specifies the resolution of the image", RequireExplicitAssignment= true, MinOccurs = 0)]
		public string ImageResolutionValue { get; set; }

		[CommandLineOption(Description = "Spline spacing", RequireExplicitAssignment= true, MinOccurs = 0)]
		public string SplineSpacing { get; set; }

		[CommandLineOption(Description = "Plant name", RequireExplicitAssignment= true, MinOccurs = 0)]
		public string PlantName { get; set; }

		[CommandLineOption(Description = "Use Curvature Profile (true/false)?", RequireExplicitAssignment= true, MinOccurs = 0)]
		public string CurvatureProfile { get; set; }

		[CommandLineOption(Description = "Use Map Profile (true/false)?", RequireExplicitAssignment= true, MinOccurs = 0)]
		public string MapProfile { get; set; }

		[CommandLineOption(Description = "Use Travel Map if MapProfile enabled?", RequireExplicitAssignment= true, MinOccurs = 0)]
		public string TravelMap { get; set; }

		[CommandLineOption(Description = "Complete architecture recorded?", RequireExplicitAssignment= true, MinOccurs = 0)]
		public string CompleteArch { get; set; }

		[CommandLineOption(Description = "Execute measure?", RequireExplicitAssignment= true, MinOccurs = 0)]
		public string DoMeasurement { get; set; }

		[CommandLineOption(Description = "Output measure table?", RequireExplicitAssignment= true, MinOccurs = 0)]
		public string DoMeasurementTable { get; set; }

		public EMConfiguration CreateConfiguration()
		{
			//create custom configuration
			EMConfiguration custom = new EMConfiguration();

			custom.BackgroundExcessSigma = this.BackgroundExcessSigma;
			custom.BackgroundPercentage = this.BackgroundPercentage;
			custom.ExpectedRootClassCount = this.ExpectedRootClassCount;
			custom.InitialClassCount = this.InitialClassCount;
			custom.MaximumClassCount = this.MaximumClassCount;
			custom.Name = this.PresetName;
			custom.PatchSize = this.PatchSize;

			Weights.Replace (" ", "");

			var splitted = Weights.Split(new []{","}, StringSplitOptions.RemoveEmptyEntries);

			List<double> ws = new List<double> ();

			foreach (string s in splitted) {
				double d;
				Double.TryParse (s, out d);
				ws.Add (d);
			}

			custom.Weights = ws.ToArray();

			return custom;
		}

		public override string ToString()
		{
			string line = String.Format ("InitialClassCount: {0} " +
				"\nMaximumClassCount: {1} " +
				"\nExpectedRootClassCount: {2} " +
				"\nPatchSize: {3} " +
				"\nBackgroundPercentage: {4} " +
				"\nBackgroundExcessSigma: {5} " +
				"\nInput path: {6} " +
				"\nOutput path: {7}", 
				this.InitialClassCount, 
				this.MaximumClassCount, 
				this.ExpectedRootClassCount, 
				this.PatchSize,
				this.BackgroundPercentage, 
				this.BackgroundExcessSigma, 
				this.InputPath != null ? this.InputPath : " ", 
				this.OutputPath != null ? this.InputPath : " ");

			string weight = "Weights: " + Weights != null? this.Weights : " ";

			line += weight;

			return line;
		}
	}


}

