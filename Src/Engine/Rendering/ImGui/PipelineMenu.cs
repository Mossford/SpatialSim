using ImGuiNET;
using SpatialSim.Engine.Core;

namespace SpatialSim.Engine.Rendering.ImGui
{
    public class PipelineMenu : ImGuiMenu
    {
        public PipelineMenu()
        {
            name = "Pipeline Viewer";
        }
        
        public override void Show()
        {
            if(!ImGuiNET.ImGui.Begin(name, ref show))
            {
                ImGuiNET.ImGui.End();
            }
            else
            {
                if (ImGuiNET.ImGui.TreeNode("Pipelines"))
                {
                    for (int i = 0; i < PipelineManager.pipelines.Count; i++)
                    {
                        if (ImGuiNET.ImGui.CollapsingHeader($"Pipeline: {PipelineManager.pipelines[i].name}"))
                        {
                            ImGuiNET.ImGui.PushID(i);
                            DrawPipeline(PipelineManager.pipelines[i]);
                            ImGuiNET.ImGui.PopID();
                        }
                    }
                    ImGuiNET.ImGui.TreePop();
                }
                
                ImGuiNET.ImGui.End();
            }
        }

        void DrawPipeline(in Pipeline pipeline)
        {
            bool recreatePipeline = false;
            //Technically the pipelines can have int max and min layer but just keep to -10 to 10
            if (ImGuiNET.ImGui.SliderInt("Layer", ref pipeline.layer, -10, 10))
            {
                EcsRendererManager.SortOrder();
            }
            if (ImGuiNET.ImGui.Checkbox("Blend Colors", ref pipeline.settings.blendColor))
                recreatePipeline = true;
            if (ImGuiNET.ImGui.Checkbox("Depth Test and Write", ref pipeline.settings.depthTest))
                recreatePipeline = true;
            int rasterMode = (int)pipeline.settings.rasterizationMode;
            if (ImGuiNET.ImGui.ArrowButton("##left", ImGuiDir.Left)) 
            {
                if (rasterMode > 0)
                {
                    rasterMode--;
                    pipeline.settings.rasterizationMode = (RasterizationMode)rasterMode;
                    recreatePipeline = true;
                }
            }
            ImGuiNET.ImGui.SameLine(0.0f, 1.0f);
            if (ImGuiNET.ImGui.ArrowButton("##right", ImGuiDir.Right)) 
            {
                if (rasterMode < Enum.GetValuesAsUnderlyingType<RasterizationMode>().Length - 1)
                {
                    rasterMode++; 
                
                    pipeline.settings.rasterizationMode = (RasterizationMode)rasterMode;
                    recreatePipeline = true;
                }
            }
            ImGuiNET.ImGui.SameLine();
            ImGuiNET.ImGui.Text($"Raster Mode: {(RasterizationMode)rasterMode}");

            if (recreatePipeline)
            {
                pipeline.Recreate();
            }
        }
    }
}