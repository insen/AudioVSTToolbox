﻿using System;
using System.Threading;
using System.IO;

using NAudio.Wave;

using Jacobi.Vst.Core;
using Jacobi.Vst.Interop.Host;
using Jacobi.Vst.Core.Host;

using CommonUtils.VSTPlugin;
using CommonUtils.Audio;

namespace ProcessVSTPlugin2
{
	/// <summary>
	/// The class where you can call a method to process an audio file with a
	/// vst plugin, with sound or without sound playing
	/// Per Ivar Nerseth, 2011 - 2012
	/// perivar@nerseth.com
	/// </summary>
	public class ProcessVSTPlugin
	{
		private bool stoppedPlaying = false;
		
		private VST vst = null;
		
		private string _pluginPath = null;
		private int _sampleRate = 0;
		private int _channels = 0;
		
		public bool ProcessOffline(String waveInputFilePath, String waveOutputFilePath, String pluginPath, String fxpFilePath=null, float volume=1.0f) {

			HostCommandStub hcs = new HostCommandStub();
			hcs.Directory = System.IO.Path.GetDirectoryName(pluginPath);
			vst = new VST();
			
			try
			{
				vst.pluginContext = VstPluginContext.Create(pluginPath, hcs);
				
				if (vst.pluginContext == null) {
					Console.Out.WriteLine("Could not open up the plugin specified by {0}!", pluginPath);
					return false;
				}
				
				// plugin does not support processing audio
				if ((vst.pluginContext.PluginInfo.Flags & VstPluginFlags.CanReplacing) == 0)
				{
					Console.Out.WriteLine("This plugin does not process any audio.");
					return false;
				}
				
				// check if the plugin supports offline proccesing
				if(vst.pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.Offline)) == VstCanDoResult.No) {
					Console.Out.WriteLine("This plugin does not support offline processing.");
					Console.Out.WriteLine("Try use realtime (-play) instead!");
					return false;
				}
				
				// add custom data to the context
				vst.pluginContext.Set("PluginPath", pluginPath);
				vst.pluginContext.Set("HostCmdStub", hcs);

				// actually open the plugin itself
				vst.pluginContext.PluginCommandStub.Open();
				vst.pluginContext.PluginCommandStub.MainsChanged(true);
				
			} catch (Exception ex) {
				Console.Out.WriteLine("Could not load VST! ({0})", ex.Message);
				return false;
			}

			if (File.Exists(fxpFilePath)) {
				vst.LoadFXP(fxpFilePath);
			} else {
				Console.Out.WriteLine("Could not find preset file (fxp|fxb) ({0})", fxpFilePath);
			}
			
			WaveFileReader wavFileReader = new WaveFileReader(waveInputFilePath);
			using (VSTStream vstStream = new VSTStream()) {
				vstStream.ProcessCalled += new EventHandler<VSTStreamEventArgs>(vst_StreamCall);
				vstStream.PlayingStarted += new EventHandler(StartedPlaying);
				vstStream.PlayingStopped += new EventHandler(StoppedPlaying);
				
				vstStream.pluginContext = vst.pluginContext;
				vstStream.SetWaveFormat(wavFileReader.WaveFormat.SampleRate, wavFileReader.WaveFormat.Channels);
				
				vstStream.SetInputWave(waveInputFilePath, volume);
				
				// each float is 4 bytes
				byte[] buffer = new byte[512*4];
				using (MemoryStream ms = new MemoryStream())
				{
					while (!stoppedPlaying)
					{
						int read = vstStream.Read(buffer, 0, buffer.Length);
						if (read <= 0) {
							break;
						}
						ms.Write(buffer, 0, read);
					}

					// save
					using (WaveStream ws = new RawSourceWaveStream(ms, vstStream.WaveFormat))
					{
						ws.Position = 0;
						WaveFileWriter.CreateWaveFile(waveOutputFilePath, ws);
					}
				}
			}

			vst.pluginContext.PluginCommandStub.MainsChanged(false);

			// reset if calling this method multiple times
			stoppedPlaying = false;
			return true;
		}
		
		public bool ProcessRealTime(String waveInputFilePath, String waveOutputFilePath, String pluginPath, String fxpFilePath=null, float volume=1.0f)
		{
			WaveFileReader wavFileReader = new WaveFileReader(waveInputFilePath);

			// reuse if batch processing
			bool doUpdateVstPlugin = false;
			bool doUpdateSampleRate = false;
			bool doUpdateNoChannels = false;
			if (_pluginPath != null) {
				if (_pluginPath.Equals(pluginPath)) {
					// plugin has not changed
				} else {
					// plugin has changed!
					doUpdateVstPlugin = true;
				}
			} else {
				_pluginPath = pluginPath;
				doUpdateVstPlugin = true;
			}
			
			if (_sampleRate != 0) {
				if (_sampleRate == wavFileReader.WaveFormat.SampleRate) {
					// same sample rate
				} else {
					// sample rate has changed!
					doUpdateSampleRate = true;
				}
			} else {
				_sampleRate = wavFileReader.WaveFormat.SampleRate;
				doUpdateSampleRate = true;
			}

			if (_channels != 0) {
				if (_channels == wavFileReader.WaveFormat.Channels) {
					// same number of channels
				} else {
					// number of channels has changed!
					doUpdateNoChannels = true;
				}
			} else {
				_channels = wavFileReader.WaveFormat.Channels;
				doUpdateNoChannels = true;
			}

			if (doUpdateNoChannels || doUpdateSampleRate) {
				Console.Out.WriteLine("Opening Audio driver using samplerate {0} and {1} channels.", _sampleRate, _channels);
				UtilityAudio.OpenAudio(AudioLibrary.NAudio, _sampleRate, _channels);
			}

			if (doUpdateVstPlugin || doUpdateNoChannels || doUpdateSampleRate) {
				Console.Out.WriteLine("Loading Vstplugin using samplerate {0} and {1} channels.", _sampleRate, _channels);
				vst = UtilityAudio.LoadVST(_pluginPath, _sampleRate, _channels);
				//vst.StreamCall += new EventHandler<VSTStreamEventArgs>(vst_StreamCall);
				UtilityAudio.VstStream.ProcessCalled += new EventHandler<VSTStreamEventArgs>(vst_StreamCall);
				UtilityAudio.VstStream.PlayingStarted += new EventHandler(StartedPlaying);
				UtilityAudio.VstStream.PlayingStopped += new EventHandler(StoppedPlaying);
				
				// plugin does not support processing audio
				if ((vst.pluginContext.PluginInfo.Flags & VstPluginFlags.CanReplacing) == 0)
				{
					Console.Out.WriteLine("This plugin does not process any audio.");
					return false;
				}
				
				// check if the plugin supports real time proccesing
				if(vst.pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.NoRealTime)) == VstCanDoResult.Yes) {
					Console.Out.WriteLine("This plugin does not support realtime processing.");
					return false;
				}
			}

			if (File.Exists(fxpFilePath)) {
				Console.Out.WriteLine("Loading preset file {0}.", fxpFilePath);
				vst.LoadFXP(fxpFilePath);
			} else {
				Console.Out.WriteLine("Could not find preset file (fxp|fxb) ({0})", fxpFilePath);
			}

			if (UtilityAudio.PlaybackDevice.PlaybackState != PlaybackState.Playing) {
				Console.Out.WriteLine("Starting audio playback engine.");
				UtilityAudio.StartAudio();
			}
			
			Console.Out.WriteLine("Setting input wave {0}.", waveInputFilePath);
			UtilityAudio.VstStream.SetInputWave(waveInputFilePath, volume);

			Console.Out.WriteLine("Setting output wave {0}.", waveOutputFilePath);
			UtilityAudio.SaveStream(waveOutputFilePath);
			
			UtilityAudio.VstStream.DoProcess = true;
			
			// just wait while the stream is playing
			// the events will trigger and set the stoppedPlaying flag
			while (!stoppedPlaying)
			{
				Thread.Sleep(50);
			}

			// reset if calling this method multiple times
			stoppedPlaying = false;
			return true;
		}
		
		public bool Process(String waveInputFilePath, String waveOutputFilePath, String pluginPath, String fxpFilePath=null, float volume=1.0f, bool doPlay=false)
		{
			if (doPlay) {
				return ProcessRealTime(waveInputFilePath, waveOutputFilePath, pluginPath, fxpFilePath, volume);
			} else {
				return ProcessOffline(waveInputFilePath, waveOutputFilePath, pluginPath, fxpFilePath, volume);
			}
		}

		private void StartedPlaying(object sender, EventArgs e) {
			if (UtilityAudio.PlaybackDevice != null) {
				UtilityAudio.StartStreamingToDisk();
				Console.Out.WriteLine("Started streaming to disk ...");
			}
			stoppedPlaying = false;
		}

		private void StoppedPlaying(object sender, EventArgs e) {
			if (UtilityAudio.PlaybackDevice != null) {
				UtilityAudio.VstStream.DoProcess = false;
				UtilityAudio.StopStreamingToDisk();
				Console.Out.WriteLine("Stopped streaming to disk ...");
			}
			stoppedPlaying = true;
		}
		
		private void vst_StreamCall(object sender, VSTStreamEventArgs e)
		{
		}
		
		public void Dispose() {
			if (UtilityAudio.PlaybackDevice != null) UtilityAudio.Dispose();
		}
	}
}