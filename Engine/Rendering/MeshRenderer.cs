using SpatialSim.Engine.Core;

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

        public void Draw(CommandBuffer commandBuffer)
        {
            commandBuffer.BindVertexBuffers(vertexBuffer.buffer!);
            commandBuffer.BindIndexBuffers(indexBuffer.buffer!);
            Mesh meshComp = ((Mesh)EcsManager.GetComponent(mesh));
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