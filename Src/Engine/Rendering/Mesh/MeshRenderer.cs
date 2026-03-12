using System.Numerics;
using SpatialSim.Engine.Core;
using SpatialSim.Engine.Core.Vulkan;

namespace SpatialSim.Engine.Rendering
{
    public class MeshRenderer : IComponent
    {
        public EcsComponentType type => EcsComponentType.MeshRenderer;
        public int id { get; set; } = -1;

        public EcsComponentRef mesh;
        public EcsComponentRef material;

        public Buffer<Vertex> vertexBuffer;
        public Buffer<int> indexBuffer;

        public MeshRenderer()
        {
            
        }

        public MeshRenderer(EcsComponentRef mesh, EcsComponentRef material)
        {
            mesh.CheckComponent(EcsComponentType.Mesh);
            material.CheckComponent(EcsComponentType.Material);
            this.mesh = mesh;
            this.material = material;
            
            Create();
        }

        public void Create()
        {
            Mesh meshComp = ((Mesh)EcsManager.GetComponent(mesh));
            vertexBuffer = new Buffer<Vertex>();
            vertexBuffer.Create(new Span<Vertex>(meshComp.GetVertexes()), BufferUsage.Vertex, BufferMemoryUsage.Cpu);
            indexBuffer = new Buffer<int>();
            indexBuffer.Create(new Span<int>(meshComp.meshData.indices), BufferUsage.Index, BufferMemoryUsage.Cpu);
        }

        public void Draw(CommandBuffer commandBuffer, int frame)
        {
            commandBuffer.BindVertexBuffers(vertexBuffer.buffer!);
            commandBuffer.BindIndexBuffers(indexBuffer.buffer!);
            Mesh meshComp = EcsManager.GetComponent<Mesh>(mesh);
            Shader vertexShader = ShaderManager.RetrieveShader(new ShaderSettings(ShaderType.Vertex, "base.vert"));
            Camera camera = (Camera)AppState.appContext.GetContext<VkContext>().camera
                .GetFirstComponentOfType(EcsComponentType.Camera);
            vertexShader.AddMat4(camera.view);
            vertexShader.AddMat4(camera.proj);
            vertexShader.AddMat4(EcsManager.GetComponent<Transform>(meshComp.transform).GetModelMat());
            AppState.appContext.defaultPipeline.UpdateUniforms(vertexShader, frame);
            commandBuffer.BindUniforms(AppState.appContext.defaultPipeline);
            commandBuffer.Draw(meshComp.meshData.indices.Length);
        }

        public void Clean()
        {
            vertexBuffer.Clean();
            indexBuffer.Clean();
        }

        public void Dispose()
        {
            Clean();
        }
    }
}