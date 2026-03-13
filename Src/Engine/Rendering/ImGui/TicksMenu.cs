using SpatialSim.Engine.Core;
using SpatialSim.Engine.Core.Vulkan;

namespace SpatialSim.Engine.Rendering.ImGui
{
    public static class TicksMenu
    {
        public static bool show = false;
        
        public static void Show()
        {
            if(!ImGuiNET.ImGui.Begin("VkTick Menu", ref show))
            {
                ImGuiNET.ImGui.End();
            }
            else
            {
                ImGuiNET.ImGui.TextWrapped("Buffer Count: " + Ticks.bufferCount);
                ImGuiNET.ImGui.TextWrapped("Pipeline Count: " + Ticks.pipelineCount);
                ImGuiNET.ImGui.TextWrapped("Command Buffer Count: " + Ticks.commandBufferCount);
                ImGuiNET.ImGui.TextWrapped("SwapChain Recreations: " + Ticks.swapchainRecreations);
                ulong gpuMem = Ticks.gpuMemoryAllocation.total;
                string unit = "B";
                if (gpuMem >= 1 << 10)
                {
                    gpuMem >>= 10;
                    unit = "KiB";
                }
                if (gpuMem >= 1 << 20)
                {
                    gpuMem >>= 10;
                    unit = "MiB";
                }
                if (gpuMem >= 1 << 30)
                {
                    gpuMem >>= 10;
                    unit = "GiB";
                }
                ImGuiNET.ImGui.TextWrapped("Gpu Memory allocation: " + gpuMem + unit);
                
                ImGuiNET.ImGui.End();
            }
        }
    }
}