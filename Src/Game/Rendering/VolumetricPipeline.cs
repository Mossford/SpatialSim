using System.Numerics;
using SpatialSim.Engine.Core;
using SpatialSim.Engine.Rendering;
using SpatialSim.Game.Math;

namespace SpatialSim.Game.Rendering
{
    public class VolumetricPipeline : Pipeline
    {
        public VolumetricPipeline(string name) : base(name)
        {
            this.name = name;
        }

        public override void SetDrawData(in CommandBuffer commandBuffer, in MeshRenderer meshRenderer, int binding)
        {
            Shader fragmentShader = ShaderManager.RetrieveShader(shaders[1]);
            fragmentShader.AddData(binding, meshRenderer.cameraRef.proj);
            fragmentShader.AddData(binding, meshRenderer.cameraRef.view);
            fragmentShader.AddData(binding, meshRenderer.cameraRef.transformRef.position);
            fragmentShader.AddData(binding, (float)AppState.GetSeconds());
            fragmentShader.AddData(binding, Window.size);
            fragmentShader.AddData(binding, meshRenderer.cameraRef.fov);
            fragmentShader.AddData(binding, (uint)100);
            fragmentShader.AddData(binding, new Vector4(0, 0, 1, MathUtil.GetScaleFromAngularSize(80 / 60f)));
            UpdateUniforms(fragmentShader, binding);
            commandBuffer.BindFragmentUniforms(this, binding);
        }
    }
}