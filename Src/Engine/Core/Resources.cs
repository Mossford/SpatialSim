using SDL;

namespace SpatialSim.Engine.Core
{
    public static class Resources
    {
        public static string BasePath;
        public static string ShaderPath;
        public static string LogPath;
        public static string ImagePath;
        public static string ModelPath;

        public static void Init()
        {
            BasePath = SDL3.SDL_GetBasePath() + "res/";
            ShaderPath = BasePath + "Shaders/";
            LogPath = BasePath + "Logs/";
            ImagePath = BasePath + "Images/";
            ModelPath = BasePath + "Models/";
        }
    }
}