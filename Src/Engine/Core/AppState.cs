using SDL;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Vulkan;
using Silk.NET.Windowing;
using SpatialSim.Engine.Rendering.ImGui;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SpatialSim.Engine.Core
{
    public static class AppState
    {

        #region Information

        public const string Version = "0.14";
        public static string gpuDeviceName;
        public static string Api;

        #endregion
        
        #region Windowing

        public static unsafe SDL_Window* window;
        public static AppContext appContext;
        public static string WindowTitle = "Spatial Sim - " + Version;
        public static Vector2 WindowStartSize = new Vector2(1920, 1080);
        public static RenderingApi renderingApi = RenderingApi.Vulkan;

        #endregion

        #region Media

        public static SDL_AudioDeviceID audioDevice;

        #endregion

        #region State

        /// <summary>
        /// In microseconds
        /// </summary>
        public static ulong totalTime;
        /// <summary>
        /// In seconds
        /// </summary>
        public static float deltaTime;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double GetSeconds() => totalTime / 1000000.0f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong GetMillis() => totalTime / 1000;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong GetMicro() => totalTime;
        /// <summary>
        /// In seconds
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetDelta() => deltaTime;

        public static Random random;

        #endregion

        #region Debug

        public static bool EnableValidationLayers = true;
        public static bool EnableLogging = true;
        public static bool EnableConsoleLogging = true;
        public static bool EnableDebugLogging = true;
        public static bool EnableImguiDebugLogging = false;

        #endregion
    }
}
