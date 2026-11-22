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
        public void BindUniforms(Pipeline pipeline, int binding, ShaderType shaderType);
        public void BindSamplers(Pipeline pipeline, Texture[] textures, int[] bindings, ShaderType shaderType);
        public void BindImageStorage(Pipeline pipeline, Texture texture, int binding, ShaderType shaderType);
        public void BindBufferStorage<T>(Pipeline pipeline, Buffer<T>[] buffers, int[] bindings, ShaderType shaderType) where T : unmanaged;
        public void Draw(int indexCount);
        public void DispatchCompute(int sizeX, int sizeY, int sizeZ);
        public void ResetPipeLine(Pipeline pipeline);
    }
}