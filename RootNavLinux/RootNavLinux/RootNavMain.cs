using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Linq;

using RootNav;
using RootNav.Core;
using RootNav.Core.Imaging;
using RootNav.Core.MixtureModels;
using RootNav.Core.Threading;
using RootNav.IO;
using RootNav.Core.Tips;
using RootNav.Core.LiveWires;
using RootNav.Interface.Controls;

using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using System.Xml;
using RootNav.Data;
using RootNav.Core.Measurement;
using RootNav.Data.IO.RSML;
using RootNav.Data.IO;
using System.Text;

namespace RootNavLinux
{
	public class RootNavMain
	{
		public static double BackgroundPenalty = 0.1;

		public static double SCALE_UI = 4.5;

		public delegate void StatusTextUpdateDelegate(String s);
		//public delegate void ScreenImageUpdateDelegate(WriteableBitmap wbmp);
		public delegate void ScreenImageUpdateDelegate(Mat wbmp);
		public delegate void EMCompletedDelegate();
		public delegate void LiveWireLateralCompletedDelegate(List<LiveWireLateralPath> paths);
		public delegate void LiveWirePrimaryCompletedDelegate(List<LiveWirePrimaryPath> paths);
		public delegate void LiveWireReCompletedDelegate();

		private EMManagerThread emManager = null;
		private byte[] intensityBuffer = null;
		private GaussianMixtureModel highlightedMixture = null;
		//private LiveWirePrimaryManager primaryLiveWireManager = null;
		//private LiveWireLateralManager lateralLiveWireManager = null;
		private LiveWirePrimaryManagerThread primaryLiveWireManager = null;
		private LiveWireLateralManagerThread lateralLiveWireManager = null;

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
		private RootNav.Data.IO.ConnectionParams connectionInfo = null;
		private TiffHeaderInfo imageHeaderInfo = null;

//		RootNav.Data.IO.Databases.DatabaseManager databaseManager = null;
		private SceneMetadata.ImageInfo imageInfo = null;
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

		//private RootTerminalCollection terminalCollection = new RootTerminalCollection();

		private string ImageFileName { get; set; }
		private string ResultXMLFileName{ get; set; } //the xml file containing result processed
		public string OutputPath{ get; set; } //output and input path will be passed from outside. By default, they should be the current directory of the program
		public string InputPath{ get; set; }

		private string ProbabilityBitmapImageFilename{ get; set; }
		private string ProbabilityBitmapDataFilename{ get; set; }

		private string FeatureBitmapImageFilename{ get; set; }
		private string FeatureBitmapDataFileName {get; set;}

		private string ProbabilityMapBestClassDataFilename { get; set; }

		private string ProbabilityMapBrightestClassDataFilename { get; set; }
		private string ProbabilityMapSecondClassDataFilename { get; set; }

		public string InputPointsFilename{ get; set; }

		//these flags are used to notify whether to find the shortest from Primary node to Source node or from Lateral node to Source Node.
		//Finding the shortest paths only if there is any source node and either at least one primary node or lateral node.
		private bool hasSourceNode = false; 
		private bool hasPrimaryNode = false;
		private bool hasLateralNode = false;

		private List<AdjustedPath> listAdjustedPaths;

		private static Random random = new Random();

		//parameters for measurement
		public double ImageResolutionValue { get; set; }
		public int SplineSpacing { get; set; }
		public string PlantName { get; set; }
		public string TagName { get; set; }
		public bool DoCurvatureProfile { get; set; }
		public bool DoMapProfile { get; set; }
		public int TravelMap { get; set; }
		public bool DoCompleteArch { get; set; }
		public bool DoMeasurement { get; set; }
		public bool DoMeasurementTable{ get; set; }
		public string RSMLDirectory{ get; set; }

		public RootNavMain (string filePathImg)
		{
			this.ImageFileName = filePathImg;

			initConfiguration ();
			createResultFilename ();
			createFilenameForSaving ();
			initialiseConnectionInfo ();

			//store the xml file into the global
			OutputResultXML.FullOutputFileName = ResultXMLFileName;

			this.screenOverlay = new RootDetectionScreenOverlay ();
		}

		public void Process()
		{
			LoadImage (this.ImageFileName);

			if (this.PlantName == null || this.PlantName.Length == 0) 
			{
				this.PlantName = RandomString (10);
			}

			if (this.TagName == null || this.TagName.Length == 0) 
			{
				this.TagName = this.PlantName;
			}

			EMProcessing ();

			//writing input data
			//OutputResultXML.writeInputData(ImageFileName, this.InputPath, this.OutputPath, this.emManager.Configuration);

			//OutputResultXML.writeOutputData (this.ProbabilityFilename, null);


		}
		private void createResultFilename()
		{
			
			ResultXMLFileName = Path.Combine(OutputPath, this.ImageFileName + "_result.xml");

		}
		private void createFilenameForSaving()
		{
			//probability map
			string name = System.IO.Path.GetFileNameWithoutExtension (this.ImageFileName);
			this.ProbabilityBitmapImageFilename = name + "_map.png";
			this.ProbabilityBitmapDataFilename = name + "_map_data.dat";

			//feature map
			this.FeatureBitmapImageFilename = name + "_feature.png";
			this.FeatureBitmapDataFileName = name + "_feature_data.dat";

			ProbabilityMapBestClassDataFilename = name + "_probBestClass_data.dat";

			//
			ProbabilityMapBrightestClassDataFilename = name + "_probBrightestClass_data.dat";
			ProbabilityMapSecondClassDataFilename = name + "_probSecondClass_data.dat";
		}
		private int initConfiguration()
		{
			try
			{
				//intialise the input/output as the current directory
				//InputPath = System.IO.Directory.GetCurrentDirectory();
				InputPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
				OutputPath = InputPath;
				RSMLDirectory = OutputPath;

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

				this.imageInfo = new SceneMetadata.ImageInfo()
				{
					Label = filePath,
					Hash = Hashing.Sha256(filePath),
					Background = "dark",
					Unit = "pixels",
					TimeInSequence = 0.0
				};

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
			OutputResultXML.writeInputData(ImageFileName, this.InputPath, this.OutputPath, this.emManager.Configuration, this.InputPointsFilename);

			this.emManager.Run();

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

			this.probabilityBitmap.Save(this.ProbabilityBitmapImageFilename);

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
						//screenBitmap.Data[y, x, 0] = (byte)(this.probabilityMapBestClass[index] * 255);
						//screenBitmap.Data[y, x, 1] = 0;
						//screenBitmap.Data[y, x, 2] = 0;

						byte[] values = BitConverter.GetBytes (bgr32);

						screenBitmap.Data[y, x, 0] = values[0];
						screenBitmap.Data[y, x, 1] = values[1];
						screenBitmap.Data[y, x, 2] = values[2];
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
					this.screenOverlay.TipAnchorPoints.Add((Point)p);

					//TODO: for testing
					//this.screenOverlay.Terminals.Add((Point)p, TerminalType.Primary, false);

				}
				//TODO: for testing
				//this.screenOverlay.Terminals.Add((Point)points[0], TerminalType.Primary, false);
			}

			//OutputResultXML.writeTipsDetectedData (this.ProbabilityFilename, points);
			OutputResultXML.writeTipsDetectedDataForBisque (this.ProbabilityBitmapImageFilename, points);

			//TODO: testing
			System.Console.WriteLine("Total points: " + points.Count.ToString());

//			int count = this.screenOverlay.TipAnchorPoints.Count;
//			this.detectionToolbox.tipDetectionLabel.Content = count == 1 ? "1 Tip Detected" : count.ToString() + " Tips Detected";
//			this.detectionToolbox.cornerDetectionBorder.Visibility = System.Windows.Visibility.Visible;
//			this.detectionToolbox.cornerProcessingBorder.Visibility = System.Windows.Visibility.Hidden;
//			this.screenOverlay.InvalidateVisual();

			//TODO: testing by adding 1 or more source points and using all tips detected
			//AddSourcePoint(new Point(675.6, 29.34), false);

			//parse the input points
			parseInputNodes();

			if (this.hasSourceNode && this.hasPrimaryNode) {
				AnalysePrimaryRoots ();	
			}
//			if (this.hasSourceNode && this.hasLateralNode) {
//				AnalyseLateralRoots ();
//			}


		}

		public void AnalysePrimaryRoots()
		{
//			if (this.screenOverlay.IsBusy
//				|| this.screenOverlay.Terminals.Sources.Count() == 0
//				|| this.screenOverlay.Terminals.Primaries.Count() == 0)
//			{
//				return;
//			}

			if (this.screenOverlay.Terminals.Sources.Count() == 0
				|| this.screenOverlay.Terminals.Primaries.Count() == 0)
			{
				return;
			}

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
			this.primaryLiveWireManager = new LiveWirePrimaryManagerThread()
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

		private void LiveWireManagerProgressChanged(object sender, ProgressChangedEventArgs args)
		{
			//this.Dispatcher.Invoke(new TaskProgressChangedHandler(TaskProgressChanged), new object[] { args.ProgressPercentage });
		}

		private void TaskProgressChanged(int progress)
		{
			//this.mainProgressBar.Value = progress;
		}

		private void LiveWireManagerProgressCompleted(object sender, RunWorkerCompletedEventArgs args)
		{
			LiveWireManagerThread manager = sender as LiveWireManagerThread;

			if (manager == null)
			{
				return;
			}

			if (sender == this.primaryLiveWireManager)
			{
				if (this.baseWeightDescriptors != null)
				{
					this.baseWeightDescriptors.Clear();
				}

				List<LiveWirePrimaryPath> paths = new List<LiveWirePrimaryPath>();
				foreach (KeyValuePair<Tuple<int, int>, List<Point>> pointPath in this.primaryLiveWireManager.Paths)
				{
					paths.Add(new LiveWirePrimaryPath(pointPath.Key.Item1, pointPath.Key.Item2, pointPath.Value, this.primaryLiveWireManager.ControlPointIndices[pointPath.Key]));
				}

				// Weightings
				this.baseWeightDescriptors = new List<LiveWireWeightDescriptor>();
				foreach (LiveWirePrimaryPath pointPath in paths)
				{
					this.baseWeightDescriptors.Add(new LiveWireWeightDescriptor(pointPath, probabilityMapBestClass, this.emManager.Width, this.emManager.Height));
				}

				// UI
				//this.Dispatcher.BeginInvoke(new LiveWirePrimaryCompletedDelegate(this.LiveWirePrimaryWorkCompletedUI), paths);
				//probably, dont need to use thread here because there is no UI
				this.LiveWirePrimaryWorkCompletedUI (paths);
			}

			if (sender == this.lateralLiveWireManager)
			{
				List<LiveWireLateralPath> paths = new List<LiveWireLateralPath>();
				foreach (var item in this.lateralLiveWireManager.Paths)
				{
					List<Point> path = item.Value;
					TargetPathPoint target = this.lateralLiveWireManager.TargetPathIndices[item.Key];
					List<int> indices = this.lateralLiveWireManager.ControlPointIndices[item.Key];
					paths.Add(new LiveWireLateralPath(item.Key, target, path, indices));
				}

				// UI
				//this.Dispatcher.BeginInvoke(new LiveWireLateralCompletedDelegate(this.LiveWireLateralWorkCompletedUI), paths);
				//probably, dont need to use thread here because there is no UI
				this.LiveWireLateralWorkCompletedUI (paths);

			}

			//saveData ();
		}

		private void LiveWirePrimaryWorkCompletedUI(List<LiveWirePrimaryPath> paths)
		{
			List<LiveWirePrimaryPath> basePaths;

			LiveWireRootAssociation.FindRoots(this.screenOverlay.Terminals,
				paths,
				this.baseWeightDescriptors,
				out basePaths,
				out this.baseWeightDescriptors);

			// Create gray image the first time only
			if (this.screenOverlay.Paths.Count == 0)
			{
				//WriteableBitmap gray = RootNav.IO.ImageConverter.ConvertToGrayScaleUniform(this.ScreenImage.ImageSource as WriteableBitmap);
				//Mat gray = RootNav.IO.ImageConverter.ConvertGrayScaleImage(this.ScreenImage.ImageSource as WriteableBitmap);
				//this.Dispatcher.Invoke(new ScreenImageUpdateDelegate(this.UpdateScreenImage), gray);
			}
			else
			{
				// Clear old paths
				this.screenOverlay.ClearAll();
			}

			foreach (LiveWirePrimaryPath path in basePaths)
			{
				this.screenOverlay.Paths.Add(path);
			}

			//this.detectionToolbox.UncheckToggleButtons(null);
			//this.screenOverlay.IsBusy = false;
			//this.statusText.Text = "Status: Idle";
			//this.preMeasurementToolbox.MeasurementButton.IsEnabled = true;

			//Now, this is the time to write the output paths
			//OutputResultXML.writePrimaryPathsData( this.screenOverlay.Paths);
			OutputResultXML.writePrimaryPathsDataForBisque( this.screenOverlay.Paths, this.screenOverlay.RenderInfo);

			if (this.screenOverlay.Paths.Primaries.Count () > 0) 
			{
				LiveWirePrimaryPath path = this.screenOverlay.Paths.Primaries.First ();
				System.Console.WriteLine ("Total points of the 1st path the primary point: " + path.Path.Count.ToString ());

			}
			else {
				System.Console.WriteLine ("No primary path.");
			}

			//analyse lateral if has
			if (this.hasSourceNode && this.hasLateralNode) {
				AnalyseLateralRoots ();
			}
			//if doesn't have lateral, no more analyse, then save data
			//if has, saving data will wait untill analysing lateral finishes
			if (!this.hasLateralNode) {
				//saveData ();
				this.processAdjustedPaths ();  
				//TODO: need to check to make sure adjusting paths completely finish before measurement begins
				BeginMeasurementStage();

			}
		}

		private void LiveWireLateralWorkCompletedUI(List<LiveWireLateralPath> paths)
		{
			if (paths.Count > 0)
			{
				if (this.screenOverlay.Paths.Laterals.Count() > 0)
				{
					this.screenOverlay.ClearLaterals();
				}

				foreach (LiveWireLateralPath path in paths)
				{
					this.screenOverlay.Paths.Add(path);
				}
			}

			//this.detectionToolbox.UncheckToggleButtons(null);
			//this.screenOverlay.IsBusy = false;
			//this.statusText.Text = "Status: Idle";

			//Now, this is the time to write the output paths
			//OutputResultXML.writeLateralPathsData( this.screenOverlay.Paths);
			OutputResultXML.writeLateralPathsDataForBisque( this.screenOverlay.Paths, this.screenOverlay.RenderInfo);

			if (this.screenOverlay.Paths.Laterals.Count () > 0) 
			{
				LiveWireLateralPath path = this.screenOverlay.Paths.Laterals.First ();
				System.Console.WriteLine ("Total points of the 1st path the lateral point: " + path.Path.Count.ToString ());

			} 
			else 
			{
				System.Console.WriteLine ("No lateral path.");
			}

			//TODO: need to check to make sure adjusting paths completely finish before measurement begins
			this.processAdjustedPaths ();

			//saveData ();
			BeginMeasurementStage();

		}

		private void UpdateScreenImage(Mat wbmp)
		{
			//ScreenImage.ImageSource = wbmp;
		}

		public void AnalyseLateralRoots()
		{
			//if (this.screenOverlay.IsBusy
			//	|| this.screenOverlay.Paths.Primaries.Count() == 0
			//	|| this.screenOverlay.Terminals.Laterals.Count() == 0)
			//{
			//	return;
			//}

			if (this.screenOverlay.Paths.Primaries.Count() == 0
				|| this.screenOverlay.Terminals.Laterals.Count() == 0)
			{
				return;
			}

			//this.screenOverlay.IsBusy = true;

			int width = this.emManager.Width;
			int height = this.emManager.Height;

			int threadCount = RootNav.Core.Threading.ThreadParams.LiveWireThreadCount;

			if (this.currentGraph == null)
				this.currentGraph = LiveWireGraph.FromProbabilityMap(this.probabilityMapBestClass, width, height);

			int lateralCount = this.screenOverlay.Terminals.Laterals.Count();
			//this.statusText.Text = "Status: Examining " + lateralCount.ToString() + " lateral" + (lateralCount == 1 ? " root" : " roots");

			this.lateralLiveWireManager = new LiveWireLateralManagerThread()
			{
				Graph = currentGraph,
				Terminals = this.screenOverlay.Terminals,
				CurrentPaths = this.screenOverlay.Paths.Primaries.ToList(),
				ThreadCount = Math.Min(lateralCount, threadCount),
				DistanceMap = this.distanceProbabilityMap
			};
			lateralLiveWireManager.ProgressChanged += new ProgressChangedEventHandler(LiveWireManagerProgressChanged);
			lateralLiveWireManager.ProgressCompleted += new RunWorkerCompletedEventHandler(LiveWireManagerProgressCompleted);
			lateralLiveWireManager.Run();
		}

		private void AddSourcePoint(Point source, bool createLink)
		{
			this.screenOverlay.Terminals.Add (source, TerminalType.Source, createLink);
			this.hasSourceNode = true;
		}

		private void AddLateralPoint(Point source, bool createLink)
		{
			this.screenOverlay.Terminals.Add (source, TerminalType.Lateral, createLink);
			this.hasLateralNode = true;
		}

		private void AddPrimaryPoint(Point source, bool createLink)
		{
			this.screenOverlay.Terminals.Add (source, TerminalType.Primary, createLink);
			this.hasPrimaryNode = true;
		}

		private void AddPoint(Point newpoint, TerminalType type, bool createLink)
		{
			switch (type)
			{
			case TerminalType.Source:
				AddSourcePoint (newpoint, createLink);
				break;
			case TerminalType.Lateral:
				AddLateralPoint (newpoint, createLink);
				break;
			case TerminalType.Primary:
				AddPrimaryPoint (newpoint, createLink);
				break;
			}
		}

		private void parseInputNodes()
		{
			if (this.InputPointsFilename != null && this.InputPointsFilename.Length > 0) {
				if (File.Exists (Path.Combine(this.InputPath, this.InputPointsFilename))) {
					//this code used to append new node to the existing xml file
					XmlTextReader reader = new XmlTextReader (InputPointsFilename);
					XmlDocument doc = new XmlDocument ();
					doc.Load (reader);
					reader.Close ();

					//select the 1st node
					XmlElement root = doc.DocumentElement;
					XmlNode pointsNode = root.SelectSingleNode ("/InputData/Points");

					foreach (XmlNode node in pointsNode.ChildNodes) 
					{
						if (node.Attributes ["type"].Value.ToUpper().CompareTo ("SOURCE") == 0) 
						{
							AddSourcePoint (new Point (double.Parse (node.Attributes ["x"].Value), double.Parse (node.Attributes ["y"].Value)), false);
						} 
						else if (node.Attributes ["type"].Value.ToUpper().CompareTo ("PRIMARY") == 0) 
						{
							AddPrimaryPoint (new Point (double.Parse (node.Attributes ["x"].Value), double.Parse (node.Attributes ["y"].Value)), false);
						} 
						else if (node.Attributes ["type"].Value.ToUpper().CompareTo ("LATERAL") == 0) 
						{
							AddLateralPoint (new Point (double.Parse (node.Attributes ["x"].Value), double.Parse (node.Attributes ["y"].Value)), false);
						}
					} //end for each

					//check if there is any adjusted paths
					XmlNodeList adjustedPaths = root.SelectNodes ("/InputData/AdjustedPaths/Path");

					if (adjustedPaths != null && adjustedPaths.Count > 0) 
					{
						this.listAdjustedPaths = new List<AdjustedPath> ();

						foreach (XmlNode path in adjustedPaths) 
						{
							AdjustedPath newPath = new AdjustedPath ();

							foreach (XmlNode point in path.ChildNodes) 
							{
								Point p = new Point (double.Parse (point.Attributes ["x"].Value), double.Parse (point.Attributes ["y"].Value));

								if (point.Attributes ["type"].Value.ToUpper().CompareTo ("MID") == 0) 
								{
									newPath.IntermediatePoints.Add (p);	
								} 
								else if (point.Attributes ["type"].Value.ToUpper().CompareTo ("START") == 0) 
								{
									newPath.StartPoint = p;

								} //end else if 
							}  //end for each

							this.listAdjustedPaths.Add(newPath);

						} //end for each
					} //end if
				} //end if
			} //end if
			else 
			{
				System.Console.WriteLine ("No point input");
			}
		} //end parseInputNodes

		private void processAdjustedPaths ()
		{
			System.Console.WriteLine ("processing Adjusted Paths...");

			if (this.listAdjustedPaths != null && this.listAdjustedPaths.Count > 0) 
			{
				foreach (AdjustedPath newPath in this.listAdjustedPaths) 
				{
					//find the closest terminal and get the terminal index and type
					int terminalIndex = -1;
					Point p = newPath.StartPoint;

					bool found = this.screenOverlay.FindNearbyTerminalPoint (p, RootDetectionScreenOverlay.RootUISize * SCALE_UI, out terminalIndex);

					if (found) {
						//determine the lateral or primary path defending on the terminal type found
						TerminalType typeFound = this.screenOverlay.Terminals [terminalIndex].Type;

						newPath.TypePath = typeFound;
					}//end if
					else 
					{
						newPath.TypePath = TerminalType.Undefined;
					}

					//find if the starting point is close to any root path
					//int controlPointIndex;
					int rootIndex;
					Point rootPosition;

					//bool rootFound = this.screenOverlay.FindNearbyControlPointByTerminalType (p, RootDetectionScreenOverlay.RootUISize, newPath.TypePath, out controlPointIndex, out rootIndex);
					bool rootFound = this.screenOverlay.FindNearbyRootPoints (p, RootDetectionScreenOverlay.RootUISize * SCALE_UI, out rootPosition, out rootIndex);

					if (rootFound) 
					{
						//if found any root, assign new control points for that root
						//this.AddControlPointToRoot(rootIndex, 
						for (int index = 0; index < newPath.IntermediatePoints.Count; index++) 
						{
							this.AddControlPointToRoot (rootIndex, newPath.IntermediatePoints [index], (index + 1) == newPath.IntermediatePoints.Count);
						}
					} //end if

				} //end foreach
			} //end if
		} //end

		public void saveData()
		{
			System.Console.WriteLine ("Saving data...");
			//save probability map to an image
			System.Console.WriteLine ("Save Probability bitmap to an image...");
			this.probabilityBitmap.Save(this.ProbabilityBitmapImageFilename);
			System.Console.WriteLine ("Save Probability bitmap to data file...");
			OutputResultXML.writeMatToFile (this.ProbabilityBitmapDataFilename, this.probabilityBitmap);

			System.Console.WriteLine ("Save Feature bitmap to an image...");
			this.featureBitmap.Save (this.FeatureBitmapImageFilename);
			System.Console.WriteLine ("Save Feature bitmap to data file...");
			OutputResultXML.writeMatToFile (this.FeatureBitmapDataFileName, this.featureBitmap);

			System.Console.WriteLine ("Save Probability Map Best Class to data file...");
			OutputResultXML.write1DArrayToFile (this.ProbabilityMapBestClassDataFilename, this.probabilityMapBestClass);
			System.Console.WriteLine ("Save Probability Map Brightest Class to data file...");
			OutputResultXML.write1DArrayToFile (this.ProbabilityMapBrightestClassDataFilename, this.probabilityMapBrightestClass);
			System.Console.WriteLine ("Save Probability Map Second Class to data file...");
			OutputResultXML.write1DArrayToFile (this.ProbabilityMapSecondClassDataFilename, this.probabilityMapSecondClass);
		}

//		~RootNavMain()
//		{
//			saveData ();
//		}

		public void BeginMeasurementStage()
		{
			if (!this.DoMeasurement) 
			{
				return;
			}
			this.imageInfo.Resolution = ImageResolutionValue;
			this.imageInfo.Unit = ImageResolutionValue == 0 ? "pixels" : "mm";

			this.screenOverlay.InitialiseMeasurementStage(this.SplineSpacing, ImageResolutionValue == 0 ? 0 : 1 / ImageResolutionValue);

			System.Console.WriteLine ("Saving root data...");
			OutputResultXML.writeRootData (this.screenOverlay.Roots, this.screenOverlay.RenderInfo);

			if (this.screenOverlay.Roots != null && this.screenOverlay.Roots.RootTree.Count > 0)
			{
				System.Console.WriteLine ("Writing RSML file...");
				writeDataToRSML (this.TagName);

				System.Console.WriteLine ("Saving measurement data...");
				OutputResultXML.writeMeasurementData (this.screenOverlay.Roots, this.screenOverlay.RenderInfo, this.TagName, 
					this.DoMeasurementTable, this.DoCurvatureProfile, 
					this.DoMapProfile, this.TravelMap, this.probabilityMapSecondClass, this.emManager.Width, this.emManager.Height);
			}

//			Binding b = new Binding();
//			b.Source = this.screenOverlay.Roots.RootTree;
//			BindingOperations.SetBinding(this.rootTreeView, TreeView.ItemsSourceProperty, b);
//
//			this.rootTreeView.MouseMove += new MouseEventHandler(rootTreeView_MouseMove);
//			this.rootTreeView.MouseLeave += new MouseEventHandler(rootTreeView_MouseLeave);
//
//			this.rootTreeView.SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(rootTreeView_SelectedItemChanged);

			//this.detectionSlidePanel.BeginHide();
			//this.measurementSlidePanel.BeginShow();
		}

		private void initialiseConnectionInfo()
		{
			this.connectionInfo = new ConnectionParams() { Directory = this.RSMLDirectory};
			this.connectionInfo.Source = ConnectionSource.RSMLDirectory;
		}

		private bool writeDataToRSML (string tag)
		{
			// Create instance of writer class
			RootNav.Data.IO.RSML.RSMLRootWriter writer = new RSMLRootWriter (connectionInfo);
			
			// Create Scene and Metadata
			SceneMetadata metadata = RootFormatConverter.RootNavDataToRSMLMetadata (this.imageInfo, this.imageHeaderInfo, tag, this.screenOverlay.Roots);
			SceneInfo scene = RootFormatConverter.RootCollectionToRSMLScene (this.screenOverlay.Roots);
			
			if (!this.DoCompleteArch) 
			{
				RootFormatConverter.SetIncompletePropertyOnScene (metadata, scene);
			}
			
			bool success = writer.Write (metadata, scene);

			if (success) 
			{
				OutputResultXML.writeRSML(this.RSMLDirectory, writer.RSMLFile);

				System.Console.WriteLine ("Status: Measurements successfully output to RSML file");
			} 
			else 
			{
				System.Console.WriteLine ("Status: Measurements could not be written to RSML file");
			}
			return success;
		}

		private string RandomString(int size)
		{
			StringBuilder builder = new StringBuilder();
			char ch;
			for (int i = 0; i < size; i++)
			{
				ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
				builder.Append(ch);
			}

			return builder.ToString();
		}

		public void ReprocessAlteredRoot(params int[] rootIndexes)
		{
			//this.screenOverlay.IsBusy = true;
			//this.statusText.Text = "Status: Recalculating " + rootIndexes.Length.ToString() + (rootIndexes.Length == 1 ? " root" : " roots");
			System.Console.WriteLine("Status: Recalculating " + rootIndexes.Length.ToString() + (rootIndexes.Length == 1 ? " root" : " roots"));
				
			int width = this.emManager.Width;
			int height = this.emManager.Height;

			int threadCount = RootNav.Core.Threading.ThreadParams.LiveWireThreadCount;

			List<LiveWirePrimaryPath> alteredPaths = new List<LiveWirePrimaryPath>();

			foreach (int i in rootIndexes)
			{
				LiveWirePrimaryPath p = this.screenOverlay.Paths[i] as LiveWirePrimaryPath;
				if (p != null)
				{
					alteredPaths.Add(p);
				}
			}

			LiveWirePrimaryManagerThread manager = new LiveWirePrimaryManagerThread()
			{
				Graph = this.currentGraph,
				Terminals = this.screenOverlay.Terminals,
				ThreadCount = Math.Min(rootIndexes.Length, threadCount),
				DistanceMap = this.distanceProbabilityMap,
				ReWorkPaths = alteredPaths
			};

			manager.ProgressChanged += new ProgressChangedEventHandler(LiveWireManagerProgressChanged);
			manager.ProgressCompleted += new RunWorkerCompletedEventHandler(LiveWireManagerReProgressCompleted);
			manager.ReRun();
		}

		private void LiveWireManagerReProgressCompleted(object sender, RunWorkerCompletedEventArgs args)
		{
			// Weightings
			this.baseWeightDescriptors.Clear();
			foreach (LiveWirePath path in this.screenOverlay.Paths)
			{
				LiveWirePrimaryPath primary = path as LiveWirePrimaryPath;
				if (primary != null)
				{
					this.baseWeightDescriptors.Add(new LiveWireWeightDescriptor(primary, probabilityMapBestClass, this.emManager.Width, this.emManager.Height));
				}
			}

			// UI
			//this.Dispatcher.BeginInvoke(new LiveWireReCompletedDelegate(this.LiveWireReWorkCompletedUI));
			this.LiveWireReWorkCompletedUI();
		}

		private void LiveWireReWorkCompletedUI()
		{
			this.screenOverlay.RecalculateAllSamples();
			//this.screenOverlay.InvalidateVisual();
			//this.screenOverlay.IsBusy = false;
			//this.statusText.Text = "Status: Idle";

			if (this.screenOverlay.Paths.Primaries.Count () > 0) 
			{
				LiveWirePrimaryPath path = this.screenOverlay.Paths.Primaries.First ();
				System.Console.WriteLine ("Total points of the 1st path the primary point (reprocessed): " + path.Path.Count.ToString ());

			}
			else {
				System.Console.WriteLine ("No primary path.");
			}

			//TODO: output result. Make sure to remove the old Primary paths
			OutputResultXML.writePrimaryPathsDataForBisque( this.screenOverlay.Paths, this.screenOverlay.RenderInfo);
		}

		public void ReprocessLateralRoot(params int[] rootIndexes)
		{
			//this.screenOverlay.IsBusy = true;
			//this.statusText.Text = "Status: Recalculating " + rootIndexes.Length.ToString() + (rootIndexes.Length == 1 ? " root" : " roots");
			System.Console.WriteLine("Status: Recalculating " + rootIndexes.Length.ToString() + (rootIndexes.Length == 1 ? " root" : " roots"));
			int width = this.emManager.Width;
			int height = this.emManager.Height;

			int threadCount = RootNav.Core.Threading.ThreadParams.LiveWireThreadCount;

			List<LiveWireLateralPath> alteredPaths = new List<LiveWireLateralPath>();

			foreach (int i in rootIndexes)
			{
				LiveWireLateralPath p = this.screenOverlay.Paths[i] as LiveWireLateralPath;
				if (p != null)
				{
					alteredPaths.Add(p);
				}
			}

			//this.lateralLiveWireManager = new LiveWireLateralManager()
			this.lateralLiveWireManager = new LiveWireLateralManagerThread()
			{
				Graph = currentGraph,
				Terminals = this.screenOverlay.Terminals,
				ThreadCount = Math.Max(rootIndexes.Length, threadCount),
				CurrentPaths = this.screenOverlay.Paths.Primaries.ToList(),
				ReWorkPaths = alteredPaths,
				DistanceMap = this.distanceProbabilityMap
			};
			lateralLiveWireManager.ProgressChanged += new ProgressChangedEventHandler(LiveWireManagerProgressChanged);
			lateralLiveWireManager.ProgressCompleted += new RunWorkerCompletedEventHandler(LateralLiveWireManagerReProgressCompleted);
			lateralLiveWireManager.ReRun();
		}

		void LateralLiveWireManagerReProgressCompleted(object sender, RunWorkerCompletedEventArgs args)
		{
			// UI
			//this.Dispatcher.BeginInvoke(new LiveWireReCompletedDelegate(this.LiveWireReWorkCompletedUI));
			this.LiveWireReWorkCompletedUI();
		}

		public void AddControlPointToRoot(int highlightedRootIndex, Point dragPoint, Point newPoint)
		{
			if (this.screenOverlay.Paths[highlightedRootIndex] is LiveWirePrimaryPath)
			{
				LiveWirePrimaryPath currentPath = this.screenOverlay.Paths[highlightedRootIndex] as LiveWirePrimaryPath;

				if (currentPath == null)
				{
					return;
				}

				int newIndex = currentPath.Path.IndexOf(dragPoint);
				if (currentPath.IntermediatePoints.Count > 0)
				{
					bool indexFound = false;
					for (int i = 0; i < currentPath.IntermediatePoints.Count; i++)
					{
						if (newIndex < currentPath.Indices[i])
						{
							currentPath.IntermediatePoints.Insert(i, newPoint);
							indexFound = true;
							break;
						}
					}
					if (!indexFound)
					{
						currentPath.IntermediatePoints.Add(newPoint);
					}

				}
				else
				{
					currentPath.IntermediatePoints.Add(newPoint);
				}

				ReprocessAlteredRoot(highlightedRootIndex);
			}
			else if (this.screenOverlay.Paths[highlightedRootIndex] is LiveWireLateralPath)
			{
				LiveWireLateralPath currentPath = this.screenOverlay.Paths[highlightedRootIndex] as LiveWireLateralPath;

				if (currentPath == null)
				{
					return;
				}

				int newIndex = currentPath.Path.IndexOf(dragPoint);
				if (currentPath.IntermediatePoints.Count > 0)
				{
					bool indexFound = false;
					for (int i = 0; i < currentPath.IntermediatePoints.Count; i++)
					{
						if (newIndex < currentPath.Indices[i])
						{
							currentPath.IntermediatePoints.Insert(i, newPoint);
							indexFound = true;
							break;
						}
					}
					if (!indexFound)
					{
						currentPath.IntermediatePoints.Add(newPoint);
					}

				}
				else
				{
					currentPath.IntermediatePoints.Add(newPoint);
				}

				ReprocessLateralRoot(highlightedRootIndex);
			}
		}

		public void AddControlPointToRoot(int highlightedRootIndex, Point newPoint, bool executed = false)
		{
			if (this.screenOverlay.Paths[highlightedRootIndex] is LiveWirePrimaryPath)
			{
				LiveWirePrimaryPath currentPath = this.screenOverlay.Paths[highlightedRootIndex] as LiveWirePrimaryPath;

				if (currentPath == null)
				{
					return;
				}

				currentPath.IntermediatePoints.Add(newPoint);

				if (executed) 
				{
					ReprocessAlteredRoot(highlightedRootIndex);
				}

			}
			else if (this.screenOverlay.Paths[highlightedRootIndex] is LiveWireLateralPath)
			{
				LiveWireLateralPath currentPath = this.screenOverlay.Paths[highlightedRootIndex] as LiveWireLateralPath;

				if (currentPath == null)
				{
					return;
				}

				currentPath.IntermediatePoints.Add(newPoint);

				if (executed) 
				{
					ReprocessLateralRoot(highlightedRootIndex);	
				}

			}
		}
	} //end class
} //end namespace

