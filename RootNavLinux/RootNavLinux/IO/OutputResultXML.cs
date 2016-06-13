using System;
using System.Data;
using System.Xml;
using System.Collections.Generic;
using RootNav.Core;
using System.IO;
using RootNav.Core.MixtureModels;
using System.Xml.XPath;
using RootNav.Core.LiveWires;
using System.Drawing;
using RootNav.Interface.Controls;
using Emgu.CV;
using RootNav.Core.Measurement;
using System.Linq;
using System.Xml.Linq; 

namespace RootNavLinux
{
	public class OutputResultXML
	{
		public static string FullOutputFileName{ get; set; }

		public OutputResultXML ()
		{
		}

		public static void writeInputData(string originFilename, string inputPath, string outputPath, EMConfiguration config, string inputDataFile)
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
				writer.WriteElementString("InputDataFile", inputDataFile);
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
		public static void writePrimaryPathsDataForBisque(LiveWirePathCollection paths, ScreenOverlayRenderInfo render)
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

				XmlNode primaryPathsNodeOld = root.SelectSingleNode ("/DataProcessed/Output/PrimaryPaths");

				XmlNode primaryPathsNode = null;

				//remove the old node if has
				if (primaryPathsNodeOld == null) 
				{
					primaryPathsNode = doc.CreateNode (XmlNodeType.Element, "PrimaryPaths", "");
				} 
				else 
				{
					primaryPathsNodeOld.RemoveAll ();

					primaryPathsNode = primaryPathsNodeOld;
				}

				int index = 0;
				foreach (LiveWirePrimaryPath path in paths.Primaries)
				{
					//LiveWirePrimaryPath path = paths.Primaries [index];
					Pen rootPen = render.RootPens[index];

					// Render lateral path in source primary colour
					//if (this.paths[i] is LiveWireLateralPath)
					//{
					//	rootPen = renderInfo.RootPens[(this.paths[i] as LiveWireLateralPath).TargetPoint.ParentIndex];
					//}

					XmlNode gObjectNode = doc.CreateNode (XmlNodeType.Element, "gobject", "");
					XmlAttribute nameAttgObject = doc.CreateAttribute("name");
					nameAttgObject.Value = index.ToString();
					gObjectNode.Attributes.Append (nameAttgObject);

					//Path node
					XmlNode eachPathNode = doc.CreateNode (XmlNodeType.Element, "polyline", "");
					XmlAttribute nameAtt = doc.CreateAttribute("name");
					nameAtt.Value = index.ToString ();
					eachPathNode.Attributes.Append (nameAtt);

					//<tag value="#ff0000" name="color" />
					XmlNode colourNode = doc.CreateNode (XmlNodeType.Element, "tag", "");
					XmlAttribute nameAttColourNode = doc.CreateAttribute("name");
					nameAttColourNode.Value = "color";
					colourNode.Attributes.Append (nameAttColourNode);
					XmlAttribute valueAttColourNode = doc.CreateAttribute("value");
					valueAttColourNode.Value = OutputResultXML.convertColourToHexString (rootPen.Color);
					colourNode.Attributes.Append (valueAttColourNode);

					eachPathNode.AppendChild (colourNode);

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

		} //end write Lateral Paths
		public static void writeLateralPathsDataForBisque(LiveWirePathCollection paths, ScreenOverlayRenderInfo render)
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

				XmlNode lateralPathsNodeOld = root.SelectSingleNode ("/DataProcessed/Output/LateralPaths");

				XmlNode lateralPathsNode = null;

				//remove the old node if has
				if (lateralPathsNodeOld == null) 
				{
					lateralPathsNode = doc.CreateNode (XmlNodeType.Element, "LateralPaths", "");
				} 
				else 
				{
					lateralPathsNodeOld.RemoveAll ();

					lateralPathsNode = lateralPathsNodeOld;
				}

				//int totalPath = paths.Laterals;
				int index = 0;
				foreach (LiveWireLateralPath path in paths.Laterals)
				{
					//LiveWirePrimaryPath path = paths.Laterals [index];
					//Pen rootPen = render.RootPens[index];
					Pen rootPen = render.RootPens[path.TargetPoint.ParentIndex];

					XmlNode gObjectNode = doc.CreateNode (XmlNodeType.Element, "gobject", "");
					XmlAttribute nameAttgObject = doc.CreateAttribute("name");
					nameAttgObject.Value = index.ToString();
					gObjectNode.Attributes.Append (nameAttgObject);

					//Path node
					XmlNode eachPathNode = doc.CreateNode (XmlNodeType.Element, "polyline", "");
					XmlAttribute nameAtt = doc.CreateAttribute("name");
					nameAtt.Value = index.ToString ();
					eachPathNode.Attributes.Append (nameAtt);

					//<tag value="#ff0000" name="color" />
					XmlNode colourNode = doc.CreateNode (XmlNodeType.Element, "tag", "");
					XmlAttribute nameAttColourNode = doc.CreateAttribute("name");
					nameAttColourNode.Value = "color";
					colourNode.Attributes.Append (nameAttColourNode);
					XmlAttribute valueAttColourNode = doc.CreateAttribute("value");
					valueAttColourNode.Value = OutputResultXML.convertColourToHexString (rootPen.Color);
					colourNode.Attributes.Append (valueAttColourNode);

					eachPathNode.AppendChild (colourNode);

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
		} //end write Lateral Paths

		public static string convertColourToHexString(Color colour)
		{
			//return "#" + colour.R.ToString ("X") + colour.G.ToString ("X") + colour.B.ToString ("X");
			return System.Drawing.ColorTranslator.ToHtml(colour);
		}

		public static Color convertHexStringToColor(string hex)
		{
			//hex string has a format: #FFDF0A
			//System.Drawing.ColorConverter c = new ColorConverter();
			//return c.ConvertFromString(hex);
			return System.Drawing.ColorTranslator.FromHtml (hex);
		}

		public static void writeMatToFile(string filename, Mat data)
		{
			FileStorage fs = new FileStorage (filename, FileStorage.Mode.Write);

			fs.Write (data, "MyData");
			fs.ReleaseAndGetString (); //need this or not?
		}

		public static void readMatFromFile(string filename, ref Mat data)
		{
			FileStorage fs = new FileStorage (filename, FileStorage.Mode.Read);

			FileNode dataNode = fs.GetNode ("MyData");
			dataNode.ReadMat (data);

			fs.ReleaseAndGetString ();
		}

		public static void write1DArrayToFile(string filename, double[] data)
		{
			File.WriteAllText(filename, String.Join (",", data));
		}

		public static void read1DArrayFromFile(string filename, ref double[] data)
		{
			string values = File.ReadAllText (filename);

			values.Replace (" ", "");

			var splitted = values.Split(new []{","}, StringSplitOptions.RemoveEmptyEntries);

			data = new double[splitted.Length];

			int index = 0;
			foreach (string s in splitted) {
				double d;
				Double.TryParse (s, out d);
				data[index] = d;
			}
		}

		public static void write2DArrayToFile(string filename, double[,] data, int rows, int cols)
		{
			string lines = "";

			for (int r = 0; r < rows; r++) 
			{
				for (int c = 0; c < cols; c++) 
				{
					lines = lines + data [r, c].ToString () + ",";
				}
				lines = lines.TrimEnd(',');
				lines = lines + Environment.NewLine;
			}
			File.WriteAllText (filename, lines);
		}

		public static void read2DArrayFromFile(string filename, ref double[,] data)
		{
			string []lines = File.ReadAllLines (filename);

			int totalLines = lines.Length;

			for (int l = 0; l < totalLines; l++) 
			{
				lines[l].Replace (" ", "");

				var splitted = lines[l].Split(new []{","}, StringSplitOptions.RemoveEmptyEntries);

				if (l == 0) 
				{
					data = new double[totalLines, splitted.Length];	
				}

				int index = 0;
				foreach (string s in splitted) 
				{
					double d;
					Double.TryParse (s, out d);
					data[l, index] = d;
				}
			}
		}

		public static void write1DArrayToFile(string filename, byte[]  data)
		{
			File.WriteAllText(filename, String.Join (",", data));
		}

		public static void read1DArrayFromFile(string filename, ref byte[] data)
		{
			string values = File.ReadAllText (filename);

			values.Replace (" ", "");

			var splitted = values.Split(new []{","}, StringSplitOptions.RemoveEmptyEntries);

			data = new byte[splitted.Length];

			int index = 0;
			foreach (string s in splitted) 
			{
				Byte d;
				Byte.TryParse (s, out d);
				data[index] = d;
			}
		} //end read1DArrayFromFile

		public static void writeRootDataForBisque(RootBase root, XmlDocument doc, ref XmlNode parent, ScreenOverlayRenderInfo render)
		{
			XmlNode rootNode = doc.CreateNode (XmlNodeType.Element, "Root", "");
			XmlAttribute orderAtt = doc.CreateAttribute("order");
			orderAtt.Value = root.Order.ToString();
			rootNode.Attributes.Append (orderAtt);

			XmlAttribute lengthAtt = doc.CreateAttribute("length");
			lengthAtt.Value = root.Length.ToString();
			rootNode.Attributes.Append (lengthAtt);

			//root.AngleMaximumDistance
			//root.AngleMinimumDistance
			//root.AngleWithParent
			//root.Color
			//root.ConvexHullArea
			//root.EmergenceAngle
			//root.EmergenceVector
			//root.End
			//root.Start
			//root.RootIndex
			//root.ID
			//root.InnerTipPoint
			//root.Label
			//root.Length
			//root.MaximumAnglePoint
			//root.MinimumAnglePoint
			//root.Parent
			//root.ParentVector
			//root.ParentVectorPoints
			//root.PixelConvexHullArea
			//root.PixelLength
			//root.PixelStartDistance
			//root.PrimaryParent
			//root.RelativeID
			//root.RootIndex
			//root.StartDistance
			//root.StartReference
			//root.TipAngle
			//root.TipAngleDistance
			//root.TipVector
			//root.TotalAngle
			//root.TotalVector
			//root.UnitConversionFactor

			if (root.Order < 0) 
			{
				if (root.ConvexHullPoints != null) 
				{
					XmlNode convexHullNode = doc.CreateNode (XmlNodeType.Element, "ConvexHull", "");

					//gobject node
					XmlNode gObjectNode = doc.CreateNode (XmlNodeType.Element, "gobject", "");
					XmlAttribute nameAttgObject = doc.CreateAttribute("name");
					nameAttgObject.Value = "0";
					gObjectNode.Attributes.Append (nameAttgObject);

					convexHullNode.AppendChild (gObjectNode);

					//polyline node
					XmlNode polylineNode = doc.CreateNode (XmlNodeType.Element, "polygon", "");
					XmlAttribute nameAtt = doc.CreateAttribute("name");
					nameAtt.Value = "0";
					polylineNode.Attributes.Append (nameAtt);

					//<tag value="#ff0000" name="color" />
					XmlNode colourNode = doc.CreateNode (XmlNodeType.Element, "tag", "");
					XmlAttribute nameAttColourNode = doc.CreateAttribute("name");
					nameAttColourNode.Value = "color";
					colourNode.Attributes.Append (nameAttColourNode);
					XmlAttribute valueAttColourNode = doc.CreateAttribute("value");
					valueAttColourNode.Value = OutputResultXML.convertColourToHexString (Color.WhiteSmoke);
					colourNode.Attributes.Append (valueAttColourNode);

					polylineNode.AppendChild (colourNode);


					gObjectNode.AppendChild (polylineNode);

					int count = root.ConvexHullPoints.Count;

					for (int j = 0; j < count; j++)
					{
						Int32Point point = root.ConvexHullPoints[j];

						XmlNode pointNode = doc.CreateNode (XmlNodeType.Element, "vertex", "");

						XmlAttribute indexAtt = doc.CreateAttribute("index");
						indexAtt.Value = j.ToString();
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

						polylineNode.AppendChild (pointNode);
					}

					//convex hull
					rootNode.AppendChild (convexHullNode);
				}

				//other stuff
				XmlAttribute areaAtt = doc.CreateAttribute("area");
				areaAtt.Value = root.ConvexHullArea.ToString();
				rootNode.Attributes.Append (areaAtt);

				XmlAttribute primaryRootsAtt = doc.CreateAttribute("primaryRoots");
				primaryRootsAtt.Value = root.Children.Count.ToString();
				rootNode.Attributes.Append (primaryRootsAtt);
			}

			//Spline
			if (root.Order >= 0) 
			{
				Pen rootPen = render.RootPens [0];

				if (root.PrimaryParent == null || root.PrimaryParent.Order == -1) 
				{
					rootPen = render.RootPens [root.RootIndex];
					
					if (root.IsHighlighted || root.IsSelected)
						rootPen = render.HighlightedRootPens [root.RootIndex];
				} else 
				{
					rootPen = render.RootPens [root.PrimaryParent.RootIndex];
					
					if (root.IsHighlighted || root.IsSelected)
						rootPen = render.HighlightedRootPens [root.PrimaryParent.RootIndex];
				}

				System.Windows.Point[] points = root.Spline.SampledPoints;
				int count = points.Length;

				XmlNode splineNode = doc.CreateNode (XmlNodeType.Element, "Spline", "");

				if (count > 0) 
				{
					//gobject node
					XmlNode gObjectNode = doc.CreateNode (XmlNodeType.Element, "gobject", "");
					XmlAttribute nameAttgObject = doc.CreateAttribute("name");
					nameAttgObject.Value = "0";
					gObjectNode.Attributes.Append (nameAttgObject);

					splineNode.AppendChild (gObjectNode);

					//polyline node
					XmlNode polylineNode = doc.CreateNode (XmlNodeType.Element, "polyline", "");
					XmlAttribute nameAtt = doc.CreateAttribute("name");
					nameAtt.Value = "0";
					polylineNode.Attributes.Append (nameAtt);

					gObjectNode.AppendChild (polylineNode);

					for (int j = 0; j < count; j++)
					{
						System.Windows.Point point = points[j];

						XmlNode pointNode = doc.CreateNode (XmlNodeType.Element, "vertex", "");

						XmlAttribute indexAtt = doc.CreateAttribute("index");
						indexAtt.Value = j.ToString();
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

						polylineNode.AppendChild (pointNode);

					}
				}

				rootNode.AppendChild (splineNode);

				if (root.Order == 0) {
					XmlAttribute lateralAtt = doc.CreateAttribute("laterals");
					lateralAtt.Value = root.Children.Count.ToString();
					rootNode.Attributes.Append (lateralAtt);
				} //else {
					
				//}

				XmlAttribute emergenceAngleAtt = doc.CreateAttribute("emergenceAngle");
				emergenceAngleAtt.Value = root.EmergenceAngle.ToString();
				rootNode.Attributes.Append (emergenceAngleAtt);

				XmlAttribute tipAngleAtt = doc.CreateAttribute("tipAngle");
				tipAngleAtt.Value = root.TipAngle.ToString();
				rootNode.Attributes.Append (tipAngleAtt);
			}

			parent.AppendChild (rootNode);

			//recursive for child trees
			foreach (RootBase child in root.Children)
			{
				writeRootDataForBisque(child, doc, ref rootNode, render);
			}
		}

		public static void writeRootDataForBisque(RootCollection roots, ScreenOverlayRenderInfo render)
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

				XmlNode rootBaseNode = doc.CreateNode (XmlNodeType.Element, "RootTree", "");

				foreach(RootBase r in roots)
				{
					writeRootDataForBisque (r, doc, ref rootBaseNode, render);
				}

				dataProcessedNode.AppendChild(rootBaseNode);

				//save changes to the file
				doc.Save (FullOutputFileName);
			}
		}

		public static void writeMeasurementData(RootCollection roots, ScreenOverlayRenderInfo render, string tag, 
								bool doMeasureTable, bool doCurvatureProfile, 
								bool doMapProfile, int travel, double[] probabilityMapSecondClass, int width, int height)
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

				XmlNode measurementNode = doc.CreateNode (XmlNodeType.Element, "Measurement", "");

				if (doMeasureTable) 
				{
					//table
					XmlNode tablesNode = doc.CreateNode (XmlNodeType.Element, "Tables", "");

					XmlDocument xD = new XmlDocument();
					foreach (Dictionary<string, string> data in RootMeasurement.GetDataAsStrings(roots.RootTree.ToList()))
					{
						data.Add("Tag", tag);

						//write to xml file, Code: Dictionary to Element using XML.LINQ
						XElement el = new XElement("Table", data.Select(kv => new XElement(kv.Key.Replace(' ', '_'), kv.Value)));
						//convert XElement to XmlNode
						xD.LoadXml(el.ToString());

						XmlNode xN = doc.ImportNode (xD.FirstChild, true);

						tablesNode.AppendChild (xN);
					}

					//				Code: Element to Dictionary:
					//
					//				XElement rootElement = XElement.Parse("<root><key>value</key></root>");
					//				Dictionary<string, string> dict = new Dictionary<string, string>();
					//				foreach(var el in rootElement.Elements())
					//				{
					//					dict.Add(el.Name.LocalName, el.Value);
					//				}

					measurementNode.AppendChild (tablesNode);
				}

				//curvature profile
				if (doCurvatureProfile) 
				{
					//TableWindow tw = new TableWindow();

					var data = RootMeasurement.GetCurvatureProfiles(roots.RootTree.ToList(), 4);

					double minDistance = double.MinValue;
					int rowCount = 0;
					foreach (var v in data)
					{
						if (v.Value != null && v.Value.Length > rowCount)
						{
							if (v.Value[0].Item1 > minDistance)
							{
								minDistance = v.Value[0].Item1;
							}

							rowCount = v.Value.Length;
						}
					}

					string[,] outputArray = new string[data.Count, rowCount + 1];

					int i = 1;
					foreach (var keyValuePair in data)
					{
						if (keyValuePair.Value != null /*&& keyValuePair.Value.Length > 0*/)
						{
							// Header
							outputArray[i, 0] = keyValuePair.Key.RelativeID;

							// Data
							var distanceAnglePair = keyValuePair.Value;
							if (distanceAnglePair.Length == rowCount)
							{
								for (int j = 0; j < distanceAnglePair.Length; j++)
								{
									outputArray[i, j + 1] = Math.Round(distanceAnglePair[j].Item2,1).ToString();
									outputArray[0, j + 1] = Math.Round(distanceAnglePair[j].Item1).ToString();
								}

							}
							else
							{
								for (int j = 0; j < distanceAnglePair.Length; j++)
								{
									outputArray[i, j + 1] = Math.Round(distanceAnglePair[j].Item2, 1).ToString();
								}
							}

							i++;
						}

					}

					// Turn array into data table
					DataTable dt = new DataTable();
					int nbColumns = outputArray.GetLength(0);
					int nbRows = outputArray.GetLength(1);
					for (i = 0; i < nbColumns; i++)
					{
						dt.Columns.Add("col" + i.ToString(), typeof(string));
					}

					for (int row = 0; row < nbRows; row++)
					{
						DataRow dr = dt.NewRow();
						for (int col = 0; col < nbColumns; col++)
						{
							dr[col] = outputArray[col,row];
						}
						dt.Rows.Add(dr);
					}

					//tw.measurementsView.ItemsSource = dt.DefaultView;

					//tw.Show();

					//Convert DataTable to xml node. using LINQ. Remember to add a reference to DatasetExtension dll
					XmlDocument xD = new XmlDocument();

					XElement cur = new XElement("CurvatureProfile", 
						dt.AsEnumerable().Select(row =>
							new XElement("row",
								dt.Columns.Cast<DataColumn>().Select(col =>
									new XAttribute(col.ColumnName, row[col])
								)
							)
						));
					
					xD.LoadXml(cur.ToString());

					XmlNode xN = doc.ImportNode (xD.FirstChild, true);

					measurementNode.AppendChild (xN);
				}

				//map profile
				if (doMapProfile) 
				{
					//TableWindow tw = new TableWindow();

					Dictionary<RootBase, Tuple<double, double>[]> leftData, rightData;

					RootMeasurement.GetMapProfiles(roots.RootTree.ToList(), out leftData, out rightData,
													4,
													travel,
													probabilityMapSecondClass,
													width,
													height);

					double minDistance = double.MinValue;
					int rowCount = 0;

					// Left
					foreach (var v in leftData)
					{
						if (v.Value != null && v.Value.Length > rowCount)
						{
							if (v.Value[0].Item1 > minDistance)
							{
								minDistance = v.Value[0].Item1;
							}

							rowCount = v.Value.Length;
						}
					}

					// Right
					foreach (var v in rightData)
					{
						if (v.Value != null && v.Value.Length > rowCount)
						{
							if (v.Value[0].Item1 > minDistance)
							{
								minDistance = v.Value[0].Item1;
							}

							rowCount = v.Value.Length;
						}
					}


					string[,] outputArray = new string[leftData.Count + rightData.Count - 1, rowCount + 1];

					int i = 1;
					foreach (var keyValuePair in leftData)
					{
						if (keyValuePair.Value != null)
						{
							// Header
							outputArray[i, 0] = keyValuePair.Key.RelativeID + " L";

							// Left Data
							var distanceMapPair = keyValuePair.Value;
							if (distanceMapPair.Length == rowCount)
							{
								for (int j = 0; j < distanceMapPair.Length; j++)
								{
									outputArray[i, j + 1] = Math.Round(distanceMapPair[j].Item2, 1).ToString();
									outputArray[0, j + 1] = Math.Round(distanceMapPair[j].Item1).ToString();
								}

							}
							else
							{
								for (int j = 0; j < distanceMapPair.Length; j++)
								{
									outputArray[i, j + 1] = Math.Round(distanceMapPair[j].Item2, 1).ToString();
								}
							}

							i+=2;
						}

					}

					i = 2;
					foreach (var keyValuePair in rightData)
					{
						if (keyValuePair.Value != null)
						{
							// Header
							outputArray[i, 0] = keyValuePair.Key.RelativeID + " R";

							// Left Data
							var distanceMapPair = keyValuePair.Value;

							for (int j = 0; j < distanceMapPair.Length; j++)
							{
								outputArray[i, j + 1] = Math.Round(distanceMapPair[j].Item2, 1).ToString();
							}

							i += 2;
						}
					}

					// Turn array into data table
					DataTable dt = new DataTable();
					int nbColumns = outputArray.GetLength(0);
					int nbRows = outputArray.GetLength(1);
					for (i = 0; i < nbColumns; i++)
					{
						dt.Columns.Add("col" + i.ToString(), typeof(string));
					}

					for (int row = 0; row < nbRows; row++)
					{
						DataRow dr = dt.NewRow();
						for (int col = 0; col < nbColumns; col++)
						{
							dr[col] = outputArray[col, row];
						}
						dt.Rows.Add(dr);
					}

					//tw.measurementsView.ItemsSource = dt.DefaultView;

					//tw.Show();

					//Convert DataTable to xml node. using LINQ. Remember to add a reference to DatasetExtension dll
					XmlDocument xD = new XmlDocument();

					XElement cur = new XElement("MapProfile", 
						dt.AsEnumerable().Select(row =>
							new XElement("row",
								dt.Columns.Cast<DataColumn>().Select(col =>
									new XAttribute(col.ColumnName, row[col])
								)
							)
						));

					xD.LoadXml(cur.ToString());

					XmlNode xN = doc.ImportNode (xD.FirstChild, true);

					measurementNode.AppendChild (xN);
				}

				dataProcessedNode.AppendChild(measurementNode);

				//save changes to the file
				doc.Save (FullOutputFileName);
			}
		}

		public static void writeRSML(string rsmlDir, string rsmlFile)
		{
			if (File.Exists (FullOutputFileName)) {

				//this code used to append new node to the existing xml file
				XmlTextReader reader = new XmlTextReader (FullOutputFileName);
				XmlDocument doc = new XmlDocument ();
				doc.Load (reader);
				reader.Close ();

				//select the 1st node
				XmlElement root = doc.DocumentElement;
				XmlNode dataProcessedNode = root.SelectSingleNode ("/DataProcessed/Input/File");

				XmlNode rmslPathNode = doc.CreateNode (XmlNodeType.Element, "RSMLPath", "");
				rmslPathNode.InnerText = rsmlDir;

				XmlNode rmslFileNode = doc.CreateNode (XmlNodeType.Element, "RSMLFile", "");
				rmslFileNode.InnerText = rsmlFile;

				dataProcessedNode.AppendChild(rmslPathNode);
				dataProcessedNode.AppendChild(rmslFileNode);

				//save changes to the file
				doc.Save (FullOutputFileName);

			} //end if
		} //end write Lateral Paths
	} //end class
}

