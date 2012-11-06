﻿/*
 * The goal of this project is to be able to take a wave sample and create a synth preset for u-he Zebra 2 synth from that
 * Date: 27.07.2011
 * Time: 12:13
 * test
 */
using System;
using System.IO;
using System.Collections.Generic;

using CommonUtils.Audio;
using Wave2Zebra2Preset.Fingerprinting;
using Wave2Zebra2Preset.DataAccess;
using Wave2Zebra2Preset.Model;
using Wave2Zebra2Preset.Fingerprinting.MathUtils;

using Lomont;

using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;
using Un4seen.Bass.AddOn.Mix;
using Un4seen.Bass.AddOn.Tags;
using Un4seen.Bass.Misc;

using Wave2Zebra2Preset.HermitGauges;

using CommonUtils;
using CommonUtils.FFT;

using System.Drawing;
using System.Drawing.Imaging;

namespace Wave2Zebra2Preset
{
	public class MyColorHSB {
		private Color rgbcolor;
		private HSBColor hsbcolor;
		
		public MyColorHSB(Color rgbcolor) {
			this.rgbcolor = rgbcolor;
			this.hsbcolor = HSBColor.FromRGB(rgbcolor);
		}

		public MyColorHSB(HSBColor hsbcolor) {
			this.hsbcolor = hsbcolor;
			this.rgbcolor = hsbcolor.Color;
		}
		
		public HSBColor HSBColor {
			get { return hsbcolor; }
		}

		public Color Color {
			get { return rgbcolor; }
		}
		
		public override string ToString()
		{
			return
				rgbcolor.A + ";" +
				rgbcolor.R + ";" +
				rgbcolor.G + ";" +
				rgbcolor.B + ";" +
				hsbcolor.Hue + ";" +
				hsbcolor.Saturation + ";" +
				hsbcolor.Value;
		}
	}
	
	public class MyColorHSL {
		private Color rgbcolor;
		private HSLColor hslcolor;
		
		public MyColorHSL(Color rgbcolor) {
			this.rgbcolor = rgbcolor;
			this.hslcolor = HSLColor.FromRGB(rgbcolor);
		}

		public MyColorHSL(HSLColor hsbcolor) {
			this.hslcolor = hsbcolor;
			this.rgbcolor = hsbcolor.Color;
		}
		
		public HSLColor HSLColor {
			get { return hslcolor; }
		}

		public Color Color {
			get { return rgbcolor; }
		}
		
		public override string ToString()
		{
			return
				rgbcolor.A + ";" +
				rgbcolor.R + ";" +
				rgbcolor.G + ";" +
				rgbcolor.B + ";" +
				hslcolor.Hue + ";" +
				hslcolor.Saturation + ";" +
				hslcolor.Value;
		}
	}
	
	// Color palette URLs:
	// http://stackoverflow.com/questions/3097753/tinting-towards-or-away-from-a-hue-by-a-certain-percentage
	// http://stackoverflow.com/questions/2593832/how-to-interpolate-hue-values-in-hsv-colour-space
	// http://www.stuartdenman.com/improved-color-blending/
	// http://devmag.org.za/2012/07/29/how-to-choose-colours-procedurally-algorithms/
	// http://stackoverflow.com/questions/340209/generate-colors-between-red-and-green-for-a-power-meter
	// http://tabs2.gerg.tamu.edu/gmt/GMT_Docs/node214.html
	// http://geography.uoregon.edu/datagraphics/color_scales.htm (Color Schemes Appropriate for Scientific Data Graphics)
	// https://github.com/jolby/colors/blob/master/src/com/evocomputing/colors/palettes/core.clj
	// https://github.com/jolby/colors/blob/master/src/com/evocomputing/colors/palettes/color_brewer.clj
	
	// heatmap in c#
	// http://dylanvester.com/post/Creating-Heat-Maps-with-NET-20-(C-Sharp).aspx
	
	// Wavelet, Set and Hash etc
	// http://blogs.msdn.com/b/spt/archive/2008/06/10/set-similarity-and-min-hash.aspx (JaccardSimilarity ++)
	// http://laplacian.wordpress.com/2009/01/10/how-shazam-works/
	// http://www.whydomath.org/node/wavlets/index.html
	// http://www.whydomath.org/node/wavlets/hwt.html
	// http://www.codeproject.com/Articles/206507/Duplicates-detector-via-audio-fingerprinting
	
	class Program
	{
		///   Music file filters
		/// </summary>
		private static readonly string[] _musicFileFilters = new[] {"*.mp3", "*.ogg", "*.flac", "*.wav"};
		
		public static void SaveColorbar(String filenameToSave) {
			int width = 33;
			int height = 305;
			System.Console.Out.WriteLine("Writing " + filenameToSave);

			Bitmap png = new Bitmap(width, height, PixelFormat.Format32bppArgb );
			Graphics g = Graphics.FromImage(png);
			Pen pen = new Pen(Color.Black, 1.0f);
			
			// start with red (0, 1, 1)
			float h = 0;
			float s = 1.0f;
			float v = 1.0f;
			for(int y = 0; y <= height; y++)
			{
				// when yellow
				if (y > 0 && y <= 58) {
					h = y;
				} else  if (y > 59 && y <= 118) {
					// decrease v from 0.99 to 0.70
					v = v - (0.3f / 60);
					h = y;
				} else if (y > 118 && y < 240) {
					v = 0.69f;
					h = y;
				} else  if (y >= 240) {
					// decrease v from 0.68 to 0.48
					v = v - (0.2f / 60);
					
					// incease h slowly from 240 - 270
					// for y = 240 - 305
					h = h + (30.0f / 65);
				}
				
				Color c = ColorUtils.AhsbToArgb(255, h, s, v);
				
				pen.Color = c;
				g.DrawLine(pen, 1, y, width, y);
			}
			
			png.Save(filenameToSave);
		}
		
		public static void ReadColorPaletteBar(String filenameToRead, String csvExport) {
			Bitmap colorimage = new Bitmap(filenameToRead);
			
			List<MyColorHSB> pixels = new List<MyColorHSB>();
			for (int y = 0; y < colorimage.Height; y++)
			{
				Color pixel = colorimage.GetPixel(0, y);
				pixels.Add(new MyColorHSB(pixel));
			}
			Export.exportCSV(csvExport, pixels.ToArray(), pixels.Count);
		}

		/// <summary>
		/// Get a Color based on a input value between 0 and 100
		/// </summary>
		/// <param name="value">the input value between 0 and 100</param>
		/// <returns>MyColor</returns>
		public static MyColorHSB GetREWColorPaletteValue(float value) {
			
			float h = 0;
			float s = 1;
			float l = 0;
			
			// determine h, s and l values
			// based on a input value between 0 and 100
			if (value < 20) {
				h = 0.05f * value;
				l = 0.5f;
			} else if (value >= 20 && value < 40) {
				h = 0.05f * value;
				l = -0.0075f * value + 0.6499f;
			} else if (value >= 40 && value < 80) {
				h = 0.05f * value;
				l = 0.3490196f;
			} else if (value >= 80) {
				h = 0.0244f * value + 2.0189f;
				l = -0.0053f * value + 0.7699f;
			}
			// h is between 0 - 6, but can sometimes be minus
			float hue = h * 60f;
			if (hue < 0) hue += 360;
			
			HSBColor hslcolor = new HSBColor(hue/360, s, l);
			MyColorHSB mycolor = new MyColorHSB(hslcolor);
			return mycolor;
		}
		
		public static void SaveColorPaletteBar(string imageToSave, string csvToSave) {

			int width = 40;
			int height = 400;
			
			Bitmap png = new Bitmap(width, height, PixelFormat.Format32bppArgb );
			Graphics g = Graphics.FromImage(png);
			Pen pen = new Pen(Color.Black, 1.0f);
			
			MyColorHSB mycolor;
			List<MyColorHSB> pixels = new List<MyColorHSB>();
			for (float i = 0; i < 100; i = i + 0.25f) {
				mycolor = GetREWColorPaletteValue(i);
				pixels.Add(mycolor);
				pen.Color = mycolor.Color;
				g.DrawLine(pen, 0, i*4, width, i*4);
			}
			png.Save(imageToSave);
			Export.exportCSV(csvToSave, pixels.ToArray(), pixels.Count);
		}
		
		public static void Main(string[] args)
		{
			//SaveColorPaletteBar("c:\\rew-colorbar-generated.png", "c:\\rew-colorbar-generated.csv", ColorPaletteType.REWColorPalette);
			
			/*
			List<Color> rew_hsb_gradients = ColorUtils.GetHSBColorGradients(256, ColorUtils.ColorPaletteType.REW);
			ColorUtils.SaveColorGradients("c:\\rew-hsb-gradients.png", rew_hsb_gradients, 40);
			List<Color> rew_hsl_gradients = ColorUtils.GetHSLColorGradients(256, ColorUtils.ColorPaletteType.REW);
			ColorUtils.SaveColorGradients("c:\\rew-hsl-gradients.png", rew_hsl_gradients, 40);

			List<Color> sox_hsb_gradients = ColorUtils.GetHSBColorGradients(256, ColorUtils.ColorPaletteType.SOX);
			ColorUtils.SaveColorGradients("c:\\sox-hsb-gradients.png", sox_hsb_gradients, 40);
			List<Color> sox_hsl_gradients = ColorUtils.GetHSLColorGradients(256, ColorUtils.ColorPaletteType.SOX);
			ColorUtils.SaveColorGradients("c:\\sox-hsl-gradients.png", sox_hsl_gradients, 40);
			
			List<Color> photosounder_hsb_gradients = ColorUtils.GetHSBColorGradients(256, ColorUtils.ColorPaletteType.PHOTOSOUNDER);
			ColorUtils.SaveColorGradients("c:\\photosounder_hsb_gradients.png", photosounder_hsb_gradients, 40);
			List<Color> photosounder_hsl_gradients = ColorUtils.GetHSLColorGradients(256, ColorUtils.ColorPaletteType.PHOTOSOUNDER);
			ColorUtils.SaveColorGradients("c:\\photosounder_hsl_gradients.png", photosounder_hsl_gradients, 40);
			List<Color> photosounder_rgb_gradients = ColorUtils.GetRGBColorGradients(255, ColorUtils.ColorPaletteType.PHOTOSOUNDER);
			ColorUtils.SaveColorGradients("c:\\photosounder_rgb_gradients.png", photosounder_rgb_gradients, 40);
			
			List<Color> grey_hsb_gradients = ColorUtils.GetHSBColorGradients(256, ColorUtils.ColorPaletteType.BLACK_AND_WHITE);
			ColorUtils.SaveColorGradients("c:\\grey-hsb-gradients.png", grey_hsb_gradients, 40);
			*/
			
			/*
			ReadColorPaletteBar(@"C:\Users\perivar.nerseth\SkyDrive\Temp\sox_colorbar.png", "c:\\sox_colorbar.csv");
			ReadColorPaletteBar(@"C:\Users\perivar.nerseth\SkyDrive\Temp\soundforge_colorbar.png", "c:\\soundforge_colorbar.csv");
			ReadColorPaletteBar(@"C:\Users\perivar.nerseth\SkyDrive\Temp\rew_colorbar.png", "c:\\rew_colorbar.csv");
			ReadColorPaletteBar(@"C:\Users\perivar.nerseth\SkyDrive\Temp\sox_colorbar.png", "c:\\sox_colorbar.csv");
			ReadColorPaletteBar(@"C:\Users\perivar.nerseth\SkyDrive\Temp\thermal_colorbar.png", "c:\\thermal_colorbar.csv");
			ReadColorPaletteBar(@"C:\rew-gradients.png", "c:\\rew-gradients.csv");
			 */
			
			String fileName = @"C:\Users\perivar.nerseth\Music\Sleep Away.mp3";
			//String fileName = @"C:\Users\perivar.nerseth\Music\Sine-500hz-60sec.wav";
			//String fileName = @"G:\Cubase and Nuendo Projects\Music To Copy Learn\Britney Spears - Hold It Against Me\02 Hold It Against Me (Instrumental) 1.mp3";

			Console.WriteLine("Analyzer starting ...");
			
			RepositoryGateway repositoryGateway = new RepositoryGateway();
			FingerprintManager manager = new FingerprintManager();

			// VB6 FFT
			double sampleRate = 44100;// 44100  default 5512
			int fftWindowsSize = 4096; //4096  default 256*8 (2048) to 256*128 (32768), reccomended: 256*64 = 16384
			float fftOverlapPercentage = 80.0f; // number between 0 and 100
			int secondsToSample = 30; //15;
			float[] wavDataVB6 = repositoryGateway._proxy.ReadMonoFromFile(fileName, (int) sampleRate, secondsToSample*1000, 20*1000 );
			VB6Spectrogram vb6Spect = new VB6Spectrogram();
			vb6Spect.ComputeColorPalette();
			//float[][] vb6Spectrogram = vb6Spect.Compute(wavDataVB6, sampleRate, fftWindowsSize, fftOverlapPercentage);
			//Export.exportCSV (@"c:\VB6Spectrogram-full.csv", vb6Spectrogram);
			
			// Exocortex.DSP FFT
			int numberOfSamples = wavDataVB6.Length;
			fftOverlapPercentage = fftOverlapPercentage / 100;
			long ColSampleWidth = (long)(fftWindowsSize * (1 - fftOverlapPercentage));
			double fftOverlapSamples = fftWindowsSize * fftOverlapPercentage;
			long NumCols = numberOfSamples / ColSampleWidth;
			
			int fftOverlap = (int)((numberOfSamples - fftWindowsSize) / NumCols);
			int numberOfSegments = (numberOfSamples - fftWindowsSize)/fftOverlap;
			
			//System.Console.Out.WriteLine(String.Format("EXO: fftWindowsSize: {0}, Overlap samples: {1:n2}.", fftWindowsSize, fftOverlap ));

			float[][] exoSpectrogram = AudioAnalyzer.CreateSpectrogramExocortex(wavDataVB6, sampleRate, fftWindowsSize, fftOverlap);
			//float[][] exoSpectrogram = AudioAnalyzer.CreateSpectrogramLomont(wavDataVB6, sampleRate, fftWindowsSize, fftOverlap);
			//repositoryGateway.drawSpectrogram1("Spectrogram1", fileName, exoSpectrogram);
			//repositoryGateway.drawSpectrogram2("Spectrogram2", fileName, exoSpectrogram, sampleRate, numberOfSamples, fftWindowsSize);
			repositoryGateway.drawSpectrogram3("Spectrogram3", fileName, exoSpectrogram);
			//repositoryGateway.drawSpectrogram4("Spectrogram4", fileName, exoSpectrogram);
			//Export.exportCSV (@"c:\exoSpectrogram-full.csv", exoSpectrogram);
			
			Console.Write("Press any key to continue . . . ");
			Console.ReadKey(true);
		}
				
		public static void GetAudioInformation(string filename)
		{
			float lFrequency = 0;
			float lVolume = 0;
			float lPan = 0;
			
			int stream = Bass.BASS_StreamCreateFile(filename, 0L, 0L, BASSFlag.BASS_STREAM_DECODE);

			// the info members will contain most of it...
			BASS_CHANNELINFO info = Bass.BASS_ChannelGetInfo(stream);

			if (Bass.BASS_ChannelGetAttribute(stream, BASSAttribute.BASS_ATTRIB_VOL, ref lVolume))
				System.Diagnostics.Debug.WriteLine("Volume: " + lVolume);
			
			if (Bass.BASS_ChannelGetAttribute(stream, BASSAttribute.BASS_ATTRIB_PAN, ref lPan))
				System.Diagnostics.Debug.WriteLine("Pan: " + lPan);

			if (Bass.BASS_ChannelGetAttribute(stream, BASSAttribute.BASS_ATTRIB_FREQ, ref lFrequency))
				System.Diagnostics.Debug.WriteLine("Frequency: " + lFrequency);
			
			int nChannels = info.chans;
			System.Diagnostics.Debug.WriteLine("Channels: " + nChannels);

			int nSamplesPerSec = info.freq;
			System.Diagnostics.Debug.WriteLine("SamplesPerSec: " + nSamplesPerSec);
		}
		
		public static int GetSampleForTime(int msecs, int nSamplesPerSec)
		{
			double t = 1.0 / nSamplesPerSec;
			return (int)(msecs / 1000.0 / t);
		}

		public static int GetSampleTime(int nSample, int nSamplesPerSec)
		{
			return (int)(nSample * 1000.0 / nSamplesPerSec);
		}
		
		public static String GetSampleTimeString(int nSample, int nSamplesPerSec)
		{
			int ms = (int)(nSample * 1000.0 / nSamplesPerSec);
			return GetTimeString(ms, nSamplesPerSec);
		}

		public static String GetTimeString(int msecs, int nSamplesPerSec)
		{
			int s = msecs / 1000;
			int m = msecs / 60000;
			int h = msecs / 3600000;
			DateTime date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, h % 100, m % 60, s % 60, msecs % 1000);
			return String.Format("{0:HH:mm:ss.fff}", date);
		}
		
		// MATLAB:
		// B = fix(A) rounds the elements of A toward zero, resulting in an array of integers.
		// For complex A, the imaginary and real parts are rounded independently
		// if we have data with value :
		// X = [-1.9, -0.2	, 3.4	, 5.6, 7.0]
		// Y = [   1, 	 0	,   3	,   5,   7]
		public static int Fix(float a)
		{
			int sign = a < 0 ? -1 : 1;
			double dout = Math.Abs(a);
			double output = MathUtils.RoundDown(dout, 0);
			//float output = (float) (Math.Floor(a) * sign);
			return (int)output;
		}
		
	}
	
}