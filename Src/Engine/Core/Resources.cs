using Silk.NET.SDL;

namespace SpatialSim.Engine.Core
{
    public static class Resources
    {
        public static string BasePath;
        public static string ShaderPath;
        public static string LogPath;

        public static void Init()
        {
            BasePath = Sdl.GetApi().GetBasePathS() + "res/";
            ShaderPath = BasePath + "Shaders/";
            LogPath = BasePath + "Logs/";
        }
    }
}