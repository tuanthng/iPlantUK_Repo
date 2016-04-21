using System;

using Plossum.CommandLine;
using System.Collections.Generic;

namespace RootNavLinux
{
	[CommandLineManager(ApplicationName = "RootNavLinux", Copyright = "University of Nottingham")]
	public class RootNavOptions
	{
		public RootNavOptions ()
		{
			
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
		public List<double> Weights { get; set; }

	}


}

