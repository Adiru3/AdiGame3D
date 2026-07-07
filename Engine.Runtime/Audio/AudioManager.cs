using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using Engine.Core.Audio;
using Engine.Core.Entities;

namespace Engine.Runtime.Audio
{
    public class AudioManager : IDisposable
    {
        private AudioContext _audioContext;
        private Dictionary<string, int> _bufferCache = new Dictionary<string, int>();
        private List<int> _activeSources = new List<int>();
        private bool _disposed;

        public bool IsActive => _audioContext != null;

        public AudioManager()
        {
            try
            {
                _audioContext = new AudioContext();
                AL.DistanceModel(ALDistanceModel.LinearDistanceClamped);
                Console.WriteLine("OpenAL successfully initialized.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize OpenAL: {ex.Message}");
                _audioContext = null;
            }
        }

        public int GetOrLoadBuffer(string soundPath)
        {
            if (string.IsNullOrEmpty(soundPath) || !File.Exists(soundPath))
                return 0;

            if (_bufferCache.TryGetValue(soundPath, out int buffer))
                return buffer;

            try
            {
                var wav = WavLoader.Load(soundPath);
                buffer = AL.GenBuffer();
                AL.BufferData(buffer, wav.GetSoundFormat(), wav.Data, wav.Data.Length, wav.SampleRate);
                _bufferCache[soundPath] = buffer;
                return buffer;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading sound buffer {soundPath}: {ex.Message}");
                return 0;
            }
        }

        public int Play3DSound(string soundPath, Vec3 pos, bool looping, float volume, float radius)
        {
            if (!IsActive) return 0;

            int buffer = GetOrLoadBuffer(soundPath);
            if (buffer == 0) return 0;

            int source = AL.GenSource();
            AL.Source(source, ALSourcei.Buffer, buffer);
            AL.Source(source, ALSourceb.Looping, looping);
            AL.Source(source, ALSourcef.Gain, volume);
            AL.Source(source, ALSource3f.Position, pos.X, pos.Y, pos.Z);

            // Настройка затухания по радиусу (Clamped Linear)
            AL.Source(source, ALSourcef.ReferenceDistance, 1.0f);
            AL.Source(source, ALSourcef.RolloffFactor, 1.0f);
            AL.Source(source, ALSourcef.MaxDistance, radius);

            AL.SourcePlay(source);
            _activeSources.Add(source);
            return source;
        }

        public void UpdateListener(Vec3 pos, Vector3 front, Vector3 up)
        {
            if (!IsActive) return;

            AL.Listener(ALListener3f.Position, pos.X, pos.Y, pos.Z);

            float[] orientation = new float[]
            {
                front.X, front.Y, front.Z,
                up.X,    up.Y,    up.Z
            };
            AL.Listener(ALListenerfv.Orientation, ref orientation);
        }

        public void SetMasterVolume(float volume)
        {
            if (!IsActive) return;
            AL.Listener(ALListenerf.Gain, volume);
        }

        public void ClearSources()
        {
            if (!IsActive) return;

            foreach (var src in _activeSources)
            {
                if (AL.IsSource(src))
                {
                    AL.SourceStop(src);
                    AL.DeleteSource(src);
                }
            }
            _activeSources.Clear();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                ClearSources();

                foreach (var buf in _bufferCache.Values)
                {
                    if (AL.IsBuffer(buf))
                        AL.DeleteBuffer(buf);
                }
                _bufferCache.Clear();

                _audioContext?.Dispose();
                _audioContext = null;
                _disposed = true;
            }
        }
    }
}
