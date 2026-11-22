using SpatialSim.Engine.Rendering;

namespace SpatialSim.Game.Rendering
{
    public class TestComputePipeline : Pipeline
    {
        Texture test;
        
        public TestComputePipeline(string name) : base(name)
        {
            this.name = name;
            test = new Texture();
            test.data = new TextureData();
            test.data.info = new TextureInfo();
            test.data.info.width = 64;
            test.data.info.height = 64;
            test.data.info.depth = 1;
            test.data.info.format = TextureFormat.R8G8B8A8Unorm;
            test.data.info.usage = TextureUsage.Sampler | TextureUsage.Storage;
            test.data.info.memoryUsage = TextureMemoryUsage.gpu;
            test.Create();
            TextureManager.LoadTexture("compute", test);
        }

        public override void SetData(in CommandBuffer commandBuffer, int uniformBinding)
        {
            Shader computeShader = ShaderManager.RetrieveShader(shaderNames[0]);
            int textureIndex = TextureManager.RetrieveTextureIndex("compute");
            computeShader.AddData(uniformBinding, textureIndex);
            UpdateUniforms(computeShader, uniformBinding);
            commandBuffer.BindUniforms(this, uniformBinding, ShaderType.Compute);
            commandBuffer.BindImageStorage(
                this,
                TextureManager.RetrieveTexture(textureIndex),
                0,
                ShaderType.Compute);
        }
    }
}