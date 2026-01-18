namespace AmpModeler.Core
{
    /// <summary>
    /// Defines a component that can process audio buffers.
    /// </summary>
    public interface IAudioProcessor
    {
        /// <summary>
        /// Processes the given audio buffer. This is called on the audio thread.
        /// </summary>
        /// <param name="buffer">The buffer containing samples to process.</param>
        void Process(AudioBuffer buffer);

        /// <summary>
        /// Prepares the processor for playback.
        /// </summary>
        /// <param name="sampleRate">The sample rate.</param>
        /// <param name="estimatedBlockSize">The estimated block size (may vary).</param>
        void PrepareToPlay(double sampleRate, int estimatedBlockSize);
    }
}
