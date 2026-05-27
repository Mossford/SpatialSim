using SDL;
using SpatialSim.Engine.Core;

namespace SpatialSim.Engine.Audio
{
    public static class AudioManager
    {
        public static Dictionary<string, int> audioToIndex;
        public static List<AudioStream> audioStreams;
        
        public static void Init()
        {
            audioToIndex = new Dictionary<string, int>();
            audioStreams = new List<AudioStream>();
            
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
            
            Debug.LogInfo("Successful audio creation");
        }

        public static void Update()
        {
            for (int i = 0; i < audioStreams.Count; i++)
            {
                 audioStreams[i].Test();
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
                audioStreams.Add(new AudioStream(audioFile));
                if (!audioStreams[^1].LoadFromFile(audioFile))
                {
                    audioStreams.RemoveAt(audioStreams.Count - 1);
                    return false;
                }

                audioToIndex.Add(audioFile, audioStreams.Count - 1);
                audioStreams[^1].CreateAudioStream();
                audioStreams[^1].BindToAudioDevice(AppState.audioDevice);
                
                return true;
            }

            Debug.Warning($"Could not add audio stream {audioFile} possible duplicate");
            return false;
        }
        
        public static bool LoadAudioStream(AudioStream audioStream)
        {
            if (!audioToIndex.ContainsKey(audioStream.name))
            {
                audioStreams.Add(audioStream);
                audioStreams[^1].CreateAudioStream();
                audioStreams[^1].BindToAudioDevice(AppState.audioDevice);
                audioToIndex.Add(audioStream.name, audioStreams.Count - 1);
                return true;
            }

            Debug.Warning($"Could not add audio stream {audioStream.name} possible duplicate");
            return false;
        }

        public static AudioStream RetrieveAudioStream(string audioFile)
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
            
            Debug.Error("Tried to retrieve a audio file that does not exist");
            return null;
        }
        
        public static AudioStream RetrieveAudioStream(int audioStream)
        {
            if (audioStream >= 0 && audioStream < audioStreams.Count)
            {
                return audioStreams[audioStream];
            }
            
            Debug.Error("Tried to retrieve a audio stream that does not exist");
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
            for (int i = 0; i < audioStreams.Count; i++)
            {
                audioStreams[i].Clean();
            }
            
            SDL3.SDL_CloseAudioDevice(AppState.audioDevice);
            
            Debug.LogInfo("Cleaned up audio manager");
        }
    }
}