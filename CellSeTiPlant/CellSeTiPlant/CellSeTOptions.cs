using System;

using Plossum.CommandLine;
using System.Collections.Generic;


namespace CellSeTiPlant
{
	[CommandLineManager(ApplicationName = "CellSeTiPlant", Copyright = "University of Nottingham")]
	public class CellSetOptions
	{
		public CellSetOptions ()
		{
			//Weights = new List<double> ();
		}

		[CommandLineOption(Description = "Displays this help text")]
		public bool Help = false;

		[CommandLineOption(Description = "Specifies the input data file", MinOccurs = 1)]
		public string InputDataFile
		{
			get { return inputDataFile; }
			set
			{
				if (String.IsNullOrEmpty(value))
					throw new InvalidOptionValueException("The input data file must not be empty", false);
				inputDataFile = value;
			}
		}

		private string inputDataFile;


		public override string ToString()
		{
			string line = String.Format (
				              "Input data: {0} ",
				              this.inputDataFile != null ? this.inputDataFile : " ");
			
			return line;
		}
	}


}

