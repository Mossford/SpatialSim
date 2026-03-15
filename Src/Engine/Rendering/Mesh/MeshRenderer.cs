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
            vertexBuffer.Create(new Span<Vertex>(meshComp.GetVertexes()), BufferUsage.Vertex, BufferMemoryUsage.Gpu);
            indexBuffer = new Buffer<int>();
            indexBuffer.Create(new Span<int>(meshComp.meshData.indices), BufferUsage.Index, BufferMemoryUsage.Gpu);
        }

        public void Draw(CommandBuffer commandBuffer, int frame)
        {
            commandBuffer.BindVertexBuffers(vertexBuffer.buffer!);
            commandBuffer.BindIndexBuffers(indexBuffer.buffer!);
            Mesh meshComp = EcsManager.GetComponent<Mesh>(mesh);
            Shader vertexShader = ShaderManager.RetrieveShader("base.vert");
            Camera camera = (Camera)AppState.appContext.GetContext<VkContext>().camera
                .GetFirstComponentOfType(EcsComponentType.Camera);
            vertexShader.AddMat4(0, camera.view);
            vertexShader.AddMat4(0, camera.proj);
            vertexShader.AddMat4(0, EcsManager.GetComponent<Transform>(meshComp.transform).GetModelMat());
            AppState.appContext.defaultPipeline.UpdateUniforms(vertexShader, 0, frame);
            commandBuffer.BindVertexUniforms(AppState.appContext.defaultPipeline, 0);
            Shader fragmentShader = ShaderManager.RetrieveShader("base.frag");
            fragmentShader.AddVec4(0, new Vector4(MathF.Abs(MathF.Sin((float)AppState.GetSeconds()))));
            fragmentShader.AddVec4(0, new Vector4(1.0f));
            fragmentShader.AddVec4(0, new Vector4(1.0f));
            fragmentShader.AddVec4(0, new Vector4(1.0f));
            AppState.appContext.defaultPipeline.UpdateUniforms(fragmentShader, 0, frame);
            commandBuffer.BindFragmentUniforms(AppState.appContext.defaultPipeline, 0);
            commandBuffer.BindTexture(AppState.appContext.defaultPipeline, TextureManager.RetrieveTexture("uvCheck.jpg"));
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