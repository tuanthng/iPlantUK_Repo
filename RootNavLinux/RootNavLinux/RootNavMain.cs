using System;
using RootNav;
using RootNav.Core;
using RootNav.Core.MixtureModels;
using RootNav.Core.Threading;

using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.Structure;

using RootNav.IO;

namespace RootNavLinux
{
	public class RootNavMain
	{
		private EMManager emManager = null;
		private byte[] intensityBuffer = null;
		private GaussianMixtureModel highlightedMixture = null;
		//private LiveWirePrimaryManager primaryLiveWireManager = null;
		//private LiveWireLateralManager lateralLiveWireManager = null;

		private EMConfiguration[] configurations;
		private EMConfiguration customConfiguration = null;
		private int currentEMConfiguration = 0;

//		private bool connectionExists = false;

//		RootNav.Data.IO.Databases.DatabaseManager databaseManager = null;
//		private SceneMetadata.ImageInfo imageInfo = null;
//		private WriteableBitmap featureBitmap = null;
//		private WriteableBitmap sourceBitmap = null;
//		private WriteableBitmap probabilityBitmap = null;
//		private bool loadImageZoomOnce = false;

		private double[] probabilityMapBestClass = null;
		private double[] probabilityMapBrightestClass = null;
		private double[] probabilityMapSecondClass = null;
		private double[,] distanceProbabilityMap = null;

		private string FileName { get; set; }

		public RootNavMain (string filePathImg)
		{
			this.FileName = filePathImg;

			initConfiguration ();
			LoadImage (this.FileName);
		}

		private int initConfiguration()
		{
			try
			{
				this.configurations = EMConfiguration.LoadFromXML();

				this.currentEMConfiguration = EMConfiguration.DefaultIndex(configurations);

			}
			catch
			{
				Console.WriteLine("Configuration XML Error: An invalid value has been found in the E-M configuration XML file. Please correct this before running RootNav.");
				//Application.Current.Shutdown();

				//if error
				return -1;
			}

			return 0;
		}

		private Mat LoadImage(string filePath)
		{
			Mat img = null;

			try
			{

				img = CvInvoke.Imread(filePath, Emgu.CV.CvEnum.ImreadModes.AnyColor);
				//Console.WriteLine(filePath);
				Console.WriteLine(img.NumberOfChannels.ToString());
				Console.WriteLine(img.Cols.ToString() + " " + img.Rows.ToString());
				//for testing
				//ImageConverter.DisplayImage(img);
				//convert to gray scale
				//Mat gray = ImageConverter.ConvertGrayScaleImage(img);
				//Console.WriteLine(gray.Cols.ToString());
				//ImageConverter.DisplayImage(gray, "Gray");

				byte[] data = ImageConverter.ConvertMatToByteArray(ref img);

				Console.WriteLine(data.Length.ToString());

				Mat newImg = ImageConverter.ConvertByteArrayToMat(ref data, img.Cols, img.Rows, img.NumberOfChannels);
				ImageConverter.DisplayImage(newImg);

			}
			catch(Exception ex)
			{
				Console.WriteLine (ex.Message);
			}

			return img;
		}

		private void LoadImages(params string[] filePaths)
		{
			try
			{
				String filePath = filePaths[0];
				this.FileName = System.IO.Path.GetFileNameWithoutExtension(filePath);


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
	}
}

