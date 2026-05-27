using System.Numerics;
using SDL;
using Silk.NET.Windowing;
using SpatialSim.Engine.Audio;
using SpatialSim.Engine.Core.Vulkan;
using SpatialSim.Engine.Rendering;
using SpatialSim.Engine.Rendering.Vulkan;
using SpatialSim.Engine.Core.SDLGpu;
using SpatialSim.Engine.Rendering.ImGui;


namespace SpatialSim.Engine.Core
{
    public static class Window
    {
        public static Vector2 size;
        public static Vector2 maxSize;
        public static Vector2 windowScale;

        static double updateStartTime;
        static double renderStartTime;
        public static double updateTime;
        public static double renderTime;
        
        static Action init;
        static Action<float> update;
        static Action<float> fixedUpdate;

        static SDL_InitFlags initFlags = SDL_InitFlags.SDL_INIT_VIDEO | SDL_InitFlags.SDL_INIT_AUDIO;
        static bool quit = false;
        
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
                    AppState.Api = graphicsApi.API + " " + graphicsApi.Version.MajorVersion + "." +
                                   graphicsApi.Version.MinorVersion;
                    break;
                }
                case RenderingApi.SDLGpu:
                {
                    AppState.appContext = new SDLGpuContext();
                    break;
                }
            }
            
            size = AppState.WindowStartSize;
            SDL3.SDL_Init(initFlags);

            unsafe
            {
                AppState.window = SDL3.SDL_CreateWindow("Spatial Sim", (int)size.X, (int)size.Y, SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
                if (AppState.window == null)
                {
                    string error = "" + SDL3.SDL_GetError();
                    Debug.Error($"SDL could not create window {error}");
                    throw new Exception($"SDL could not create window {error}");
                }
            }
            
            Debug.LogInfo("Running on Windowing Backend: SDL");
            
            Load();

            Debug.LogInfo("Running on Api: " + AppState.Api);

            float frameTime = 0;
            float pastTime = 0;
            while (!quit)
            {
                float time = SDL3.SDL_GetTicksNS() / 1e9f;
                frameTime = time - pastTime;
                
                Update(frameTime);
                Render(frameTime);
                
                pastTime = time;
            }
            
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
            AudioManager.Init();
            
            MainImgui.Init();
            
            init.Invoke();

            unsafe
            {
                int x, y;
                SDL3.SDL_GetWindowMaximumSize(AppState.window, &x, &y);
                maxSize.X = x;
                maxSize.Y = y;
                SDL3.SDL_GetWindowSize(AppState.window, &x, &y);
                size.X = x;
                size.Y = y;
                windowScale = new Vector2(SDL3.SDL_GetWindowDisplayScale(AppState.window));
            }

            Ticks.startUpTimer.SignalEnd();

            Debug.LogInfo($"Load time took {Ticks.startUpTimer.elapsed}ms");
        }

        static void Update(double delta)
        {
            updateStartTime = SDL3.SDL_GetTicksNS() / 1000f;
            
            AppState.deltaTime = (float)delta;
            AppState.totalTime = SDL3.SDL_GetTicksNS() / 1000;
            
            Input.UpdateNonEvent();
            
            unsafe
            {
                SDL_Event sdlEvent;
                
                while (SDL3.SDL_PollEvent(&sdlEvent))
                {
                    Input.Update(sdlEvent);

                    if (sdlEvent.type == (uint)SDL_EventType.SDL_EVENT_QUIT)
                    {
                        quit = true;
                    }

                    if (sdlEvent.type == (uint)SDL_EventType.SDL_EVENT_WINDOW_RESIZED)
                    {
                        WindowResize();
                    }
                }
            }
            
            EcsManager.Update();
            
            update.Invoke((float)delta);
            
            AppState.appContext.Update((float)delta);
            
            AudioManager.Update();
            
            if (Input.IsKeyDown(SDL_Scancode.SDL_SCANCODE_ESCAPE))
            {
                quit = true;
            }

            updateTime = SDL3.SDL_GetTicksNS() / 1000f - updateStartTime;
        }

        static void WindowResize()
        {
            unsafe
            {
                int x, y;
                SDL3.SDL_GetWindowSize(AppState.window, &x, &y);
                size.X = x;
                size.Y = y;
                windowScale = new Vector2(SDL3.SDL_GetWindowDisplayScale(AppState.window));
            }
            
            AppState.appContext.WindowResize();
            PostProcessManager.RecreatePostProcesses();
        }

        static void Render(double delta)
        {
            renderStartTime = SDL3.SDL_GetTicksNS() / 1000f;
            MainImgui.MainMenu();
            AppState.appContext.Render();
            AppState.appContext.FinishRender();
            renderTime = SDL3.SDL_GetTicksNS() / 1000f - renderStartTime;
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
            AudioManager.Clean();
            AppState.appContext.CleanContext();
            
            SDL3.SDL_DestroyWindow(AppState.window);
            SDL3.SDL_QuitSubSystem(initFlags);
            SDL3.SDL_Quit();
            
            Debug.CompressLog();
        }
    }
}
