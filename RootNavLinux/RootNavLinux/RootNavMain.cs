﻿using System;
using RootNav;
using RootNav.Core;
using RootNav.Core.Imaging;
using RootNav.Core.MixtureModels;
using RootNav.Core.Threading;
using RootNav.IO;
using RootNav.Core.Tips;

using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using RootNav.Core.LiveWires;
using RootNav.Interface.Controls;

namespace RootNavLinux
{
	public class RootNavMain
	{
		public static double BackgroundPenalty = 0.1;

		public delegate void StatusTextUpdateDelegate(String s);
		//public delegate void ScreenImageUpdateDelegate(WriteableBitmap wbmp);
		public delegate void EMCompletedDelegate();
		public delegate void LiveWireLateralCompletedDelegate(List<LiveWireLateralPath> paths);
		public delegate void LiveWirePrimaryCompletedDelegate(List<LiveWirePrimaryPath> paths);
		public delegate void LiveWireReCompletedDelegate();

		private EMManagerThread emManager = null;
		private byte[] intensityBuffer = null;
		private GaussianMixtureModel highlightedMixture = null;
		private LiveWirePrimaryManager primaryLiveWireManager = null;
		private LiveWireLateralManager lateralLiveWireManager = null;

		private EMConfiguration[] configurations;
		private EMConfiguration customConfiguration = null;
		private int currentEMConfiguration = 0;

		public string PresetRootName{ get; set; }

		public EMConfiguration CustomEMConfiguration
		{
			set{
				customConfiguration = value;
			}
		}

//		private bool connectionExists = false;

//		RootNav.Data.IO.Databases.DatabaseManager databaseManager = null;
//		private SceneMetadata.ImageInfo imageInfo = null;
//		private WriteableBitmap featureBitmap = null;
		private Mat featureBitmap = null;
//		private WriteableBitmap sourceBitmap = null;
		private Mat sourceBitmap = null;
//		private WriteableBitmap probabilityBitmap = null;
		private Mat probabilityBitmap = null;
//		private bool loadImageZoomOnce = false;

		private double[] probabilityMapBestClass = null;
		private double[] probabilityMapBrightestClass = null;
		private double[] probabilityMapSecondClass = null;
		private double[,] distanceProbabilityMap = null;

		private RootDetectionScreenOverlay screenOverlay = null;

		private LiveWireGraph currentGraph = null;

		private List<LiveWireWeightDescriptor> baseWeightDescriptors = null;

		public List<LiveWireWeightDescriptor> BaseWeightDescriptors
		{
			get { return baseWeightDescriptors; }
			set { baseWeightDescriptors = value; }
		}

		private RootTerminalCollection terminalCollection = new RootTerminalCollection();

		private string ImageFileName { get; set; }
		private string ResultXMLFileName{ get; set; } //the xml file containing result processed
		public string OutputPath{ get; set; } //output and input path will be passed from outside. By default, they should be the current directory of the program
		public string InputPath{ get; set; }

		private string ProbabilityFilename{ get; set; }

		public RootNavMain (string filePathImg)
		{
			this.ImageFileName = filePathImg;

			initConfiguration ();
			createResultFilename ();
			createProbabilityFilename ();

			//store the xml file into the global
			OutputResultXML.FullOutputFileName = ResultXMLFileName;
		}

		public void Process()
		{
			LoadImage (this.ImageFileName);

			EMProcessing ();

			//writing input data
			//OutputResultXML.writeInputData(ImageFileName, this.InputPath, this.OutputPath, this.emManager.Configuration);

			//OutputResultXML.writeOutputData (this.ProbabilityFilename, null);


		}
		private void createResultFilename()
		{
			
			ResultXMLFileName = this.ImageFileName + "_result.xml";

		}
		private void createProbabilityFilename()
		{
			string name = System.IO.Path.GetFileNameWithoutExtension (this.ImageFileName);
			ProbabilityFilename = name + "_map.png";
		}
		private int initConfiguration()
		{
			try
			{
				//intialise the input/output as the current directory
				//InputPath = System.IO.Directory.GetCurrentDirectory();
				InputPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
				OutputPath = InputPath;

				this.configurations = EMConfiguration.LoadFromXML();

				this.currentEMConfiguration = EMConfiguration.DefaultIndex(configurations);

			}
			catch(Exception e)
			{
				Console.WriteLine("Configuration XML Error: An invalid value has been found in the E-M configuration XML file. Please correct this before running RootNav.");
				//Application.Current.Shutdown();
				throw e;
				//if error
				//return -1;
			}

			return 0;
		}

		private Mat LoadImage(string filePath)
		{
			Mat img = null;

			try
			{
				img = CvInvoke.Imread(filePath, Emgu.CV.CvEnum.ImreadModes.AnyColor);

				//for testing
				//Console.WriteLine(filePath);
				//Console.WriteLine(img.NumberOfChannels.ToString());
				//Console.WriteLine(img.Cols.ToString() + " " + img.Rows.ToString());
				//ImageConverter.DisplayImage(img, "Origin", false);
				//convert to gray scale
				//Mat gray = ImageConverter.ConvertGrayScaleImage(img);
				//Console.WriteLine(gray.Cols.ToString());
				//ImageConverter.DisplayImage(gray, "Gray");

				//byte[] data = ImageConverter.ConvertMatToByteArray(ref img);

				//Console.WriteLine(data.Length.ToString());

				//Mat newImg = ImageConverter.ConvertByteArrayToMat(ref data, img.Cols, img.Rows, img.NumberOfChannels, img.Depth);
				//ImageConverter.DisplayImage(newImg, "Getting back", true);

				//TODO: do we need to convert images loaded into 32 bits?


				sourceBitmap = img;
			}
			catch(Exception ex)
			{
				Console.WriteLine (ex.Message);

				throw ex;
			}

			return img;
		}

		private void LoadImages(params string[] filePaths)
		{
			try
			{
				String filePath = filePaths[0];
				this.ImageFileName = System.IO.Path.GetFileNameWithoutExtension(filePath);


//				// For TIF files, extract header information
//				if (System.IO.Path.GetExtension(filePath) == ".tif"
//					|| System.IO.Path.GetExtension(filePath) == ".tiff")
//				{
//					imageHeaderInfo = TiffHeaderDecoder.ReadHeaderInfo(filePath);
//				}
//
//				BitmapImage bmp = new BitmapImage();
//
//				using (FileStream fs = new FileStream(filePath, FileMode.Open))
//				{
//					bmp.BeginInit();
//					bmp.CacheOption = BitmapCacheOption.OnLoad;
//					bmp.StreamSource = fs;
//					bmp.EndInit();
//				}
//				bmp.Freeze();

				//WriteableBitmap wbmp = new WriteableBitmap(bmp);

				//imageInfo.Dpi = wbmp.DpiX;

//				if (wbmp.Format == PixelFormats.Indexed8 || wbmp.Format == PixelFormats.Bgra32 || wbmp.Format == PixelFormats.Gray8)
//					wbmp = IO.ImageConverter.ConvertTo32bpp(wbmp);
//
//				if (wbmp.PixelWidth * wbmp.PixelHeight > 7000000) // 7MP images will likely cause out of memory exceptions. They are also unnecessarily large for root detection
//				{
//					MessageBox.Show("Images of over 7 megapixels are too large to be processed efficiently. Images around 5 megapixels are recommended.", "Image too large.");
//					this.StartScreenLabel.Visibility = System.Windows.Visibility.Visible;
//					this.imageInfo = null;
//					return;
//				}              
//
//				sourceBitmap = wbmp;
			}
			catch
			{
//				this.StartScreenLabel.Visibility = System.Windows.Visibility.Visible;
//				MessageBox.Show("Please select a valid image file");
			}


			//EMProcessing();
		}

		private void EMProcessing()
		{
			Console.WriteLine ("EMProcessing");

			//WriteableBitmap greybmp = Core.Imaging.ImageProcessor.MakeGreyscale32bpp(sourceBitmap);
			Mat grayImg = ImageConverter.ConvertGrayScaleImage(sourceBitmap);

			// Intensity Buffer
//			intensityBuffer = RootNav.IO.ImageConverter.WriteableBitmapToIntensityBuffer(greybmp);
			intensityBuffer = ImageConverter.ConvertMatToByteArray(ref grayImg);
//
//			int width = greybmp.PixelWidth;
//			int height = greybmp.PixelHeight;
			int width = grayImg.Cols;
			int height = grayImg.Rows;
//
//			// Initial custom E-M config?
//			if (this.EMDetectionToolbox.emPresetComboBox.SelectedIndex >= this.configurations.Length)
//			{
//				try
//				{
//					this.customConfiguration = this.EMDetectionToolbox.ToEMConfiguration();
//				}
//				catch
//				{
//					// Error creating EM configuration
//					MessageBox.Show("An invalid value has been found in the E-M configuration. Please correct this then apply E-M again.", "Configuration Error");
//					this.customConfiguration = null;
//					return;
//				}
//			}
//			else
//			{
			//TODO: for now, just use default configuration
				this.customConfiguration = null;
//			}


			this.emManager = new EMManagerThread()
			{
				IntensityBuffer = intensityBuffer,
				Width = width,
				Height = height,
				Configuration = customConfiguration == null ? configurations[currentEMConfiguration] : customConfiguration,
				ThreadCount = RootNav.Core.Threading.ThreadParams.EMThreadCount
			};

			//int GMMArrayWidth = (int)Math.Ceiling(width / (double)emManager.Configuration.PatchSize);
			//int GMMArrayHeight = (int)Math.Ceiling(height / (double)emManager.Configuration.PatchSize);

			//Mat smaller = new Mat(new System.Drawing.Size (500, 600), grayImg.Depth, grayImg.NumberOfChannels);
			//CvInvoke.Resize (grayImg, smaller, smaller.Size);


			//GaussianMixtureModel[,] GMMArray = new GaussianMixtureModel[GMMArrayWidth, GMMArrayHeight];

			this.emManager.ProgressChanged += new ProgressChangedEventHandler(EMManagerProgressChanged);
			this.emManager.ProgressCompleted += new RunWorkerCompletedEventHandler(EMManagerProgressCompleted);

//			this.UpdateStatusText("Status: Processing " + (GMMArrayHeight * GMMArrayWidth).ToString() + " patches");

			//store some input value to the result xml file
			OutputResultXML.writeInputData(ImageFileName, this.InputPath, this.OutputPath, this.emManager.Configuration);

			this.emManager.Run();
//
//			this.StartScreenLabel.Visibility = System.Windows.Visibility.Hidden;
//
//			this.screenOverlay.IsBusy = true;
//
//			this.DetectionPanel.IsEnabled = false;
//			this.preMeasurementToolbox.MeasurementButton.IsEnabled = false;

		} //end EMProcessing

		private void EMManagerProgressChanged(object sender, ProgressChangedEventArgs args)
		{
			//this.Dispatcher.Invoke(new TaskProgressChangedHandler(TaskProgressChanged), new object[] { args.ProgressPercentage });
			Console.WriteLine ("EMManagerProgressChanged of RootNavMain");
		}

		/*unsafe*/ private void EMManagerProgressCompleted(object sender, RunWorkerCompletedEventArgs args)
		{
			//this.Dispatcher.Invoke(new StatusTextUpdateDelegate(this.UpdateStatusText), "Status: Rendering Image");

			Console.WriteLine ("EMManagerProgressCompleted of RootNavMain");

//			WriteableBitmap wbmp = new WriteableBitmap(this.emManager.Width, this.emManager.Height, 96.0, 96.0, PixelFormats.Bgr32, null);
			Mat wbmp = new Mat(this.emManager.Height, this.emManager.Width, Emgu.CV.CvEnum.DepthType.Cv32S, 3);
//			this.featureBitmap = new WriteableBitmap(this.emManager.Width, this.emManager.Height, 96.0, 96.0, PixelFormats.Gray8, null);
			this.featureBitmap = new Mat(this.emManager.Height, this.emManager.Width, Emgu.CV.CvEnum.DepthType.Cv8U, 1);

			this.probabilityMapBestClass = new double[this.emManager.Width * this.emManager.Height];
			this.probabilityMapBrightestClass = new double[this.emManager.Width * this.emManager.Height];
			this.probabilityMapSecondClass = new double[this.emManager.Width * this.emManager.Height];
//			this.screenOverlay.PatchSize = this.emManager.Configuration.PatchSize;
//

			Console.WriteLine ("Total MM: " + this.emManager.Mixtures.Count.ToString());

			Image<Bgr, Byte> imgScreen = wbmp.ToImage<Bgr, Byte> ();
			Image<Gray, Byte> featureScreen = this.featureBitmap.ToImage<Gray, Byte> ();

			foreach (KeyValuePair<EMPatch, GaussianMixtureModel> Pair in this.emManager.Mixtures)
			{
				//Console.WriteLine ("foreach (KeyValuePair");
				EMPatch currentPatch = Pair.Key;
				GaussianMixtureModel currentModel = Pair.Value;
				currentModel.CalculateBounds();

				UpdateImageOnPatchChange(currentPatch, currentModel, ref imgScreen, ref featureScreen);
			}
			//TODO: for testing. should be removed later on.
			//imgScreen.Save("imgScreen.png");

//			if (wbmp.CanFreeze)
//			{
//				wbmp.Freeze();
//			}
//
//			if (featureBitmap.CanFreeze)
//			{
//				featureBitmap.Freeze();
//			}
//
//			this.screenOverlay.IsBusy = false;
//
			//this.probabilityBitmap = wbmp;
			//this.probabilityBitmap = imgScreen.Mat;
			this.probabilityBitmap = new Mat(imgScreen.Mat, imgScreen.ROI);
			//this.featureBitmap = featureScreen.Mat; //changed to below code, because it causes error when later time using this variable. Not sure why?
			this.featureBitmap = new Mat(featureScreen.Mat, featureScreen.ROI);

			this.distanceProbabilityMap = DistanceMap.CreateDistanceMap(featureBitmap);

			//TODO: testing
			//featureBitmap.Save ("FeatureMapInMain.png");

//			*/
//
//
//            int w = featureBitmap.PixelWidth;
//            int h = featureBitmap.PixelHeight;
//
//            System.Drawing.Bitmap b = new System.Drawing.Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
//
//            for (int x = 0; x < w; x++)
//            {
//                for (int y = 0; y < h; y++)
//                {
//                    int col = (int)(distanceProbabilityMap[x, y] * 255);
//                    b.SetPixel(x, y, System.Drawing.Color.FromArgb(col, col, col));
//                }
//            }
//
//            b.Save("C:\\Users\\mpound\\Desktop\\distanceweights.bmp");
//            */

			//this.Dispatcher.Invoke(new ScreenImageUpdateDelegate(this.UpdateScreenImageAndZoom), wbmp);
			//this.Dispatcher.Invoke(new EMCompletedDelegate(this.EMTaskCompleted), null);

			BeginTipDetection();
		}
		unsafe private void UpdateImageOnPatchChange(EMPatch patch, GaussianMixtureModel model, ref Image<Bgr, Byte> screenBitmap, ref Image<Gray, Byte>  featureBitmap)
		{

			//Console.WriteLine ("UpdateImageOnPatchChange");

			int width = emManager.Width;
			int height = emManager.Height;
			EMConfiguration config = emManager.Configuration;			

			//uint* outputPointer = (uint*)screenBitmap.BackBuffer.ToPointer();
			//byte* featurePointer = (byte*)featureBitmap.BackBuffer.ToPointer();

			//int outputStride = screenBitmap.BackBufferStride / 4;
			//int featureStride = featureBitmap.BackBufferStride;
			int outputStride = screenBitmap.Width;
			int featureStride = featureBitmap.Width;

			if (patch != null && model != null)
			{
				model.CalculateBounds();
				int left = patch.Left, right = patch.Right, top = patch.Top, bottom = patch.Bottom;

				// Probabilities for the intensities in this patch
				int cK = model.K;
				double[,] normalisedProbabilities = new double[256, cK];
				int[] probabilityMaximums = new int[256];
				for (int intensity = 0; intensity < 256; intensity++)
				{
					double max = double.MinValue;
					for (int k = 0; k < cK; k++)
					{
						// Normalised probability
						normalisedProbabilities[intensity, k] = model.NormalisedProbability(intensity, k);

						// Maximum for this intensity
						if (normalisedProbabilities[intensity, k] > max)
						{
							max = normalisedProbabilities[intensity, k];
							probabilityMaximums[intensity] = k;
						}
					}
				}

				// Render patch data to image and probability map
				for (int y = top; y < bottom; y++)
				{
					for (int x = left; x < right; x++)
					{
						uint bgr32 = 0;
						int thresholdedK = model.K - model.ThresholdK - 1;

						int index = y * width + x;
						int bufferValue = intensityBuffer[index];
						int k = probabilityMaximums[bufferValue];
						//double p = normalisedProbabilities[bufferValue, k];

						// p is the probability of the most probable class at this pixel.
						// k is the most probable class
						// thresholdedK is the number of classes above the threshold value.

						if (k > model.ThresholdK && thresholdedK >= config.ExpectedRootClassCount)
						{
							int weightIndex = Math.Max(0, k - model.K + config.Weights.Length);

							// Most probable class
							this.probabilityMapBestClass[index] = normalisedProbabilities[bufferValue, k] * config.Weights[weightIndex];

							// Highest intensity class, regardless of probability
							this.probabilityMapBrightestClass[index] = normalisedProbabilities[bufferValue, model.K - 1] * config.Weights[config.Weights.Length - 1];

							// Second intensity class, regardless of probability - if one exists
							this.probabilityMapSecondClass[index] = normalisedProbabilities[bufferValue, model.K - 2] * config.Weights[Math.Max(0, config.Weights.Length - 2)];

							int pixelIntensity = (int)(this.probabilityMapBestClass[index] * 255);

							if (k == model.K - 1)
							{
								int blue = 127 + (pixelIntensity / 2);
								bgr32 = (uint)blue | 80 << 8 | 80 << 16;
							}
							else
							{
								bgr32 = (uint)(0 | pixelIntensity << 8);
							}
						}
						else
						{
							this.probabilityMapBestClass[index] = 0.0;
							this.probabilityMapBrightestClass[index] = 0.0;
							this.probabilityMapSecondClass[index] = 0.0;
							bgr32 = 0;
						}

						//*(featurePointer + (y * featureStride) + x) = (byte)(this.probabilityMapBrightestClass[index] * 255);
						//*(outputPointer + (y * outputStride) + x) = bgr32;

						//featureBitmap.Data.SetValue ((byte)(this.probabilityMapBrightestClass[index] * 255), (y * featureStride) + x);
						featureBitmap.Data[y, x, 0] = (byte)(this.probabilityMapBrightestClass[index] * 255);
						//screenBitmap.Data.SetValue(bgr32, (y * outputStride) + x);
						screenBitmap.Data[y, x, 0] = (byte)(this.probabilityMapBestClass[index] * 255);
						screenBitmap.Data[y, x, 1] = 0;
						screenBitmap.Data[y, x, 2] = 0;


					}
				}

				// Saves the blue channel out to a probability map
				//ImageEncoder.SaveImage(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\featuremap.png", featureBitmap, ImageEncoder.EncodingType.PNG);
				//Console.WriteLine ("Saving...");
				//featureBitmap.Save("/home/tuan/MyProject/RootNavLinux_MovedTo_iPlantUK_Repo/RootNavLinux/bin/Debug/featuremap.png");
				//ImageConverter.DisplayImage (featureBitmap.Mat, "Feature");

			} 


//			Console.WriteLine ("Saving...");
//			featureBitmap.Save("/home/tuan/MyProject/iPlantUK_Repo/RootNavLinux/RootNavLinux/bin/Debug/featuremap.png");
//			ImageConverter.DisplayImage (featureBitmap, "Feature");
		}

		public void BeginTipDetection()
		{
			//TipDetectionWorker tdw = new TipDetectionWorker();
			TipDetectionThread tdw = new TipDetectionThread();
			tdw.FeatureBitmap = this.featureBitmap;

			//this.featureBitmap.Save ("FeatureMap.png");

			//Mat smaller = new Mat(this.featureBitmap, new System.Drawing.Rectangle(0, 0, this.featureBitmap.Width, this.featureBitmap.Height));
			//CvInvoke.Resize (smaller, smaller, new System.Drawing.Size (600, 800) );

			//Mat smaller = new Mat(new System.Drawing.Size (600, 800), featureBitmap.Depth, featureBitmap.NumberOfChannels);
			//CvInvoke.Resize (featureBitmap, smaller, smaller.Size );


			tdw.ProgressCompleted += new RunWorkerCompletedEventHandler(TipDetectionCompleted);

			//tdw.RunWorkerAsync();
			tdw.Start();
		}

		void TipDetectionCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			//TipDetectionWorker tdw = sender as TipDetectionWorker;
			TipDetectionThread tdw = sender as TipDetectionThread;
			List<Int32Point> points = null;
			if (tdw != null)
			{
				points = tdw.Points;
			}

			if (points != null)
			{
				this.screenOverlay.TipAnchorPoints.Clear();

				this.screenOverlay.ResetAll();

				foreach (Int32Point p in points)
				{
//					this.screenOverlay.TipAnchorPoints.Add((Point)p);

//					 this.screenOverlay.Terminals.Add((Point)p, TerminalType.Undefined, false);
				}
			}

			OutputResultXML.writeOutputData (this.ProbabilityFilename, points);

			//TODO: testing
			System.Console.WriteLine("Total points: " + points.Count.ToString());

//			int count = this.screenOverlay.TipAnchorPoints.Count;
//			this.detectionToolbox.tipDetectionLabel.Content = count == 1 ? "1 Tip Detected" : count.ToString() + " Tips Detected";
//			this.detectionToolbox.cornerDetectionBorder.Visibility = System.Windows.Visibility.Visible;
//			this.detectionToolbox.cornerProcessingBorder.Visibility = System.Windows.Visibility.Hidden;
//			this.screenOverlay.InvalidateVisual();
		}

		public void AnalysePrimaryRoots()
		{
//			if (this.screenOverlay.IsBusy
//				|| this.screenOverlay.Terminals.Sources.Count() == 0
//				|| this.screenOverlay.Terminals.Primaries.Count() == 0)
//			{
//				return;
//			}

			//this.statusText.Text = "Status: Generating probability map";
			//this.screenOverlay.IsBusy = true;

			int width = this.emManager.Width;
			int height = this.emManager.Height;



			this.currentGraph = LiveWireGraph.FromProbabilityMap(this.probabilityMapBestClass, width, height);

			int combinations = this.screenOverlay.Terminals.UnlinkedSources.Count() * this.screenOverlay.Terminals.UnlinkedPrimaries.Count() + this.screenOverlay.Terminals.TerminalLinks.Count();

			int threadCount = Math.Min(RootNav.Core.Threading.ThreadParams.LiveWireThreadCount, combinations);
//
//			this.statusText.Text = "Status: Examining " + combinations.ToString() + " potential" + (combinations == 1 ? " root" : " roots");
//
			this.primaryLiveWireManager = new LiveWirePrimaryManager()
			{
				DistanceMap = this.distanceProbabilityMap,
				Graph = this.currentGraph,
				Terminals = this.screenOverlay.Terminals,
				ThreadCount = threadCount
			};

			primaryLiveWireManager.ProgressChanged += new ProgressChangedEventHandler(LiveWireManagerProgressChanged);
			primaryLiveWireManager.ProgressCompleted += new RunWorkerCompletedEventHandler(LiveWireManagerProgressCompleted);
			primaryLiveWireManager.Run();
		}
	} //end class
} //end namespace

