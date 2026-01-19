namespace AmpModeler.Audio.Abstractions
{
    public class AudioDeviceDescriptor
    {
        public string Id { get; }
        public string Name { get; }
        public bool IsDefault { get; }

        public AudioDeviceDescriptor(string id, string name, bool isDefault = false)
        {
            Id = id;
            Name = name;
            IsDefault = isDefault;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
