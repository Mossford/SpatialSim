using System.Numerics;
using SpatialSim.Engine.Core;
using SpatialSim.Engine.Rendering;

namespace SpatialSim.Game.Rendering
{
    public class VolumetricPipeline : Pipeline
    {
        public VolumetricPipeline(string pipelineName) : base(pipelineName)
        {
            this.pipelineName = pipelineName;
        }

        public override void SetDrawData(in CommandBuffer commandBuffer, in MeshRenderer meshRenderer, int binding)
        {
            //dont set any data
            Shader fragmentShader = ShaderManager.RetrieveShader(shaders[1]);
            fragmentShader.AddData(binding, meshRenderer.cameraRef.proj);
            fragmentShader.AddData(binding, meshRenderer.cameraRef.view);
            fragmentShader.AddData(binding, meshRenderer.cameraRef.transformRef.position);
            fragmentShader.AddData(binding, (float)AppState.GetSeconds());
            fragmentShader.AddData(binding, new Vector4(Window.size, 0, 0));
            UpdateUniforms(fragmentShader, binding);
            commandBuffer.BindFragmentUniforms(this, binding);
        }
    }
}