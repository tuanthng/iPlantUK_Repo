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

			string serverType = "Centos"; //Debian, Centos, 

			// Open the file into a StreamReader
			StreamReader file = File.OpenText(filename);
			// Hook a write to the text file.
			StreamWriter writer = new StreamWriter(scriptFile);

			// Read the file into a string
			//string s = file.ReadToEnd();

			if (serverType.Equals ("Debian")) {
				writer.WriteLine ("sudo apt-get install gcc");
				writer.WriteLine ("sudo apt-get install git-all");
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

				writer.WriteLine ("wget https://bootstrap.pypa.io/ez_setup.py -O - | sudo python"); //setuptools

				//make sure to use the latest virtualenv package. Some errors have happened while isntalling bisque on Debian 7
				writer.WriteLine ("sudo pip install virtualenv --upgrade");
				writer.WriteLine ("sudo apt-get install build-essential");
				writer.WriteLine ("sudo apt-get install libbz2-dev");
			
				writer.WriteLine ("#install openjdk 7");

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
			} else if (serverType.Equals ("Centos")) {
				writer.WriteLine ("sudo yum install wget");
				writer.WriteLine ("sudo yum install gcc");

				writer.WriteLine ("sudo yum install git-all");

				writer.WriteLine ("sudo yum install gcc-gfortran");
				writer.WriteLine ("sudo yum install python-devel");
				writer.WriteLine ("sudo yum install python-virtualenv");

				writer.WriteLine ("sudo yum groupinstall \"Development Tools\"");

				writer.WriteLine ("sudo yum install libxml2");
				writer.WriteLine ("sudo yum install libxml2-python");
				writer.WriteLine ("sudo yum install libxml2-devel");

				writer.WriteLine ("sudo yum install libjpeg-devel");
				writer.WriteLine ("sudo yum install openjpeg-devel");
				writer.WriteLine ("sudo yum install libtiff-devel");
				writer.WriteLine ("sudo yum install glib*");
				writer.WriteLine ("sudo yum install cairo-devel");


				writer.WriteLine ("#download pkgconfig-0.27.1-4.el7.x86_64.rpm");
				writer.WriteLine ("#and download gdk-pixbuff2-devel");
				writer.WriteLine ("sudo yum install libgdk_pixbuf*");
				writer.WriteLine ("sudo yum install gdk-pixbuf2-devel");
				writer.WriteLine ("sudo yum install sqlite-devel");

				//writer.WriteLine ("sudo yum install libxslt1-dev");
				writer.WriteLine ("wget  http://xmlsoft.org/sources/libxslt-1.1.28.tar.gz");
				writer.WriteLine ("tar -xvzf libxslt-1.1.28.tar.gz");
				writer.WriteLine ("cd libxslt-1.1.28");
				writer.WriteLine ("./configure --with-python=/usr/bin/python2.7");
				writer.WriteLine ("make");
				writer.WriteLine ("sudo make install");
				writer.WriteLine ("cd ..");

				writer.WriteLine ("sudo yum install postgresql");

				//download http://dl.fedoraproject.org/pub/epel/7/x86_64/o/openslide-devel-3.4.1-1.el7.x86_64.rpm
				//writer.WriteLine ("https://github.com/openslide/openslide.git");
				//writer.WriteLine ("cd openslide");
				//writer.WriteLine ("autoreconf -i");
				//writer.WriteLine ("./configure");

				writer.WriteLine ("sudo yum install openslide*");


				//writer.WriteLine ("sudo yum install libopenslide-dev");
				//writer.WriteLine ("sudo yum install libtiff4-dev");
				writer.WriteLine ("sudo yum update");
				writer.WriteLine ("# remember to install setuptools, matlab, configure postgres, mysql");
				writer.WriteLine ("sudo yum install mysql");
				writer.WriteLine ("sudo yum install mysql-devel");
				writer.WriteLine ("sudo yum install postgres*");

				//make sure to use the latest virtualenv package. Some errors have happened while isntalling bisque on Debian 7
				writer.WriteLine ("wget https://bootstrap.pypa.io/ez_setup.py -O - | sudo python"); //setuptools

				writer.WriteLine ("sudo pip install virtualenv --upgrade");
				//writer.WriteLine ("sudo yum install build-essential");
				writer.WriteLine ("sudo yum install libbz2*");

				writer.WriteLine ("sudo yum install java-1.7.0-openjdk");

				writer.WriteLine ("sudo yum install java-1.7.0-openjdk-devel");

				//xorg-x11-xauth
				//xorg-x11-fonts*
				//xorg-x11-utils
				//X11Fordwarding yes
				//sudo yum install xorg-x11-apps
				//yum install libXau-devel
				//yum install libXp
				//yum install libXp-devel

				//Install required packages to compile imgcnv.
				//Note: Thecodec packages (libopenjpeg-dev, libschroedinger-dev, libtheora-dev and libxvidcore-dev) may not be needed for imgcnv in later versions
				//writer.WriteLine ("sudo yum install libopenjpeg-dev");
				writer.WriteLine ("sudo yum install libschroedinger-dev");
				writer.WriteLine ("sudo yum install libtheora-dev");
				writer.WriteLine ("sudo yum install qt4-qmake");
				writer.WriteLine ("sudo yum install qt4-dev-tools");
				writer.WriteLine ("sudo yum install libhdf5-dev");
				writer.WriteLine ("sudo yum install python-psycopg2");
				writer.WriteLine ("sudo yum install libpq-dev");
				writer.WriteLine ("# install setuptools");
				writer.WriteLine ("wget https://bootstrap.pypa.io/ez_setup.py -O - | sudo python");

				writer.WriteLine ("# install bisque");
				writer.WriteLine ("mkdir bisque; cd bisque");
				writer.WriteLine ("wget http://biodev.ece.ucsb.edu/projects/bisquik/export/tip/bisque-stable/contrib/bootstrap/bisque-bootstrap.py");
				writer.WriteLine ("python bisque-bootstrap.py");
			}
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

							if (serverType.Equals ("Debian")) {
								newPackage = "# sudo pip install " + line;
							} else if (serverType.Equals ("Centos")) {
								newPackage = "# sudo pip install " + line;
							}

						} else {
							if (serverType.Equals ("Debian")) {
								newPackage = "sudo pip install " + line;	
							} else if (serverType.Equals ("Centos")) {
								newPackage = "sudo pip install " + line;	
							}
						}

						writer.WriteLine (newPackage);	
					}
				}

			}

			if (serverType.Equals ("Debian")) {
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
			} else if (serverType.Equals ("Centos")) {
				
			}


			// Close the file so it can be accessed again.
			file.Close();
			// Close the writer
			writer.Close();

			Console.WriteLine ("Finished!");
		}
	}
}
