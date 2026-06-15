using SDL;
using SpatialSim.Engine.Core;

namespace SpatialSim.Engine.Audio
{
    public class AudioStream : IDisposable
    {
        public unsafe SDL_AudioStream* stream;
        public string name;
        public ulong samplesQueued;

        public const int sampleFreq = 44100;

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
        /// otherwise it will start popping and shit, The time to fill is also how much this will update at so a FPS of 1 will update the buffer every second
        /// </summary>
        public float[] samples = new float[512];
        float phase;
        float accumulator;
        public unsafe void Test()
        {
            //fill up the stream buffer with chunks of the samples until its filled and played
            int queuedBytes = SDL3.SDL_GetAudioStreamQueued(stream);
            int bytesPerChunk = samples.Length * sizeof(float);
            int chunks = Math.Max(0, (sampleFreq * sizeof(float) - queuedBytes + bytesPerChunk - 1) / bytesPerChunk);

            accumulator += AudioManager.audioDeltaTime;
            
            for (int i = 0; i < chunks; i++)
            {
                for (int g = 0; g < samples.Length; g++)
                {
                    float runTime = 0.3f;
                    float hangTime = 0.1f;
                    if(accumulator > 0 && accumulator < runTime)
                        samples[g] = AudioFunctions.TriangleWave(phase);
                    else if (accumulator > runTime && accumulator < runTime + hangTime)
                        samples[g] = 0;
                    else if (accumulator > hangTime)
                        accumulator = 0;

                    samples[g] *= MathF.Exp(-accumulator * 10);
                    
                    //clamp between 1 and -1 to stop clipping and to represent the actual audio wave
                    samples[g] = Math.Clamp(samples[g], -1f, 1f);
                    float frequency = Math.Clamp((400 * runTime) - accumulator * 400, 0f, float.MaxValue);
                    
                    phase += 2.0f * MathF.PI * frequency / sampleFreq;

                    if (phase >= 2.0f * MathF.PI)
                    {
                        phase -= 2.0f * MathF.PI;
                    }

                    samples[g] *= AudioManager.globalVolume;
                }

                fixed (float* sampleArray = samples)
                {
                    SDL3.SDL_PutAudioStreamData(stream, (nint)sampleArray, bytesPerChunk);
                    samplesQueued += (ulong)samples.Length;
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