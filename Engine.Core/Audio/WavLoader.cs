using System;
using System.IO;

namespace Engine.Core.Audio
{
    public class WavData
    {
        public byte[] Data { get; set; }
        public int Channels { get; set; }
        public int BitsPerSample { get; set; }
        public int SampleRate { get; set; }

        public OpenTK.Audio.OpenAL.ALFormat GetSoundFormat()
        {
            if (Channels == 1)
            {
                return BitsPerSample == 8 
                    ? OpenTK.Audio.OpenAL.ALFormat.Mono8 
                    : OpenTK.Audio.OpenAL.ALFormat.Mono16;
            }
            else
            {
                return BitsPerSample == 8 
                    ? OpenTK.Audio.OpenAL.ALFormat.Stereo8 
                    : OpenTK.Audio.OpenAL.ALFormat.Stereo16;
            }
        }
    }

    public static class WavLoader
    {
        public static WavData Load(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"WAV file not found: {filePath}");

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(stream))
            {
                // RIFF Header
                string signature = new string(reader.ReadChars(4));
                if (signature != "RIFF")
                    throw new NotSupportedException("Specified stream is not a RIFF stream.");

                reader.ReadInt32(); // File size

                string format = new string(reader.ReadChars(4));
                if (format != "WAVE")
                    throw new NotSupportedException("Specified stream is not a WAVE stream.");

                // Read chunks
                int channels = 0;
                int sampleRate = 0;
                int bitsPerSample = 0;
                byte[] data = null;

                while (stream.Position < stream.Length)
                {
                    string chunkId = new string(reader.ReadChars(4));
                    int chunkSize = reader.ReadInt32();

                    if (chunkId == "fmt ")
                    {
                        int audioFormat = reader.ReadInt16(); // 1 = PCM
                        channels = reader.ReadInt16();
                        sampleRate = reader.ReadInt32();
                        reader.ReadInt32(); // Byte rate
                        reader.ReadInt16(); // Block align
                        bitsPerSample = reader.ReadInt16();

                        // Skip any extra bytes in fmt chunk
                        if (chunkSize > 16)
                        {
                            reader.ReadBytes(chunkSize - 16);
                        }
                    }
                    else if (chunkId == "data")
                    {
                        data = reader.ReadBytes(chunkSize);
                        break; // Data chunk found, we can stop
                    }
                    else
                    {
                        // Skip unknown chunk
                        reader.ReadBytes(chunkSize);
                    }
                }

                if (data == null)
                    throw new InvalidDataException("WAV file does not contain a data chunk.");

                return new WavData
                {
                    Data = data,
                    Channels = channels,
                    BitsPerSample = bitsPerSample,
                    SampleRate = sampleRate
                };
            }
        }
    }
}
