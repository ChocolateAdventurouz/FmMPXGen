using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave.SampleProviders;
using NAudio.Wave;

namespace FmMPXGen
{
    class Program
    {
        static void Main(string[] args)
        {
            for (int n = -1; n < WaveOut.DeviceCount; n++)
            {
                var caps = WaveOut.GetCapabilities(n);
                Console.WriteLine($"{n}: {caps.ProductName}");
            }

            // stereo pilot (mpx)
            var sine = new SignalGenerator()
            {
                Gain = 1.0,
                Frequency = 19000,
                Type = SignalGeneratorType.Sin
            }.Take(TimeSpan.FromSeconds(50));

            // rds pilot
            var rdssine = new SignalGenerator()
            {
                Gain = 0.2,
                Frequency = 57000,
                Type = SignalGeneratorType.Sin
            }.Take(TimeSpan.FromSeconds(50));
            var stereo = new MultiplexingSampleProvider(new[] { sine, rdssine }, 2);
            using (var wo = new WaveOutEvent())
            {
                wo.DesiredLatency = 333;
                wo.DeviceNumber = 5;
                var resampler = new WdlResamplingSampleProvider(stereo, 192000);
                wo.Init(resampler);
                wo.Play();
                while (wo.PlaybackState == PlaybackState.Playing)
                {
                    Thread.Sleep(wo.DesiredLatency);
                }
            }
        }
    }
}
