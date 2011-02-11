﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheGrid.Model;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using IrrKlang;

namespace TheGrid.Logic.Sound
{
    public class SoundLogic
    {

        IWavePlayer waveOutDevice;

            ISoundEngine _soundEngine;

        public GameEngine GameEngine { get; set; }

        public SoundLogic(GameEngine gameEngine)
        {
            this.GameEngine = gameEngine;

            //waveOutDevice = new NAudio.Wave.AsioOut();//AudioClientShareMode.Shared, 100);

            _soundEngine = new ISoundEngine();
        }

        public void PlaySample(Sample sample)
        {
            _soundEngine.Play2D(sample.FileName);
        }

        public void PlaySample2(Sample sample)
        {
            //IWavePlayer waveOutDevice;

            //waveOutDevice = new NAudio.Wave.DirectSoundOut();//AudioClientShareMode.Shared, 100);

            waveOutDevice.Stop();

            
            sample.WaveStream.CurrentTime = TimeSpan.Zero;

            waveOutDevice.Init(sample.WaveStream);
            waveOutDevice.Play();
        }

        void waveOutDevice_PlaybackStopped(object sender, EventArgs e)
        {
            IWavePlayer waveOutDevice = sender as IWavePlayer;

            waveOutDevice.Stop();
            waveOutDevice.Dispose();
        }

        public WaveStream CreateInputStream(string fileName)
        {
            WaveChannel32 inputStream;
            if (fileName.EndsWith(".mp3"))
            {
                WaveStream reader = new Mp3FileReader(fileName);
                inputStream = new WaveChannel32(reader);
            }
            else
            {
                WaveStream reader = new NAudio.Wave.WaveFileReader(fileName);
                inputStream = new WaveChannel32(reader);
            }
            //volumeStream = inputStream;
            //return volumeStream;

            return inputStream;
        }
    }
}
