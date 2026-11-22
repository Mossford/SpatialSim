using System.Numerics;
using SpatialSim.Engine.Core;
using SpatialSim.Engine.Core.Vulkan;

namespace SpatialSim.Engine.Rendering
{
    public class MeshRenderer : IComponent
    {
        public int type => EcsComponentType.MeshRenderer.GetId();
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
        
        public EcsComponentRef transform
        {
            get;
            set
            {
                //make sure we update the transform reference if we update it
                field = value;
                transformRef = EcsManager.GetComponent<Transform>(transform);
            }
        }
        
        public Transform transformRef { get; private set; }
        public Mesh meshRef { get; private set; }
        public Material materialRef { get; private set; }
        public Camera cameraRef { get; private set; }
        public string pipelineRef;

        public MeshRenderer()
        {
            
        }

        public MeshRenderer(EcsComponentRef mesh, EcsComponentRef material, EcsComponentRef camera, EcsComponentRef transform, string pipeline = "")
        {
            mesh.CheckComponent(EcsComponentType.Mesh.GetId());
            material.CheckComponent(EcsComponentType.Material.GetId());
            camera.CheckComponent(EcsComponentType.Camera.GetId());
            transform.CheckComponent(EcsComponentType.Transform.GetId());
            this.mesh = mesh;
            this.material = material;
            if (pipeline.Length == 0)
            {
                pipeline = materialRef.normalMapRef.Length == 0 ? DefaultPipelines.BaseColor : DefaultPipelines.BaseNormalMap;
            }
            this.camera = camera;
            this.transform = transform;
            pipelineRef = pipeline;
        }
        
        public MeshRenderer(EcsComponentRef mesh, EcsComponentRef material, EcsComponentRef transform, string pipeline = "")
        {
            mesh.CheckComponent(EcsComponentType.Mesh.GetId());
            material.CheckComponent(EcsComponentType.Material.GetId());
            transform.CheckComponent(EcsComponentType.Transform.GetId());
            this.mesh = mesh;
            this.material = material;
            this.transform = transform;
            if (pipeline.Length == 0)
            {
                pipeline = materialRef.normalMapRef.Length == 0 ? DefaultPipelines.BaseColor : DefaultPipelines.BaseNormalMap;
            }
            pipelineRef = pipeline;
        }

        public void Draw(CommandBuffer commandBuffer)
        {
            Pipeline pipeline = PipelineManager.RetrievePipeline(pipelineRef);
            commandBuffer.BindVertexBuffers(meshRef.vertexBuffer.buffer!);
            commandBuffer.BindIndexBuffers(meshRef.indexBuffer.buffer!);
            pipeline.SetData(commandBuffer, this, 0);
            commandBuffer.Draw(meshRef.meshData.indices.Length);
        }

        public void Dispose()
        {
            
        }
    }
}