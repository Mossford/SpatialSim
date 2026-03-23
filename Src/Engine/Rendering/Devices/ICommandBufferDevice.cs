using System.Numerics;

namespace SpatialSim.Engine.Rendering
{
    public interface ICommandBufferDevice
    {
        public void Create();
        public void Clean();

        public void BindVertexBuffers<T>(IBufferDevice<T> bufferDevice) where T : unmanaged;
        public void BindIndexBuffers<T>(IBufferDevice<T> bufferDevice) where T : unmanaged;
        public void Begin();
        public void BeginOneUse();
        public void EndCommandBuffer();
        public void Submit();
        public void BeingRendering(int frame);
        public void EndRendering();
        public void BindPipeLine(Pipeline pipeline);
        public void SetViewport(Vector2 size);
        public void SetScissor(Vector2 size);
        public void BindVertexUniforms(Pipeline pipeline, int binding);
        public void BindFragmentUniforms(Pipeline pipeline, int binding);
        public void BindTexture(Pipeline pipeline, Texture texture);
        public void Draw(int indexCount);
        public void ResetPipeLine(Pipeline pipeline);
    }
}