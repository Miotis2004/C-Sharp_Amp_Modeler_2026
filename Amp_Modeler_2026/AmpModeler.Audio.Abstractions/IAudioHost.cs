using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AmpModeler.Core;

namespace AmpModeler.Audio.Abstractions
{
    public interface IAudioHost
    {
        /// <summary>
        /// Gets a value indicating whether the host is currently running.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Gets the current sample rate.
        /// </summary>
        double SampleRate { get; }

        /// <summary>
        /// Starts the audio stream.
        /// </summary>
        Task StartAsync();

        /// <summary>
        /// Stops the audio stream.
        /// </summary>
        Task StopAsync();

        /// <summary>
        /// Sets the audio processor that will receive callbacks.
        /// </summary>
        void SetProcessor(IAudioProcessor processor);

        /// <summary>
        /// Gets the list of available input devices.
        /// </summary>
        IEnumerable<AudioDeviceDescriptor> GetInputDevices();

        /// <summary>
        /// Gets the list of available output devices.
        /// </summary>
        IEnumerable<AudioDeviceDescriptor> GetOutputDevices();

        /// <summary>
        /// Sets the input device.
        /// </summary>
        void SetInputDevice(AudioDeviceDescriptor device);

        /// <summary>
        /// Sets the output device.
        /// </summary>
        void SetOutputDevice(AudioDeviceDescriptor device);
    }
}
