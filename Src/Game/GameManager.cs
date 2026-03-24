using System.Numerics;
using SpatialSim.Engine.Core;
using SpatialSim.Engine.Rendering;
using SpatialSim.Game.Rendering;

namespace SpatialSim.Game
{
    public static class GameManager
    {
        public static void Init()
        {
            PipelineManager.LoadPipeline(new VolumetricPipeline("Volumetric"), [
                ShaderManager.RetrieveShader(
                    new ShaderSettings(
                        ShaderType.Vertex,
                        [new ShaderDescriptorDef(RendererSettings.VertexUniformSet, 0, ShaderDescriptorUsage.Uniform, ShaderType.Vertex)],
                        "volumetric.vert")),
                ShaderManager.RetrieveShader(
                    new ShaderSettings(ShaderType.Fragment,
                        [
                            new ShaderDescriptorDef(RendererSettings.FragmentSamplerSet, 0, ShaderDescriptorUsage.Sampler, ShaderType.Fragment), 
                            new ShaderDescriptorDef(RendererSettings.FragmentUniformSet, 0, ShaderDescriptorUsage.Uniform, ShaderType.Fragment)
                        ],
                        "volumetric.frag"))
            ]);
            
            Entity camera = EcsManager.AddEntity();
            EcsComponentRef cameraRef = camera.AddComponent(new Camera(
                camera.AddComponent(
                    new Transform(
                        new Vector3(0f), 
                        Quaternion.CreateFromYawPitchRoll(0, 0, 0), 
                        new Vector3(1.0f))), 
                2f * MathF.Atan2(23.9f, 2f * 2400) * 180f / MathF.PI));

            Entity mesh = EcsManager.AddEntity();
            EcsComponentRef transform = mesh.AddComponent(new Transform(
                new Vector3(0, 0, 1),
                Quaternion.CreateFromYawPitchRoll(90 * MathF.PI / 180f, 0, 0),
                new Vector3(MathF.Tan(31 / 60f * MathF.PI / 180f / 2f))));
            EcsComponentRef meshRef = mesh.AddComponent(
                new Mesh(MeshGeneration.CreateSphereMesh(1f, 4),
                    transform));
            mesh.AddComponent(new MeshRenderer(meshRef, mesh.AddComponent(new Material
            {
                textureRef = "3840px-Moon_texture.jpg"
            }), cameraRef));
            
            Entity screenQuad = EcsManager.AddEntity();
            transform = screenQuad.AddComponent(new Transform(
                new Vector3(0, 0, 0),
                Quaternion.Identity,
                new Vector3(1f)));
            meshRef = screenQuad.AddComponent(new Mesh(MeshGeneration.Create2DQuad(), transform));
            screenQuad.AddComponent(new MeshRenderer(
                meshRef, 
                screenQuad.AddComponent(
                    new Material()), 
                cameraRef,
                "Volumetric"));
        }

        public static void Update(float dt)
        {
            
        }

        public static void FixedUpdate(float dt)
        {
            
        }
    }
}