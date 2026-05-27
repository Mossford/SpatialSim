using SDL;
using SpatialSim.Engine.Core;

namespace SpatialSim.Engine.Audio
{
    public class AudioStream : IDisposable
    {
        public unsafe SDL_AudioStream* stream;
        public string name;

        public static int sampleFreq = 44100;

        public AudioStream(string name)
        {
            this.name = name;
        }

        public bool LoadFromFile(string file)
        {


            return true;
        }

        public unsafe bool LoadFromData(byte[] data)
        {
            
            Debug.LogDebug($"Loaded audio stream with data");
            return true;
        }

        public unsafe void CreateAudioStream()
        {
            SDL_AudioSpec spec = new()
            {
                channels = 1,
                //needs to be LE as that is little endian
                format = SDL_AudioFormat.SDL_AUDIO_F32LE,
                freq = sampleFreq
            };
            
            stream = SDL3.SDL_CreateAudioStream(&spec, null);

            if (stream == null)
            {
                Debug.Error("Could not create audio stream");
                throw new Exception("Could not create audio stream");
            }
            
            Debug.LogDebug($"Created audio stream");
        }

        public unsafe void BindToAudioDevice(SDL_AudioDeviceID id)
        {
            SDL3.SDL_BindAudioStream(id, stream);

            Debug.LogDebug($"Bound audio stream to audio device {id}");
        }
        
        /// <summary>
        /// this requires a update FPS of the sampleFreq / sampleCount or sampleCount / sampleFreq for the ms per frame allow
        /// otherwise it will start popping and shit
        /// </summary>
        public float[] samples = new float[1024];
        float phase;
        public unsafe void Test()
        {
            //fill up the stream buffer with chunks of the samples until its filled and played
            int queuedBytes = SDL3.SDL_GetAudioStreamQueued(stream);
            int bytesPerChunk = samples.Length * sizeof(float);
            int chunks = Math.Max(0, (sampleFreq * sizeof(float) - queuedBytes + bytesPerChunk - 1) / bytesPerChunk);
            
            for (int i = 0; i < chunks; i++)
            {
                for (int g = 0; g < samples.Length; g++)
                {
                    samples[g] = WaveGenerator.TriangleWave(phase);

                    //clamp between 1 and -1 to stop clipping and to represent the actual audio wave
                    if (samples[g] > 1f)
                        samples[g] = 1f;
                    else if (samples[g] < -1f)
                        samples[g] = -1f;

                    samples[g] *= 0.3f;
                    
                    phase += 2.0f * MathF.PI * 100 / sampleFreq;

                    if (phase >= 2.0f * MathF.PI)
                    {
                        phase -= 2.0f * MathF.PI;
                    }
                }

                fixed (float* sampleArray = samples)
                {
                    SDL3.SDL_PutAudioStreamData(stream, (nint)sampleArray, bytesPerChunk);
                }
            }
        }
        
        public unsafe void Clean()
        {
            SDL3.SDL_DestroyAudioStream(stream);
            
            Debug.LogDebug("Cleaned up audio stream");
        }

        public void Dispose()
        {
            Clean();
        }
    }
}