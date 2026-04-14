using System.Numerics;
using ImGuiNET;
using SpatialSim.Engine.Core;

namespace SpatialSim.Engine.Rendering.ImGui
{
    public class MeshMenu : ImGuiMenu
    {
        int selectedMesh;
        MeshRenderer meshRef;
        bool showMesh;
        
        public MeshMenu()
        {
            name = "Mesh Viewer";
            selectedMesh = -1;
        }
        
        public override void Show()
        {
            if(!ImGuiNET.ImGui.Begin(name, ref show))
            {
                ImGuiNET.ImGui.End();
            }
            else
            {
                if (ImGuiNET.ImGui.TreeNode("Meshes"))
                {
                    for (int i = 0; i < EcsManager.componentPools[EcsComponentType.MeshRenderer.GetId()].components.ValueCount; i++)
                    {
                        ImGuiNET.ImGui.PushID(i);
                        ImGuiNET.ImGui.TextWrapped($"Mesh {i}");
                        ImGuiNET.ImGui.SameLine();
                        if (ImGuiNET.ImGui.Button("Select"))
                        {
                            selectedMesh = i;
                            meshRef = (MeshRenderer)EcsManager.componentPools[EcsComponentType.MeshRenderer.GetId()].components
                                .Get(i);
                            showMesh = true;
                        }
                        ImGuiNET.ImGui.PopID();
                    }
                    ImGuiNET.ImGui.TreePop();
                }
                
                ImGuiNET.ImGui.End();
            }

            if (selectedMesh != -1 && showMesh)
            {
                if(!ImGuiNET.ImGui.Begin("Selected Mesh", ref showMesh))
                {
                    ImGuiNET.ImGui.End();
                }
                else
                {
                    ImDrawListPtr drawList = ImGuiNET.ImGui.GetWindowDrawList();
                    Vector2 windowPos = ImGuiNET.ImGui.GetCursorScreenPos();
                    Vector2 windowSize = ImGuiNET.ImGui.GetWindowSize() - new Vector2(25, 25);
                    Vector2 windowExtent = windowPos + windowSize;
                    drawList.AddRectFilled(windowPos, windowExtent,
                        ImGuiNET.ImGui.GetColorU32(new Vector4(0.1f, 0.1f, 0.1f, 1)));

                    for (int i = 0; i < meshRef.meshRef.meshData.indices.Length; i += 3)
                    {
                        int a = meshRef.meshRef.meshData.indices[i];
                        int b = meshRef.meshRef.meshData.indices[i + 1];
                        int c = meshRef.meshRef.meshData.indices[i + 2];
                        
                        Vector2 aPos = meshRef.meshRef.meshData.vertexData.uvs[a] * windowSize + windowPos;
                        Vector2 bPos = meshRef.meshRef.meshData.vertexData.uvs[b] * windowSize + windowPos;
                        Vector2 cPos = meshRef.meshRef.meshData.vertexData.uvs[c] * windowSize + windowPos;
                        
                        drawList.AddLine(aPos, bPos, ImGuiNET.ImGui.GetColorU32(new Vector4(0, 1, 0, 1)));
                        drawList.AddLine(bPos, cPos, ImGuiNET.ImGui.GetColorU32(new Vector4(0, 1, 0, 1)));
                        drawList.AddLine(aPos, cPos, ImGuiNET.ImGui.GetColorU32(new Vector4(0, 1, 0, 1)));
                    }
                
                
                    ImGuiNET.ImGui.End();
                }
            }
        }
    }
}