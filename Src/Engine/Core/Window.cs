using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using SpatialSim.Engine.Core.Vulkan;
using SpatialSim.Engine.Rendering;
using SpatialSim.Engine.Rendering.Vulkan;
using Silk.NET.Windowing.Glfw;


namespace SpatialSim.Engine.Core
{
    public static class Window
    {
        public static Vector2 size;
        public static Vector2 maxSize;
        public static Vector2 windowScale;
        public static Vector2 scaleFromBase;
        
        static Action init;
        static Action<float> update;
        static Action<float> fixedUpdate;
        
        public static void Init(Action init, Action<float> update, Action<float> fixedUpdate)
        {
            Resources.Init();
            //load logging before anything
            Debug.Init();
            
            Window.init = init;
            Window.update = update;
            Window.fixedUpdate = fixedUpdate;

            GraphicsAPI graphicsApi = GraphicsAPI.DefaultVulkan with
            {
                Version = new APIVersion(1, 4)
            };
            
            //switch based on api but only vulkan for now
            AppState.appContext = new VkContext(graphicsApi);
            size = AppState.WindowStartSize;
            WindowOptions options = WindowOptions.DefaultVulkan with
            {
                Size = new Vector2D<int>((int)size.X, (int)size.Y),
                Title = AppState.WindowTitle,
                API = graphicsApi,
                VSync = true,
                WindowBorder = WindowBorder.Resizable
            };

            //make sure running on glfw
            GlfwWindowing.RegisterPlatform();
            GlfwWindowing.Use();
            
            AppState.window = Silk.NET.Windowing.Window.Create(options);
            
            AppState.Api = AppState.window.API.API + " " + AppState.window.API.Version.MajorVersion + "." +
                           AppState.window.API.Version.MinorVersion;
            
            Debug.LogInfo("Running on Api " + AppState.Api);
            Debug.LogInfo("Running on Windowing Backend " + AppState.window.GetType().Name);
            
            AppState.window.Load += Load;
            AppState.window.Update += Update;
            AppState.window.Render += Render;
            AppState.window.Resize += WindowResize;

            AppState.window.Run();
            
            Clean();
        }

        static void Load()
        {
            ShaderManager.Init();
            EcsManager.Init();
            Input.Init();
            
            AppState.appContext.Init();
            
            MainImgui.SetImGuiStyle();
            
            AppState.window.WindowState = WindowState.Fullscreen;
            maxSize = (Vector2)AppState.window.GetFullSize();
            AppState.window.WindowState = WindowState.Normal;
            size = (Vector2)AppState.window.FramebufferSize;
            windowScale = size / (Vector2)AppState.window.Size;
            scaleFromBase = size / AppState.WindowStartSize;
        }

        static void Update(double delta)
        {
            AppState.deltaTime = (float)delta;
            AppState.totalTime += (ulong)(delta * 1000000);
            
            Input.Update();
            EcsManager.Update();
            
            AppState.appContext.Update((float)delta);
            
            if (Input.IsKeyDown(Key.Escape))
            {
                AppState.window.Close();
            }
        }

        static void WindowResize(Vector2D<int> vector2D)
        {
            size = (Vector2)AppState.window.FramebufferSize;
            windowScale = size / (Vector2)AppState.window.Size;
            scaleFromBase = size / AppState.WindowStartSize;
            
            AppState.appContext.WindowResize();
        }

        static void Render(double delta)
        {
            MainImgui.MainMenu();
            AppState.appContext.Render();
        }

        static unsafe void Clean()
        {
            ShaderManager.Clean();
            Input.Clean();
            AppState.appContext.CleanObjects();
            EcsManager.Clean();
            AppState.appContext.CleanContext();
            AppState.window.Dispose();
        }
    }
}