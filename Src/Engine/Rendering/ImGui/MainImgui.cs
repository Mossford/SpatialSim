

using System.Numerics;
using ImGuiNET;
using SpatialSim.Engine.Core;
using SpatialSim.Engine.Rendering.ImGui;
using SpatialSim.Game.ImGui;

namespace SpatialSim.Engine.Rendering
{
    public static class MainImgui
    {
        public static bool ShowMainWindow = true;
        public static List<ImGuiMenu> menus = new List<ImGuiMenu>();
        public static List<ImguiPopup> popups = new List<ImguiPopup>();
        public const int MAXPOPUPS = 10;
        
        static uint frameCount;
        static double frameAvg;
        static double updateAvg;
        static double renderAvg;
        static float fpsMax;
        static float fpsTime;

        public static void Init()
        {
            SetImGuiStyle();
            menus.Add(new TicksMenu());
            menus.Add(new MeshMenu());
            menus.Add(new PipelineMenu());
        }
        
        public static void MainMenu()
        {
            float curTime = (float)AppState.GetSeconds();

            const ImGuiWindowFlags window_flags = ImGuiWindowFlags.MenuBar;
            
            if(!ShowMainWindow)
                return;
            
            ImGuiNET.ImGui.SetNextWindowSize(new Vector2(500, 400), ImGuiCond.FirstUseEver);
            
            if(!ImGuiNET.ImGui.Begin("SpatialSim", ref ShowMainWindow, window_flags))
            {
                ImGuiNET.ImGui.End();
            }
            else
            {
                float frameRate = 1.0f / AppState.GetDelta();
                
                ImGuiNET.ImGui.TextWrapped("Version: " + AppState.Version);
                ImGuiNET.ImGui.TextWrapped("Gpu: " + AppState.gpuDeviceName);
                ImGuiNET.ImGui.TextWrapped("API: " + AppState.Api);
                ImGuiNET.ImGui.TextWrapped($"{1000f / frameAvg:N3} ms ({frameAvg:N1} FPS)");
                string unit = "us";
                if (updateAvg >= 1000)
                {
                    updateAvg /= 1000;
                    unit = "ms";
                }
                ImGuiNET.ImGui.TextWrapped($"Update Tick: {updateAvg:N3} {unit}");
                unit = "us";
                if (renderAvg >= 1000)
                {
                    renderAvg /= 1000;
                    unit = "ms";
                }
                ImGuiNET.ImGui.TextWrapped($"Render Tick: {renderAvg:N3} {unit}");
                ImGuiNET.ImGui.TextWrapped($"{1.0f / fpsMax * 1000.0f:N3} ms/frame Max ({fpsMax:N1} FPS Max)");
                ImGuiNET.ImGui.TextWrapped($"Time Open {MathF.Floor(curTime / 60.0f):N0}:{curTime:N2}");
                ImGuiNET.ImGui.TextWrapped("ECS GameObject Count " + EcsManager.entities.ValueCount);
                ImGuiNET.ImGui.TextWrapped("ECS Component Count " + EcsManager.totalComponents);
            
                if (fpsMax < frameRate)
                {
                    fpsMax = frameRate;
                }
            
                frameAvg += (frameRate - frameAvg) / (frameCount + 1);
                updateAvg += (Window.updateTime - updateAvg) / (frameCount + 1);
                renderAvg += (Window.renderTime - renderAvg) / (frameCount + 1);
                frameCount++;
                fpsTime += AppState.GetDelta();
                if(fpsTime >= 5)
                {
                    fpsTime = 0f;
                    frameCount = 0;
                    frameAvg = 0;
                }

                if (ImGuiNET.ImGui.BeginMenuBar())
                {
                    if (ImGuiNET.ImGui.BeginMenu("Menus"))
                    {
                        for (int i = 0; i < menus.Count; i++)
                        {
                            ImGuiNET.ImGui.MenuItem(menus[i].name, null, ref menus[i].show);
                        }
                        
                        ImGuiNET.ImGui.EndMenu();
                    }
                    ImGuiNET.ImGui.EndMenuBar();
                }

                ImGuiNET.ImGui.End();
            }

            for (int i = 0; i < menus.Count; i++)
            {
                if (menus[i].show)
                {
                    ImGuiNET.ImGui.SetNextWindowSize(new Vector2(500, 400), ImGuiCond.FirstUseEver);
                    menus[i].Show();
                }
            }

            for (int i = 0; i < popups.Count; i++)
            {
                popups[i].Show();
                if (popups[i].removed)
                {
                    popups.RemoveAt(i);
                }
            }
        }

        public static void CreatePopup(string title, string msg)
        {
            if(popups.Count > MAXPOPUPS)
                return;
            
            popups.Add(new ImguiPopup(title, msg));
        }

        public static void SetImGuiStyle()
        {
            RangeAccessor<Vector4> colors = ImGuiNET.ImGui.GetStyle().Colors;
            colors[(int)ImGuiCol.Text]                   = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
            colors[(int)ImGuiCol.TextDisabled]           = new Vector4(0.50f, 0.50f, 0.50f, 1.00f);
            colors[(int)ImGuiCol.WindowBg]               = new Vector4(0.06f, 0.06f, 0.06f, 0.94f);
            colors[(int)ImGuiCol.ChildBg]                = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
            colors[(int)ImGuiCol.PopupBg]                = new Vector4(0.08f, 0.08f, 0.08f, 0.94f);
            colors[(int)ImGuiCol.Border]                 = new Vector4(0.43f, 0.19f, 0.17f, 0.50f);
            colors[(int)ImGuiCol.BorderShadow]           = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
            colors[(int)ImGuiCol.FrameBg]                = new Vector4(0.73f, 0.25f, 0.28f, 0.40f);
            colors[(int)ImGuiCol.FrameBgHovered]         = new Vector4(0.95f, 0.38f, 0.38f, 0.55f);
            colors[(int)ImGuiCol.FrameBgActive]          = new Vector4(0.56f, 0.41f, 0.38f, 0.58f);
            colors[(int)ImGuiCol.TitleBg]                = new Vector4(0.31f, 0.09f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.TitleBgActive]          = new Vector4(0.51f, 0.29f, 0.11f, 1.00f);
            colors[(int)ImGuiCol.TitleBgCollapsed]       = new Vector4(0.00f, 0.00f, 0.00f, 0.51f);
            colors[(int)ImGuiCol.MenuBarBg]              = new Vector4(0.31f, 0.09f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.ScrollbarBg]            = new Vector4(0.02f, 0.02f, 0.02f, 0.53f);
            colors[(int)ImGuiCol.ScrollbarGrab]          = new Vector4(0.31f, 0.31f, 0.31f, 1.00f);
            colors[(int)ImGuiCol.ScrollbarGrabHovered]   = new Vector4(0.41f, 0.41f, 0.41f, 1.00f);
            colors[(int)ImGuiCol.ScrollbarGrabActive]    = new Vector4(0.51f, 0.51f, 0.51f, 1.00f);
            colors[(int)ImGuiCol.CheckMark]              = new Vector4(1.00f, 0.49f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.SliderGrab]             = new Vector4(1.00f, 0.24f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.SliderGrabActive]       = new Vector4(1.00f, 0.49f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.Button]                 = new Vector4(1.00f, 0.42f, 0.00f, 0.40f);
            colors[(int)ImGuiCol.ButtonHovered]          = new Vector4(0.81f, 0.43f, 0.20f, 1.00f);
            colors[(int)ImGuiCol.ButtonActive]           = new Vector4(0.87f, 0.66f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.Header]                 = new Vector4(1.00f, 0.49f, 0.00f, 0.31f);
            colors[(int)ImGuiCol.HeaderHovered]          = new Vector4(1.00f, 0.59f, 0.00f, 0.80f);
            colors[(int)ImGuiCol.HeaderActive]           = new Vector4(1.00f, 0.59f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.Separator]              = new Vector4(0.60f, 0.27f, 0.00f, 0.50f);
            colors[(int)ImGuiCol.SeparatorHovered]       = new Vector4(0.60f, 0.39f, 0.00f, 0.78f);
            colors[(int)ImGuiCol.SeparatorActive]        = new Vector4(0.60f, 0.39f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.ResizeGrip]             = new Vector4(0.47f, 0.14f, 0.11f, 0.20f);
            colors[(int)ImGuiCol.ResizeGripHovered]      = new Vector4(0.57f, 0.20f, 0.19f, 0.67f);
            colors[(int)ImGuiCol.ResizeGripActive]       = new Vector4(0.57f, 0.20f, 0.20f, 0.95f);
            colors[(int)ImGuiCol.Tab]                    = new Vector4(0.51f, 0.13f, 0.11f, 0.86f);
            colors[(int)ImGuiCol.TabHovered]             = new Vector4(0.55f, 0.13f, 0.11f, 0.80f);
            colors[(int)ImGuiCol.TabActive]              = new Vector4(0.55f, 0.13f, 0.11f, 1.00f);
            colors[(int)ImGuiCol.TabUnfocused]           = new Vector4(0.45f, 0.10f, 0.07f, 0.97f);
            colors[(int)ImGuiCol.TabUnfocusedActive]     = new Vector4(0.45f, 0.11f, 0.09f, 1.00f);
            colors[(int)ImGuiCol.PlotLines]              = new Vector4(0.61f, 0.61f, 0.61f, 1.00f);
            colors[(int)ImGuiCol.PlotLinesHovered]       = new Vector4(1.00f, 0.43f, 0.35f, 1.00f);
            colors[(int)ImGuiCol.PlotHistogram]          = new Vector4(0.90f, 0.70f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.PlotHistogramHovered]   = new Vector4(1.00f, 0.60f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.TableHeaderBg]          = new Vector4(0.19f, 0.19f, 0.20f, 1.00f);
            colors[(int)ImGuiCol.TableBorderStrong]      = new Vector4(0.31f, 0.31f, 0.35f, 1.00f);
            colors[(int)ImGuiCol.TableBorderLight]       = new Vector4(0.23f, 0.23f, 0.25f, 1.00f);
            colors[(int)ImGuiCol.TableRowBg]             = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
            colors[(int)ImGuiCol.TableRowBgAlt]          = new Vector4(1.00f, 1.00f, 1.00f, 0.06f);
            colors[(int)ImGuiCol.TextSelectedBg]         = new Vector4(0.51f, 0.15f, 0.11f, 0.35f);
            colors[(int)ImGuiCol.DragDropTarget]         = new Vector4(1.00f, 1.00f, 0.00f, 0.90f);
            colors[(int)ImGuiCol.NavHighlight]           = new Vector4(0.49f, 0.14f, 0.11f, 1.00f);
            colors[(int)ImGuiCol.NavWindowingHighlight]  = new Vector4(1.00f, 1.00f, 1.00f, 0.70f);
            colors[(int)ImGuiCol.NavWindowingDimBg]      = new Vector4(0.80f, 0.80f, 0.80f, 0.20f);
            colors[(int)ImGuiCol.ModalWindowDimBg]       = new Vector4(0.80f, 0.80f, 0.80f, 0.35f);

            ImGuiNET.ImGui.GetStyle().FrameRounding = 1;
        }
    }
}