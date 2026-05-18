using SpatialSim.Engine.Rendering.ImGui;
using SpatialSim.Game.Math;

namespace SpatialSim.Game.ImGui
{
    public class CameraMenu : ImGuiMenu
    {
        public CameraMenu()
        {
            name = "Camera";
        }

        public override void Show()
        {
            if(!ImGuiNET.ImGui.Begin("Camera Menu", ref show))
            {
                ImGuiNET.ImGui.End();
            }
            else
            {
                ImGuiNET.ImGui.TextWrapped($"Camera focal length " +
                                           $"{MathUtil.GetFocalLengthFromFov(23.9f, GameManager.cameraController.camera.fov):N1}mm");
                ImGuiNET.ImGui.TextWrapped($"Camera fov " +
                                           $"{GameManager.cameraController.camera.fov:N1}");
                ImGuiNET.ImGui.TextWrapped($"Camera dir {GameManager.cameraController.camera.transformRef.GetForward()}");
                ImGuiNET.ImGui.TextWrapped($"Camera pos {GameManager.cameraController.camera.transformRef.position}");
                
                ImGuiNET.ImGui.End();
            }
        }
    }
}