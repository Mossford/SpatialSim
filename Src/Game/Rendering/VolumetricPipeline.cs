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
            fragmentShader.AddVec3(binding, new Vector3(Window.size, (float)AppState.GetSeconds()));
            UpdateUniforms(fragmentShader, binding);
            commandBuffer.BindFragmentUniforms(this, binding);
        }
    }
}