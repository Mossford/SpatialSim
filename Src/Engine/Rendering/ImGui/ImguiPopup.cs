

namespace SpatialSim.Engine.Rendering.ImGui
{
    public class ImguiPopup
    {
        public string title;
        public string msg;
        public bool removed;

        public ImguiPopup(string title, string msg)
        {
            this.title = title;
            this.msg = msg;
        }

        public void Show()
        {
            if(removed)
                return;
            
            ImGuiNET.ImGui.OpenPopup(title);

            if (ImGuiNET.ImGui.BeginPopupModal(title))
            {
                ImGuiNET.ImGui.TextWrapped(msg);
                
                if (ImGuiNET.ImGui.Button("ok"))
                {
                    removed = true;
                }
                
                ImGuiNET.ImGui.EndPopup();
            }
        }
    }
}
