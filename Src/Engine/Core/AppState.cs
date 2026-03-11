using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.SDL;
using Silk.NET.Vulkan;
using Silk.NET.Windowing;
using SpatialSim.Engine.Rendering.ImGui;

namespace SpatialSim.Engine.Core
{
    public static class AppState
    {

        #region Information

        public const string Version = "0.02";
        public static string gpuDeviceName;
        public static string Api;

        #endregion
        
        #region Windowing
        
        public static IWindow window;
        public static AppContext appContext;
        public static string WindowTitle = "Spatial Sim - " + Version;
        public static Vector2 WindowStartSize = new Vector2(1280, 720);

        #endregion

        #region State

        public static ulong totalTime;
        public static float deltaTime;
        public static double GetSeconds() => totalTime / 1000000.0f;
        public static ulong GetMillis() => totalTime / 1000;
        public static ulong GetMicro() => totalTime;
        public static float GetDelta() => deltaTime;

        #endregion

        #region Debug

        public static bool EnableVkValidationLayers = true;
        public static bool EnableLogging = true;
        public static bool EnableConsoleLogging = true;
        public static bool EnableDebugLogging = false;

        #endregion
    }
}