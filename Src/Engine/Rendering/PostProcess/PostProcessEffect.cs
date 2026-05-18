using Glslang.NET;
using SpatialSim.Engine.Core;

namespace SpatialSim.Engine.Rendering
{
    public class PostProcessEffect : IDisposable
    {
        public IPostProcessDevice? postProcess;
        
        /// <summary>
        /// Will have a specific pipeline attached to it
        /// </summary>
        public string pipeline;
        public string shader;
        public int layer;

        public Texture texture;

        public PostProcessEffect(string pipeline)
        {
            this.pipeline = pipeline;

            Create();
        }

        public void Create()
        {
            texture = new Texture();
            postProcess = AppState.appContext.DeviceFactory.CreatePostProcessDevice(ref texture);
        }

        /// <summary>
        /// Render to stored texture
        /// </summary>
        public void Render(in CommandBuffer commandBuffer, Texture writeTexture)
        {
            commandBuffer.BeginRendering(writeTexture);
            Pipeline pipeline = PipelineManager.RetrievePipeline(this.pipeline);
            commandBuffer.BindPipeLine(pipeline);
            commandBuffer.SetViewport(Window.size);
            commandBuffer.SetScissor(Window.size);
            
            commandBuffer.BindVertexBuffers(PostProcessManager.vertexBuffer.buffer!);
            commandBuffer.BindIndexBuffers(PostProcessManager.indexBuffer.buffer!);
            //This does nothing?
            //pipeline.SetDrawData(commandBuffer, 0);
            commandBuffer.BindSamplers(pipeline, [texture], [0], ShaderType.Fragment);
            commandBuffer.Draw(PostProcessManager.quad.indices.Length);
            commandBuffer.EndRendering();
        }
        
        /// <summary>
        /// Render to swapchain
        /// </summary>
        public void Render(in CommandBuffer commandBuffer, int frame)
        {
            commandBuffer.BeginRendering(frame);
            Pipeline pipeline = PipelineManager.RetrievePipeline(this.pipeline);
            commandBuffer.BindPipeLine(pipeline);
            commandBuffer.SetViewport(Window.size);
            commandBuffer.SetScissor(Window.size);
            
            commandBuffer.BindVertexBuffers(PostProcessManager.vertexBuffer.buffer!);
            commandBuffer.BindIndexBuffers(PostProcessManager.indexBuffer.buffer!);
            //This does nothing?
            //pipeline.SetDrawData(commandBuffer, 0);
            commandBuffer.BindSamplers(pipeline, [texture], [0], ShaderType.Fragment);
            commandBuffer.Draw(PostProcessManager.quad.indices.Length);
            commandBuffer.EndRendering();
        }

        public void EnableRead(in CommandBuffer commandBuffer)
        {
            postProcess?.EnableRead(commandBuffer);
        }

        public void EnableWrite(in CommandBuffer commandBuffer)
        {
            postProcess?.EnableWrite(commandBuffer);
        }

        public void Clean()
        {
            texture.Dispose();
        }

        public void Dispose()
        {
            Clean();
        }
    }
}