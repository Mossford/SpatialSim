using System.Numerics;
using SpatialSim.Engine.Core;
using SpatialSim.Engine.Core.Vulkan;

namespace SpatialSim.Engine.Rendering
{
    public class MeshRenderer : IComponent
    {
        public EcsComponentType type => EcsComponentType.MeshRenderer;
        public int id { get; set; } = -1;

        public EcsComponentRef mesh 
        {
            get;
            set
            {
                //make sure we update the mesh reference if we update it
                field = value;
                meshRef = EcsManager.GetComponent<Mesh>(mesh);
            }
        }
        public EcsComponentRef material
        {
            get;
            set
            {
                //make sure we update the material reference if we update it
                field = value;
                materialRef = EcsManager.GetComponent<Material>(material);
            }
        }
        public EcsComponentRef camera
        {
            get;
            set
            {
                //make sure we update the camera reference if we update it
                field = value;
                cameraRef = EcsManager.GetComponent<Camera>(camera);
            }
        }

        public Buffer<Vertex> vertexBuffer;
        public Buffer<int> indexBuffer;

        public Mesh meshRef { get; private set; }
        public Material materialRef { get; private set; }
        public Camera cameraRef { get; private set; }
        public string pipelineRef;

        public MeshRenderer()
        {
            
        }

        public MeshRenderer(EcsComponentRef mesh, EcsComponentRef material, EcsComponentRef camera, string pipeline = "")
        {
            mesh.CheckComponent(EcsComponentType.Mesh);
            material.CheckComponent(EcsComponentType.Material);
            this.mesh = mesh;
            this.material = material;
            this.camera = camera;
            pipelineRef = pipeline;
            
            Create();
        }

        public void Create()
        {
            vertexBuffer = new Buffer<Vertex>();
            vertexBuffer.Create(new Span<Vertex>(meshRef.GetVertexes()), BufferUsage.Vertex, BufferMemoryUsage.Cpu);
            indexBuffer = new Buffer<int>();
            indexBuffer.Create(new Span<int>(meshRef.meshData.indices), BufferUsage.Index, BufferMemoryUsage.Cpu);
        }

        public void UpdateMesh()
        {
            vertexBuffer.UpdateData(meshRef.GetVertexes());
            indexBuffer.UpdateData(meshRef.meshData.indices);
        }

        public void Draw(CommandBuffer commandBuffer, int frame)
        {
            Pipeline pipeline = PipelineManager.RetrievePipeline(pipelineRef);
            commandBuffer.BindPipeLine(pipeline);
            commandBuffer.SetViewport(Window.size);
            commandBuffer.SetScissor(Window.size);
            
            commandBuffer.BindVertexBuffers(vertexBuffer.buffer!);
            commandBuffer.BindIndexBuffers(indexBuffer.buffer!);
            pipeline.SetDrawData(commandBuffer, this, 0);
            commandBuffer.Draw(meshRef.meshData.indices.Length);
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