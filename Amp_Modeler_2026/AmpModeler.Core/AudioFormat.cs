namespace AmpModeler.Core
{
    public readonly struct AudioFormat
    {
        public double SampleRate { get; }
        public int Channels { get; }

        public AudioFormat(double sampleRate, int channels)
        {
            SampleRate = sampleRate;
            Channels = channels;
        }
    }
}
