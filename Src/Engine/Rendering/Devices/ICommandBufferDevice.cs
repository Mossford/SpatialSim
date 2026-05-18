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
        public void BeginRendering(int frame);
        public void BeginRendering(Texture colorWrite);
        public void EndRendering();
        public void BindPipeLine(Pipeline pipeline);
        public void SetViewport(Vector2 size);
        public void SetScissor(Vector2 size);
        public void BindVertexUniforms(Pipeline pipeline, int binding);
        public void BindFragmentUniforms(Pipeline pipeline, int binding);
        public void BindSamplers(Pipeline pipeline, Texture[] textures, int[] bindings, ShaderType shaderType);
        public void Draw(int indexCount);
        public void ResetPipeLine(Pipeline pipeline);
    }
}