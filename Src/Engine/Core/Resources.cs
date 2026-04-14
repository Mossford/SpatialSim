using SDL3;

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
            BasePath = SDL.GetBasePath() + "res/";
            ShaderPath = BasePath + "Shaders/";
            LogPath = BasePath + "Logs/";
            ImagePath = BasePath + "Images/";
            ModelPath = BasePath + "Models/";
        }
    }
}