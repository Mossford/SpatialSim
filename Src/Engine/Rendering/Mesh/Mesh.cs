using SpatialSim.Engine.Core;

namespace SpatialSim.Engine.Rendering
{
    public class Mesh : IComponent
    {
        public int type => EcsComponentType.Mesh.GetId();
        public int id { get; set; } = -1;
        
        public MeshData meshData;
        
        public Buffer<Vertex> vertexBuffer;
        public Buffer<int> indexBuffer;
        
        public Mesh()
        {
            
        }

        public Mesh(MeshData meshData)
        {
            this.meshData = meshData;
            
            Create();
        }
        
        public void Create()
        {
            vertexBuffer = new Buffer<Vertex>();
            vertexBuffer.Create(new Span<Vertex>(GetVertexes()), BufferUsage.Vertex, BufferMemoryUsage.Cpu);
            indexBuffer = new Buffer<int>();
            indexBuffer.Create(new Span<int>(meshData.indices), BufferUsage.Index, BufferMemoryUsage.Cpu);
        }

        public void UpdateMesh()
        {
            vertexBuffer.UpdateData(GetVertexes());
            indexBuffer.UpdateData(meshData.indices);
        }

        public Vertex[] GetVertexes()
        {
            return meshData.GetVertexes();
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