using System.Numerics;
using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using SpatialSim.Engine.Core.Vulkan;
using SpatialSim.Engine.Rendering;
using SpatialSim.Engine.Rendering.Vulkan;
using Silk.NET.Windowing.Glfw;
using SpatialSim.Engine.Core.SDLGpu;
using SpatialSim.Engine.Rendering.ImGui;


namespace SpatialSim.Engine.Core
{
    public static class Window
    {
        public static Vector2 size;
        public static Vector2 maxSize;
        public static Vector2 windowScale;
        public static Vector2 scaleFromBase;

        static double updateStartTime;
        static double renderStartTime;
        public static double updateTime;
        public static double renderTime;
        
        static Action init;
        static Action<float> update;
        static Action<float> fixedUpdate;
        
        public static void Init(Action init, Action<float> update, Action<float> fixedUpdate)
        {
            Resources.Init();
            //load logging before anything
            Debug.Init();

            Ticks.startUpTimer.SignalStart(Timer.TimerMode.Milliseconds);

            Window.init = init;
            Window.update = update;
            Window.fixedUpdate = fixedUpdate;

            GraphicsAPI graphicsApi = default;

            //If we select sdlGpu, we might have to use sdl as the windowing platform
            switch (AppState.renderingApi)
            {
                case RenderingApi.Vulkan:
                {
                    graphicsApi = GraphicsAPI.DefaultVulkan with
                    {
                        Version = new APIVersion(1, 4)
                    };
                    AppState.appContext = new VkContext(graphicsApi);
                    break;
                }
                case RenderingApi.SDLGpu:
                {
                    AppState.appContext = new SDLGpuContext();
                    break;
                }
            }
            
            size = AppState.WindowStartSize;
            WindowOptions options = WindowOptions.Default with
            {
                Size = new Vector2D<int>((int)size.X, (int)size.Y),
                Title = AppState.WindowTitle,
                API = graphicsApi,
                VSync = true,
                WindowBorder = WindowBorder.Resizable,
                TransparentFramebuffer = false,
            };

            //make sure running on glfw
            GlfwWindowing.RegisterPlatform();
            GlfwWindowing.Use();

            AppState.window = Silk.NET.Windowing.Window.Create(options);

            AppState.Api = AppState.window.API.API + " " + AppState.window.API.Version.MajorVersion + "." +
                           AppState.window.API.Version.MinorVersion;
            
            Debug.LogInfo("Running on Api: " + AppState.Api);
            Debug.LogInfo("Running on Windowing Backend: " + AppState.window.GetType().Name);
            
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
            
            Debug.LogInfo("Running on Device: " + AppState.gpuDeviceName);
            
            PipelineManager.Init();
            TextureManager.Init();
            PostProcessManager.Init();
            
            MainImgui.Init();
            
            init.Invoke();
            
            maxSize = (Vector2)AppState.window.Monitor!.Bounds.Size;
            size = (Vector2)AppState.window.FramebufferSize;
            windowScale = size / (Vector2)AppState.window.Size;
            scaleFromBase = size / AppState.WindowStartSize;

            Ticks.startUpTimer.SignalEnd();

            Debug.LogInfo($"Load time took {Ticks.startUpTimer.elapsed}ms");
        }

        static void Update(double delta)
        {
            updateStartTime = AppState.window.Time * 1000000;
            
            AppState.deltaTime = (float)delta;
            AppState.totalTime += (ulong)(delta * 1000000);
            
            Input.Update();
            EcsManager.Update();
            
            update.Invoke((float)delta);
            
            AppState.appContext.Update((float)delta);
            
            if (Input.IsKeyDown(Key.Escape))
            {
                AppState.window.Close();
            }

            updateTime = AppState.window.Time * 1000000 - updateStartTime;
        }

        static void WindowResize(Vector2D<int> vector2D)
        {
            size = (Vector2)AppState.window.FramebufferSize;
            windowScale = size / (Vector2)AppState.window.Size;
            scaleFromBase = size / AppState.WindowStartSize;
            
            AppState.appContext.WindowResize();
            PostProcessManager.RecreatePostProcesses();
        }

        static void Render(double delta)
        {
            renderStartTime = AppState.window.Time * 1000000;
            MainImgui.MainMenu();
            AppState.appContext.Render();
            AppState.appContext.FinishRender();
            renderTime = AppState.window.Time * 1000000 - renderStartTime;
        }

        static unsafe void Clean()
        {
            ShaderManager.Clean();
            Input.Clean();
            AppState.appContext.CleanObjects();
            PipelineManager.Clean();
            TextureManager.Clean();
            PostProcessManager.Clean();
            EcsManager.Clean();
            AppState.appContext.CleanContext();
            AppState.window.Dispose();
            
            Debug.CompressLog();
        }
    }
}
