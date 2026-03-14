namespace SpatialSim.Engine.Rendering
{
    public interface ICommandBufferDevice
    {
        public void Create();
        public void Clean();

        public void BindVertexBuffers<T>(IBufferDevice<T> bufferDevice) where T : unmanaged;
        public void BindIndexBuffers<T>(IBufferDevice<T> bufferDevice) where T : unmanaged;
        public void BeginCommandBuffer();
        public void EndCommandBuffer();
        public void SubmitCommandBuffer();
        public void BeginRenderPass(int frame);
        public void BindPipeLine(Pipeline pipeline);
        public void BindUniforms(Pipeline pipeline);
        public void BindTexture(Pipeline pipeline, Texture texture);
        public void Draw(int indexCount);
        public void EndRenderPass();
        public void ResetPipeLine(Pipeline pipeline);
    }
}