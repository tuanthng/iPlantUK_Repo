using System;
using System.Data;
using System.Xml;
using System.Collections.Generic;
using RootNav.Core;
using System.IO;
using RootNav.Core.MixtureModels;
using System.Xml.XPath;
using RootNav.Core.LiveWires; 

namespace RootNavLinux
{
	public class OutputResultXML
	{
		public static string FullOutputFileName{ get; set; }

		public OutputResultXML ()
		{
		}

		public static void writeInputData(string originFilename, string inputPath, string outputPath, EMConfiguration config)
		{
			//System.IO.Stream s = new FileStream(FullOutputFileName, FileMode.OpenOrCreate);
			//System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer();

			using (XmlWriter writer = XmlWriter.Create(FullOutputFileName))
			{
				writer.WriteStartDocument();
				writer.WriteStartElement("DataProcessed");


				writer.WriteStartElement("Input");

				writer.WriteStartElement("File");
				writer.WriteElementString("ImageFile", originFilename);
				writer.WriteElementString("InputPath", inputPath);
				writer.WriteElementString("OutputPath", outputPath);
				writer.WriteEndElement(); //end File

				writer.WriteStartElement("EMConfiguration");
				writer.WriteElementString("Name", config.Name);
				writer.WriteElementString("InitialClassCount", config.InitialClassCount.ToString());
				writer.WriteElementString("MaximumClassCount", config.MaximumClassCount.ToString());
				writer.WriteElementString("ExpectedRootClassCount", config.ExpectedRootClassCount.ToString());
				writer.WriteElementString("PatchSize", config.PatchSize.ToString());
				writer.WriteElementString("BackgroundPercentage", config.BackgroundPercentage.ToString());
				writer.WriteElementString("BackgroundExcessSigma", config.BackgroundExcessSigma.ToString());
				writer.WriteStartElement("Weights");
				foreach (double w in config.Weights) {
					writer.WriteElementString("double", w.ToString());	
				}
				writer.WriteEndElement(); //end Weights

				writer.WriteEndElement(); //end Configuration

				writer.WriteEndElement(); //end Input

				writer.WriteEndElement(); //end DataProcessed
				writer.WriteEndDocument();
				//closing the file
				writer.Flush ();
				writer.Close ();
			}

		}

		public static void writeTipsDetectedData(string outputFilename, List<Int32Point> tipsDetected)
		{
			//System.IO.Stream s = new FileStream(FullOutputFileName, FileMode.OpenOrCreate);
			//System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer();

			if (File.Exists (FullOutputFileName)) {
				//this code used to query the node from xml file
//				XPathDocument doc = new XPathDocument (FullOutputFileName);
//				XPathNavigator nav = doc.CreateNavigator ();
//
//				//Compile a standard XPath expression
//				XPathExpression expr;
//				expr = nav.Compile ("/DataProcessed"); //find the DataProcessed Node
//				XPathNodeIterator iterator = nav.Select(expr);
//
//				iterator.Current.AppendChild ("New node");

				//this code used to append new node to the existing xml file
				XmlTextReader reader = new XmlTextReader (FullOutputFileName);
				XmlDocument doc = new XmlDocument ();
				doc.Load (reader);
				reader.Close ();

				//select the 1st node
				XmlElement root = doc.DocumentElement;
				XmlNode dataProcessedNode = root.SelectSingleNode ("/DataProcessed");

				XmlNode outputNode = doc.CreateNode (XmlNodeType.Element, "Output", "");

				//File node
				XmlNode fileNode = doc.CreateNode (XmlNodeType.Element, "File", "");
				XmlNode imageFileNode = doc.CreateNode (XmlNodeType.Element, "ProbabilityImageFile", "");
				imageFileNode.InnerText = outputFilename;

				fileNode.AppendChild (imageFileNode);
				outputNode.AppendChild (fileNode);

				//Tip node
				XmlNode tipsNode = doc.CreateNode (XmlNodeType.Element, "TipsDetected", "");

				if (tipsDetected != null) {
					XmlAttribute totalAtt = doc.CreateAttribute("total");
					totalAtt.Value = tipsDetected.Count.ToString ();
					tipsNode.Attributes.Append (totalAtt);

					for(int index  = 0 ; index < tipsDetected.Count; index++) {
						XmlNode point = doc.CreateNode(XmlNodeType.Element, "Tip" , "");
						XmlAttribute idAtt = doc.CreateAttribute("id");
						idAtt.Value = index.ToString ();
						point.Attributes.Append (idAtt);
						XmlAttribute xAtt = doc.CreateAttribute("x");
						xAtt.Value = tipsDetected [index].X.ToString ();
						point.Attributes.Append (xAtt);
						XmlAttribute yAtt = doc.CreateAttribute("y");
						yAtt.Value = tipsDetected [index].Y.ToString ();
						point.Attributes.Append (yAtt);

						tipsNode.AppendChild (point);
					}
				}

				outputNode.AppendChild (tipsNode);

				//patch node


				dataProcessedNode.AppendChild(outputNode);

				//save changes to the file
				doc.Save (FullOutputFileName);
			}

		} //end write tip detected

		public static void writeTipsDetectedDataForBisque(string outputFilename, List<Int32Point> tipsDetected)
		{
			//System.IO.Stream s = new FileStream(FullOutputFileName, FileMode.OpenOrCreate);
			//System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer();

			if (File.Exists (FullOutputFileName)) {
				//this code used to query the node from xml file
				//				XPathDocument doc = new XPathDocument (FullOutputFileName);
				//				XPathNavigator nav = doc.CreateNavigator ();
				//
				//				//Compile a standard XPath expression
				//				XPathExpression expr;
				//				expr = nav.Compile ("/DataProcessed"); //find the DataProcessed Node
				//				XPathNodeIterator iterator = nav.Select(expr);
				//
				//				iterator.Current.AppendChild ("New node");

				//this code used to append new node to the existing xml file
				XmlTextReader reader = new XmlTextReader (FullOutputFileName);
				XmlDocument doc = new XmlDocument ();
				doc.Load (reader);
				reader.Close ();

				//select the 1st node
				XmlElement root = doc.DocumentElement;
				XmlNode dataProcessedNode = root.SelectSingleNode ("/DataProcessed");

				XmlNode outputNode = doc.CreateNode (XmlNodeType.Element, "Output", "");

				//File node
				XmlNode fileNode = doc.CreateNode (XmlNodeType.Element, "File", "");
				XmlNode imageFileNode = doc.CreateNode (XmlNodeType.Element, "ProbabilityImageFile", "");
				imageFileNode.InnerText = outputFilename;

				fileNode.AppendChild (imageFileNode);
				outputNode.AppendChild (fileNode);

				//Tip node
				XmlNode tipsNode = doc.CreateNode (XmlNodeType.Element, "TipsDetected", "");

				if (tipsDetected != null) {
					XmlAttribute totalAtt = doc.CreateAttribute("total");
					totalAtt.Value = tipsDetected.Count.ToString ();
					tipsNode.Attributes.Append (totalAtt);

					XmlNode gObject = doc.CreateNode(XmlNodeType.Element, "gobject" , "");
					XmlAttribute nameAttgObject = doc.CreateAttribute("name");
					nameAttgObject.Value = "PointsDetected";
					gObject.Attributes.Append (nameAttgObject);

					for(int index  = 0 ; index < tipsDetected.Count; index++) {
						XmlNode point = doc.CreateNode(XmlNodeType.Element, "point" , "");
						XmlAttribute idAtt = doc.CreateAttribute("name");
						idAtt.Value = index.ToString ();
						point.Attributes.Append (idAtt);

						XmlNode vertex = doc.CreateNode(XmlNodeType.Element, "vertex" , "");

						XmlAttribute indexAtt = doc.CreateAttribute("index");
						indexAtt.Value = "0";
						vertex.Attributes.Append (indexAtt);

						XmlAttribute xAtt = doc.CreateAttribute("x");
						xAtt.Value = tipsDetected [index].X.ToString ();
						vertex.Attributes.Append (xAtt);
						XmlAttribute yAtt = doc.CreateAttribute("y");
						yAtt.Value = tipsDetected [index].Y.ToString ();
						vertex.Attributes.Append (yAtt);

						XmlAttribute tAtt = doc.CreateAttribute("t");
						tAtt.Value = "0";
						vertex.Attributes.Append (tAtt);

						XmlAttribute zAtt = doc.CreateAttribute("z");
						zAtt.Value = "0";
						vertex.Attributes.Append (zAtt);

						point.AppendChild (vertex);
						gObject.AppendChild (point);

					}
					tipsNode.AppendChild (gObject);
				}

				outputNode.AppendChild (tipsNode);

				//patch node


				dataProcessedNode.AppendChild(outputNode);

				//save changes to the file
				doc.Save (FullOutputFileName);
			}

		} //end write tip detected

		public static void writePrimaryPathsData(LiveWirePathCollection paths)
		{
			if (File.Exists (FullOutputFileName)) {

				//this code used to append new node to the existing xml file
				XmlTextReader reader = new XmlTextReader (FullOutputFileName);
				XmlDocument doc = new XmlDocument ();
				doc.Load (reader);
				reader.Close ();

				//select the 1st node
				XmlElement root = doc.DocumentElement;
				XmlNode dataProcessedNode = root.SelectSingleNode ("/DataProcessed/Output");

				XmlNode primaryPathsNode = doc.CreateNode (XmlNodeType.Element, "PrimaryPaths", "");

				foreach (LiveWirePrimaryPath path in paths.Primaries)
				{
					//Path node
					XmlNode eachPathNode = doc.CreateNode (XmlNodeType.Element, "Path", "");

					foreach (System.Windows.Point point in path.Path) {

						XmlNode pointNode = doc.CreateNode (XmlNodeType.Element, "Point", "");

						XmlAttribute xAtt = doc.CreateAttribute("x");
						xAtt.Value = point.X.ToString ();
						pointNode.Attributes.Append (xAtt);
						XmlAttribute yAtt = doc.CreateAttribute("y");
						yAtt.Value = point.Y.ToString ();
						pointNode.Attributes.Append (yAtt);

						eachPathNode.AppendChild (pointNode);
					} //end for each

					primaryPathsNode.AppendChild (eachPathNode);
				} //end for each

				dataProcessedNode.AppendChild(primaryPathsNode);

				//save changes to the file
				doc.Save (FullOutputFileName);

			} //end if

		} //end write Primary Paths
		public static void writePrimaryPathsDataForBisque(LiveWirePathCollection paths)
		{
			if (File.Exists (FullOutputFileName)) {

				//this code used to append new node to the existing xml file
				XmlTextReader reader = new XmlTextReader (FullOutputFileName);
				XmlDocument doc = new XmlDocument ();
				doc.Load (reader);
				reader.Close ();

				//select the 1st node
				XmlElement root = doc.DocumentElement;
				XmlNode dataProcessedNode = root.SelectSingleNode ("/DataProcessed/Output");

				XmlNode primaryPathsNode = doc.CreateNode (XmlNodeType.Element, "PrimaryPaths", "");

				int index = 0;
				foreach (LiveWirePrimaryPath path in paths.Primaries)
				{
					//LiveWirePrimaryPath path = paths.Primaries [index];


					XmlNode gObjectNode = doc.CreateNode (XmlNodeType.Element, "gobject", "");
					XmlAttribute nameAttgObject = doc.CreateAttribute("name");
					nameAttgObject.Value = index.ToString();
					gObjectNode.Attributes.Append (nameAttgObject);

					//Path node
					XmlNode eachPathNode = doc.CreateNode (XmlNodeType.Element, "polyline", "");
					XmlAttribute nameAtt = doc.CreateAttribute("name");
					nameAtt.Value = index.ToString ();
					eachPathNode.Attributes.Append (nameAtt);

					index ++;

					int totalPoints = path.Path.Count;

					for (int p = 0; p < totalPoints; p++) {
						System.Windows.Point point = path.Path[p];
						XmlNode pointNode = doc.CreateNode (XmlNodeType.Element, "vertex", "");

						XmlAttribute indexAtt = doc.CreateAttribute("index");
						indexAtt.Value = p.ToString();
						pointNode.Attributes.Append (indexAtt);

						XmlAttribute xAtt = doc.CreateAttribute("x");
						xAtt.Value = point.X.ToString ();
						pointNode.Attributes.Append (xAtt);
						XmlAttribute yAtt = doc.CreateAttribute("y");
						yAtt.Value = point.Y.ToString ();
						pointNode.Attributes.Append (yAtt);

						XmlAttribute tAtt = doc.CreateAttribute("t");
						tAtt.Value = "0";
						pointNode.Attributes.Append (tAtt);

						XmlAttribute zAtt = doc.CreateAttribute("z");
						zAtt.Value = "0";
						pointNode.Attributes.Append (zAtt);

						eachPathNode.AppendChild (pointNode);
					}
					//foreach (System.Windows.Point point in path.Path) {
						
					//} //end for each

					gObjectNode.AppendChild (eachPathNode);
					primaryPathsNode.AppendChild (gObjectNode);
				}

				//foreach (LiveWirePrimaryPath path in paths.Primaries)
				//{

				//} //end for each

				dataProcessedNode.AppendChild(primaryPathsNode);

				//save changes to the file
				doc.Save (FullOutputFileName);

			} //end if

		} //end write Primary Paths
		public static void writeLateralPathsData(LiveWirePathCollection paths)
		{
			if (File.Exists (FullOutputFileName)) {

				//this code used to append new node to the existing xml file
				XmlTextReader reader = new XmlTextReader (FullOutputFileName);
				XmlDocument doc = new XmlDocument ();
				doc.Load (reader);
				reader.Close ();

				//select the 1st node
				XmlElement root = doc.DocumentElement;
				XmlNode dataProcessedNode = root.SelectSingleNode ("/DataProcessed/Output");

				XmlNode lateralPathsNode = doc.CreateNode (XmlNodeType.Element, "LateralPaths", "");

				foreach (LiveWireLateralPath path in paths.Laterals)
				{
					//Path node
					XmlNode eachPathNode = doc.CreateNode (XmlNodeType.Element, "Path", "");

					foreach (System.Windows.Point point in path.Path) {

						XmlNode pointNode = doc.CreateNode (XmlNodeType.Element, "Point", "");

						XmlAttribute xAtt = doc.CreateAttribute("x");
						xAtt.Value = point.X.ToString ();
						pointNode.Attributes.Append (xAtt);
						XmlAttribute yAtt = doc.CreateAttribute("y");
						yAtt.Value = point.Y.ToString ();
						pointNode.Attributes.Append (yAtt);

						eachPathNode.AppendChild (pointNode);
					} //end for each

					lateralPathsNode.AppendChild (eachPathNode);
				} //end for each

				dataProcessedNode.AppendChild(lateralPathsNode);

				//save changes to the file
				doc.Save (FullOutputFileName);

			} //end if

		} //end write Primary Paths
		public static void writeLateralPathsDataForBisque(LiveWirePathCollection paths)
		{
			if (File.Exists (FullOutputFileName)) {

				//this code used to append new node to the existing xml file
				XmlTextReader reader = new XmlTextReader (FullOutputFileName);
				XmlDocument doc = new XmlDocument ();
				doc.Load (reader);
				reader.Close ();

				//select the 1st node
				XmlElement root = doc.DocumentElement;
				XmlNode dataProcessedNode = root.SelectSingleNode ("/DataProcessed/Output");

				XmlNode lateralPathsNode = doc.CreateNode (XmlNodeType.Element, "LateralPaths", "");

				//int totalPath = paths.Laterals;
				int index = 0;
				foreach (LiveWireLateralPath path in paths.Laterals)
				{
					//LiveWirePrimaryPath path = paths.Laterals [index];
				
					XmlNode gObjectNode = doc.CreateNode (XmlNodeType.Element, "gobject", "");
					XmlAttribute nameAttgObject = doc.CreateAttribute("name");
					nameAttgObject.Value = index.ToString();
					gObjectNode.Attributes.Append (nameAttgObject);

					//Path node
					XmlNode eachPathNode = doc.CreateNode (XmlNodeType.Element, "polyline", "");
					XmlAttribute nameAtt = doc.CreateAttribute("name");
					nameAtt.Value = index.ToString ();
					eachPathNode.Attributes.Append (nameAtt);

					index ++;

					int totalPoints = path.Path.Count;

					for (int p = 0; p < totalPoints; p++) {
						System.Windows.Point point = path.Path [p];

						XmlNode pointNode = doc.CreateNode (XmlNodeType.Element, "vertex", "");

						XmlAttribute indexAtt = doc.CreateAttribute("index");
						indexAtt.Value = p.ToString();
						pointNode.Attributes.Append (indexAtt);

						XmlAttribute xAtt = doc.CreateAttribute("x");
						xAtt.Value = point.X.ToString ();
						pointNode.Attributes.Append (xAtt);
						XmlAttribute yAtt = doc.CreateAttribute("y");
						yAtt.Value = point.Y.ToString ();
						pointNode.Attributes.Append (yAtt);

						XmlAttribute tAtt = doc.CreateAttribute("t");
						tAtt.Value = "0";
						pointNode.Attributes.Append (tAtt);

						XmlAttribute zAtt = doc.CreateAttribute("z");
						zAtt.Value = "0";
						pointNode.Attributes.Append (zAtt);

						eachPathNode.AppendChild (pointNode);
					}
					//foreach (System.Windows.Point point in path.Path) {

					//} //end for each

					gObjectNode.AppendChild (eachPathNode);
					lateralPathsNode.AppendChild (gObjectNode);
				}

				//foreach (LiveWireLateralPath path in paths.Laterals)
				//{
					
				//} //end for each

				dataProcessedNode.AppendChild(lateralPathsNode);

				//save changes to the file
				doc.Save (FullOutputFileName);

			} //end if

		} //end write Primary Paths
	} //end class
}

