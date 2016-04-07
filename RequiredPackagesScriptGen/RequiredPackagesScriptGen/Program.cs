using System;
using System.IO;

namespace RequiredPackagesScriptGen
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.WriteLine ("Generating a script file...");

			string filename = "requirements.txt";
			string scriptFile = "installPacks.sh";

			// Open the file into a StreamReader
			StreamReader file = File.OpenText(filename);
			// Hook a write to the text file.
			StreamWriter writer = new StreamWriter(scriptFile);

			// Read the file into a string
			//string s = file.ReadToEnd();

			writer.WriteLine ("sudo apt-get install gcc");
			writer.WriteLine ("sudo apt-get install gfortran");
			writer.WriteLine ("sudo apt-get install python-dev");
			writer.WriteLine ("sudo apt-get install python-virtualenv");
			writer.WriteLine ("sudo apt-get install libxml2-dev");
			writer.WriteLine ("sudo apt-get install libxslt1-dev");
			writer.WriteLine ("sudo apt-get install postgresql");
			writer.WriteLine ("sudo apt-get install libopenslide-dev");
			writer.WriteLine ("sudo apt-get install libtiff4-dev");
			writer.WriteLine ("sudo apt-get update");
			writer.WriteLine ("# remember to install setuptools, matlab, configure postgres, mysql");
			//make sure to use the latest virtualenv package. Some errors have happened while isntalling bisque on Debian 7
			writer.WriteLine ("sudo pip install virtualenv --upgrade");
			writer.WriteLine ("sudo apt-get install build-essential");
			writer.WriteLine ("sudo apt-get install libbz2-dev");

			//Install required packages to compile imgcnv.
			//Note: Thecodec packages (libopenjpeg-dev, libschroedinger-dev, libtheora-dev and libxvidcore-dev) may not be needed for imgcnv in later versions
			writer.WriteLine ("sudo apt-get install libopenjpeg-dev");
			writer.WriteLine ("sudo apt-get install libschroedinger-dev");
			writer.WriteLine ("sudo apt-get install libtheora-dev");
			writer.WriteLine ("sudo apt-get install qt4-qmake");
			writer.WriteLine ("sudo apt-get install qt4-dev-tools");
			writer.WriteLine ("sudo apt-get install libhdf5-dev");
			writer.WriteLine ("sudo apt-get install python-psycopg2");
			writer.WriteLine ("sudo apt-get install libpq-dev");
			writer.WriteLine ("# install setuptools");
			writer.WriteLine ("wget https://bootstrap.pypa.io/ez_setup.py -O - | sudo python");

			writer.WriteLine ("# install bisque");
			writer.WriteLine ("mkdir bisque; cd bisque");
			writer.WriteLine ("wget http://biodev.ece.ucsb.edu/projects/bisquik/export/tip/bisque-stable/contrib/bootstrap/bisque-bootstrap.py");
			writer.WriteLine ("python bisque-bootstrap.py");

			writer.WriteLine ("");

			while (!file.EndOfStream) 
			{
				string line = file.ReadLine ();	

				if (line.Length > 0) {
					if (line [0] != '#' && line [0] != '-') {
						// Add a single line
						string newPackage = "";

						//ignore some special packages above
						if (line == "Minimatic==1.0.4" || line == "Paste==1.7.5.2" || line == "PasteScript==1.7.3"
						    || line == "tgext.registration2==0.5.2" || line == "tw.output==0.5.0dev-20110906" ||
						    line == "numpy==1.11.0" || line == "TurboGears2==2.1.5" ||	line == "httplib2==0.7.1-1") {

							newPackage = "# sudo pip install " + line;

						} else {
							newPackage = "sudo pip install " + line;	
						}

						writer.WriteLine (newPackage);	
					}
				}

			}

			//write special packages
			writer.WriteLine ("sudo pip install Minimatic-1.0.4-py2-none-any.whl");
			writer.WriteLine ("sudo pip install Paste-1.7.5.2-py2-none-any.whl");
			writer.WriteLine ("sudo pip install PasteScript-1.7.3-py2-none-any.whl");
			writer.WriteLine ("sudo pip install TurboGears2-2.1.5-py2-none-any.whl");
			writer.WriteLine ("sudo pip install httplib2-0.7.1_1-py2-none-any.whl");
			writer.WriteLine ("sudo pip install numpy-1.11.0-cp27-cp27mu-linux_x86_64.whl");
			writer.WriteLine ("sudo pip install tgext.registration2-0.5.2-py2-none-any.whl");
			writer.WriteLine ("sudo pip install tw.output-0.5.0dev_20110906-py2-none-any.whl");
			writer.WriteLine ("sudo pip install pylibtiff-0.3.0_1-cp27-none-linux_x86_64.whl");



			// Close the file so it can be accessed again.
			file.Close();
			// Close the writer
			writer.Close();

			Console.WriteLine ("Finished!");
		}
	}
}
