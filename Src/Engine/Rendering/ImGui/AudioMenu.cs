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
                    for (int i = 0; i < AudioManager.audioStreams.Length; i++)
                    {
                        if(AudioManager.audioStreams[i] is null)
                            continue;
                        
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
            float[] samples = stream.samples;
            int count = samples.Length;

            int divisor = 2;
            ImGuiNET.ImGui.SliderInt("Division", ref divisor, 2, 16);
            int visibleSamplesMax = count / divisor;

            int trigger = 0;

            for (int i = 1; i < count - 128; i++)
            {
                if (samples[i - 1] < 0f && samples[i] >= 0f)
                {
                    trigger = i;
                    break;
                }
            }

            int visibleSamples = Math.Min(visibleSamplesMax, count - trigger);

            float[] visible = new float[visibleSamples];
            Array.Copy(samples, trigger, visible, 0, visibleSamples);

            ImGuiNET.ImGui.PlotLines("Wave", ref visible[0], visible.Length, 0, "", -AudioManager.globalVolume, AudioManager.globalVolume, new Vector2(0, 120f));

            ImGuiNET.ImGui.PlotLines("Current Sample", ref stream.samples[0], stream.samples.Length, 0, "", -AudioManager.globalVolume, AudioManager.globalVolume, new Vector2(0, 120f));
        }
    }
}