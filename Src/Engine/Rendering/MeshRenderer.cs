using System.Numerics;
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
            Mesh meshComp = ((Mesh)EcsManager.GetComponent(mesh));
            meshComp.CreateModelMatrix();
            Shader vertexShader = ShaderManager.RetrieveShader(new ShaderSettings(ShaderType.Vertex, "base.vert"));
            // TODO Add support so that this is referenced without needing to shove it in here
            vertexShader.AddMat4(Matrix4x4.CreateLookAt(new Vector3(MathF.Sin((float)AppState.GetSeconds()) * 13, 3, MathF.Cos((float)AppState.GetSeconds()) * 13), new Vector3(0, 0, 0), new Vector3(0, -1, 0)));
            vertexShader.AddMat4(Matrix4x4.CreatePerspectiveFieldOfView(45 * MathF.PI / 180.0f, Window.size.X / Window.size.Y, 0.01f, 20.0f));
            vertexShader.AddMat4(meshComp.modelMat);
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