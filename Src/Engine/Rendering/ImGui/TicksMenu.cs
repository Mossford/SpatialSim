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
                ImGuiNET.ImGui.TextWrapped("SwapChain Recreations: " + Ticks.swapchainRecreations);
                
                
                ImGuiNET.ImGui.End();
            }
        }
    }
}