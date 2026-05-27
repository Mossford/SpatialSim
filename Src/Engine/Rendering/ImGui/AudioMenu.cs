using System.Numerics;
using SpatialSim.Engine.Audio;

namespace SpatialSim.Engine.Rendering.ImGui
{
    public class AudioMenu : ImGuiMenu
    {
        public AudioMenu()
        {
            name = "Audio Stream Viewer";
        }
        
        public override void Show()
        {
            if(!ImGuiNET.ImGui.Begin(name, ref show))
            {
                ImGuiNET.ImGui.End();
            }
            else
            {
                
                if (ImGuiNET.ImGui.TreeNode("Audio Streams"))
                {
                    for (int i = 0; i < AudioManager.audioStreams.Count; i++)
                    {
                        if (ImGuiNET.ImGui.CollapsingHeader($"Audio Stream: {AudioManager.audioStreams[i].name}"))
                        {
                            ImGuiNET.ImGui.PushID(i);
                            DrawAudioStream(AudioManager.audioStreams[i]);
                            ImGuiNET.ImGui.PopID();
                        }
                    }
                    ImGuiNET.ImGui.TreePop();
                }
                
                ImGuiNET.ImGui.End();
            }
        }

        void DrawAudioStream(AudioStream stream)
        {
            ImGuiNET.ImGui.PlotLines("track", ref stream.samples[0], stream.samples.Length, 0, "", float.MaxValue, float.MaxValue, new Vector2(0, 80f));
        }
    }
}