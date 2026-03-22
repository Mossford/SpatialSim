using SpatialSim.Engine.Core;
using SpatialSim.Engine.Core.Vulkan;

namespace SpatialSim.Engine.Rendering.ImGui
{
    public static class TicksMenu
    {
        public static bool show = false;
        
        public static void Show()
        {
            if(!show)
                return;
            
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

                double value = gpuMem;
                string unit = "B";

                if (gpuMem >= 1 << 30)
                {
                    value = gpuMem / (double)(1 << 30);
                    unit = "GiB";
                }
                else if (gpuMem >= 1 << 20)
                {
                    value = gpuMem / (double)(1 << 20);
                    unit = "MiB";
                }
                else if (gpuMem >= 1 << 10)
                {
                    value = gpuMem / (double)(1 << 10);
                    unit = "KiB";
                }
                
                ImGuiNET.ImGui.TextWrapped($"Gpu Memory allocation: {value:N2} {unit}");
                
                ImGuiNET.ImGui.End();
            }
        }
    }
}