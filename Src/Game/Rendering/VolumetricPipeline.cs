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

        public override void SetDrawData(in CommandBuffer commandBuffer, in MeshRenderer meshRenderer, int uniformBinding)
        {
            Shader fragmentShader = ShaderManager.RetrieveShader(shaders[1]);
            fragmentShader.AddData(uniformBinding, meshRenderer.cameraRef.proj);
            fragmentShader.AddData(uniformBinding, meshRenderer.cameraRef.view);
            fragmentShader.AddData(uniformBinding, meshRenderer.cameraRef.transformRef.position);
            fragmentShader.AddData(uniformBinding, (float)AppState.GetSeconds());
            fragmentShader.AddData(uniformBinding, Window.size);
            fragmentShader.AddData(uniformBinding, meshRenderer.cameraRef.fov);
            fragmentShader.AddData(uniformBinding, (uint)100);
            fragmentShader.AddData(uniformBinding, new Vector4(0, 0, 1, MathUtil.GetScaleFromAngularSize(80 / 60f)));
            UpdateUniforms(fragmentShader, uniformBinding);
            commandBuffer.BindFragmentUniforms(this, uniformBinding);
        }
    }
}