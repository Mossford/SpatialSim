namespace SpatialSim.Engine.Rendering.ImGui
{
    public abstract class ImGuiMenu
    {
        public string name;
        public bool show;
        public abstract void Show();
    }
}