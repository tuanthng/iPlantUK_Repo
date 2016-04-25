using System;
using System.Data;
using System.Xml;
using System.Collections.Generic;
using RootNav.Core;
using System.IO;
using RootNav.Core.MixtureModels;
using System.Xml.XPath; 

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

		public static void writeOutputData(string outputFilename, List<Int32Point> tipsDetected)
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
				XmlNode imageFileNode = doc.CreateNode (XmlNodeType.Element, "ImageFile", "");
				imageFileNode.InnerText = outputFilename;

				fileNode.AppendChild (imageFileNode);

				//Tip node

				//patch node

				outputNode.AppendChild (fileNode);
				dataProcessedNode.AppendChild(outputNode);

				//save changes to the file
				doc.Save (FullOutputFileName);
			}

		}
	}
}

