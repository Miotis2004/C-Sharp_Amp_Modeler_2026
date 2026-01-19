using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AmpModeler.Audio.Abstractions;
using AmpModeler.Audio.Wasapi;
using AmpModeler.Engine;

namespace Amp_Modeler_2026.ViewModels
{
    public class MainViewModel : ViewModelBase, IDisposable
    {
        private readonly AudioEngine _engine;
        private readonly WasapiHost _host;

        private AudioDeviceDescriptor? _selectedInput;
        private AudioDeviceDescriptor? _selectedOutput;
        private bool _isRunning;
        private string _statusMessage = "Ready";

        public ObservableCollection<AudioDeviceDescriptor> InputDevices { get; } = new();
        public ObservableCollection<AudioDeviceDescriptor> OutputDevices { get; } = new();

        public AudioDeviceDescriptor? SelectedInput
        {
            get => _selectedInput;
            set
            {
                if (SetProperty(ref _selectedInput, value))
                {
                    if (value != null && !_isRunning) // Only allow changing when stopped for now
                        _host.SetInputDevice(value);
                    ((RelayCommand)StartCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public AudioDeviceDescriptor? SelectedOutput
        {
            get => _selectedOutput;
            set
            {
                if (SetProperty(ref _selectedOutput, value))
                {
                     if (value != null && !_isRunning)
                        _host.SetOutputDevice(value);
                     ((RelayCommand)StartCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                if (SetProperty(ref _isRunning, value))
                {
                    ((RelayCommand)StartCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)StopCommand).RaiseCanExecuteChanged();
                    StatusMessage = value ? "Running" : "Stopped";
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }

        public MainViewModel()
        {
            _host = new WasapiHost();
            _engine = new AudioEngine();
            _engine.SetHost(_host);

            StartCommand = new RelayCommand(async () => await StartAudioAsync(), CanStartAudio);
            StopCommand = new RelayCommand(async () => await StopAudioAsync(), CanStopAudio);

            RefreshDevices();
        }

        private void RefreshDevices()
        {
            InputDevices.Clear();
            foreach (var device in _host.GetInputDevices()) InputDevices.Add(device);

            OutputDevices.Clear();
            foreach (var device in _host.GetOutputDevices()) OutputDevices.Add(device);

            // Auto-select first or default
            SelectedInput = InputDevices.FirstOrDefault(d => d.IsDefault) ?? InputDevices.FirstOrDefault();
            SelectedOutput = OutputDevices.FirstOrDefault(d => d.IsDefault) ?? OutputDevices.FirstOrDefault();
        }

        private bool CanStartAudio()
        {
            return !IsRunning && SelectedInput != null && SelectedOutput != null;
        }

        private bool CanStopAudio()
        {
            return IsRunning;
        }

        private async Task StartAudioAsync()
        {
            try
            {
                StatusMessage = "Starting...";
                await _engine.StartAsync();
                IsRunning = true;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                IsRunning = false;
            }
        }

        private async Task StopAudioAsync()
        {
             try
            {
                StatusMessage = "Stopping...";
                await _engine.StopAsync();
                IsRunning = false;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error stopping: {ex.Message}";
            }
        }

        public void Dispose()
        {
            _engine?.Dispose();
            _host?.Dispose();
        }
    }
}
