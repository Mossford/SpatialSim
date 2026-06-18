using SDL;
using SpatialSim.Engine.Core;

namespace SpatialSim.Engine.Audio
{
    public static class AudioManager
    {
        public static Dictionary<string, int> audioToIndex;
        public const int MaximumStreams = 32;
        public static AudioStream[] audioStreams;
        public static Thread audioThread;
        public static object audioThreadLock = new();
        /// <summary>
        /// Audio thread will only ever be reading this
        /// </summary>
        public static volatile bool runningAudioThread;

        /// <summary>
        /// In seconds
        /// </summary>
        public static float audioDeltaTime;
        public static float globalVolume = 1.0f;
        
        static int currentEndStream;
        
        public static void Init()
        {
            audioToIndex = new Dictionary<string, int>();
            audioStreams = new AudioStream[MaximumStreams];
            audioThread = new Thread(Update);
            runningAudioThread = true;
            
            unsafe
            {
                //TODO This needs to be able to select a audio device
                AppState.audioDevice = SDL3.SDL_OpenAudioDevice(SDL3.SDL_AUDIO_DEVICE_DEFAULT_PLAYBACK, null);
            }

            if (AppState.audioDevice == 0)
            {
                Debug.Error("Could not open audio device");
                throw new Exception("Could not open audio device");
            }

            ResumeAudioDevice(AppState.audioDevice);
            
            audioThread.Start();
            
            Debug.LogInfo("Successful audio creation");
        }

        static void Update()
        {
            float pastTime = 0;

            while (runningAudioThread)
            {
                float time = SDL3.SDL_GetTicksNS() / 1e9f;
                audioDeltaTime = time - pastTime;

                int count = currentEndStream;

                for (int i = 0; i < count; i++)
                {
                    if (audioStreams[i] is null) 
                        continue;

                    audioStreams[i].Test();
                }

                pastTime = time;
            }
        }

        public static void ResumeAudioDevice(SDL_AudioDeviceID id)
        {
            SDL3.SDL_ResumeAudioDevice(id);
            Debug.LogDebug($"Resumed audio device {id}");
        }
        
        public static bool IsAudioStreamStored(string audioStream)
        {
            return audioToIndex.ContainsKey(audioStream);
        }

        public static bool LoadAudioStream(string audioFile)
        {
            if (!File.Exists(Resources.ImagePath + audioFile) && audioFile.Length != 0)
            {
                Debug.Warning($"Could not find file at path {Resources.ImagePath + audioFile}");
                return false;
            }
            
            if (!audioToIndex.ContainsKey(audioFile))
            {
                lock (audioThreadLock)
                {
                    if (currentEndStream >= MaximumStreams)
                    {
                        Debug.Error("Max streams reached");
                        return false;
                    }

                    audioStreams[currentEndStream] = new AudioStream(audioFile);

                    if (!audioStreams[currentEndStream].LoadFromFile(audioFile))
                    {
                        audioStreams[currentEndStream] = null;
                        return false;
                    }

                    audioStreams[currentEndStream].CreateAudioStream();
                    audioStreams[currentEndStream].BindToAudioDevice(AppState.audioDevice);
                    audioToIndex.Add(audioFile, currentEndStream);
                    
                    currentEndStream++;
                }
                
                
                return true;
            }

            Debug.Warning($"Could not add audio stream {audioFile} possible duplicate");
            return false;
        }
        
        public static bool LoadAudioStream(AudioStream audioStream)
        {
            if (!audioToIndex.ContainsKey(audioStream.name))
            {
                lock (audioThreadLock)
                {
                    if (currentEndStream >= MaximumStreams)
                    {
                        Debug.Error("Max streams reached");
                        return false;
                    }

                    audioStreams[currentEndStream] = audioStream;
                    audioStreams[currentEndStream].CreateAudioStream();
                    audioStreams[currentEndStream].BindToAudioDevice(AppState.audioDevice);
                    audioToIndex.Add(audioStream.name, currentEndStream);
                    
                    currentEndStream++;
                }
                return true;
            }

            Debug.Warning($"Could not add audio stream {audioStream.name} possible duplicate");
            return false;
        }

        public static AudioStream? RetrieveAudioStream(string audioFile)
        {
            lock (audioStreams)
            {
                if (audioToIndex.TryGetValue(audioFile, out int index))
                {
                    return audioStreams[index];
                }
                else
                {
                    if (LoadAudioStream(audioFile))
                    {
                        return audioStreams[audioToIndex[audioFile]];
                    }
                }   
            }
            
            Debug.Error("Tried to retrieve an audio file that does not exist");
            return null;
        }
        
        public static AudioStream? RetrieveAudioStream(int audioStream)
        {
            lock (audioStreams)
            {
                if (audioStream >= 0 && audioStream < audioStreams.Length)
                {
                    return audioStreams[audioStream];
                }   
            }
            
            Debug.Error("Tried to retrieve an audio stream that does not exist");
            return null;
        }

        public static int RetrieveAudioStreamIndex(string texture)
        {
            if (audioToIndex.TryGetValue(texture, out int index))
            {
                return index;
            }
            
            return 0;
        }

        public static void Clean()
        {
            runningAudioThread = false;

            audioThread.Join();
            
            //dont think this lock is needed but rider really wants it so
            lock (audioStreams)
            {
                for (int i = 0; i < audioStreams.Length; i++)
                {
                    if(audioStreams[i] is not null)
                        audioStreams[i].Clean();
                }
            }
            
            SDL3.SDL_CloseAudioDevice(AppState.audioDevice);
            
            Debug.LogInfo("Cleaned up audio manager");
        }
    }
}