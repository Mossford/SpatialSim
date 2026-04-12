using System.Numerics;
using Silk.NET.Input;
using SpatialSim.Engine.Core;
using SpatialSim.Engine.Rendering;
using SpatialSim.Game.ImGui;
using SpatialSim.Game.Math;
using SpatialSim.Game.Rendering;

namespace SpatialSim.Game
{
    public static class GameManager
    {

        public static CameraController cameraController;
        static Entity moon;
        
        public static void Init()
        {
            PipelineManager.LoadPipeline(new VolumetricPipeline("Volumetric"), [
                ShaderManager.RetrieveShader(
                    new ShaderSettings(
                        ShaderType.Vertex,
                        [new ShaderDescriptorDef(RendererSettings.VertexUniformSet, [0], ShaderDescriptorUsage.Uniform, ShaderType.Vertex)],
                        "volumetric.vert")),
                ShaderManager.RetrieveShader(
                    new ShaderSettings(ShaderType.Fragment,
                        [
                            new ShaderDescriptorDef(RendererSettings.FragmentSamplerSet, [0], ShaderDescriptorUsage.Sampler, ShaderType.Fragment), 
                            new ShaderDescriptorDef(RendererSettings.FragmentUniformSet, [0], ShaderDescriptorUsage.Uniform, ShaderType.Fragment)
                        ],
                        "volumetric.frag"))
            ]);
            
            Entity camera = EcsManager.AddEntity();
            EcsComponentRef cameraRef = camera.AddComponent(new Camera(
                camera.AddComponent(
                    new Transform(
                        new Vector3(), 
                        new Vector3(), 
                        new Vector3(1.0f))), 
                 MathUtil.GetFovFromFocalLength(23.9f,130)));

            cameraController = new CameraController(cameraRef);
            MainImgui.menus.Add(new CameraMenu());
            
            moon = EcsManager.AddEntity();
            {
                EcsComponentRef transform = moon.AddComponent(new Transform(
                    new Vector3(0, 0, 1),
                    new Vector3(85 * MathF.PI / 180f, 253 * MathF.PI / 180f, 0),
                    new Vector3(MathF.Tan(35 / 60f * MathF.PI / 180f / 2f))));
                EcsComponentRef meshRef = moon.AddComponent(
                    new Mesh(ModelLoader.LoadModelFile("UvSphere.fbx"),
                        transform));
                moon.AddComponent(new MeshRenderer(meshRef, moon.AddComponent(new Material
                {
                    textureRef = "moonColor.png",
                    normalMapRef = "moonNormal.png",
                }), cameraRef));   
            }
            
            Entity screenQuad = EcsManager.AddEntity();
            {
                EcsComponentRef transform = screenQuad.AddComponent(new Transform(
                    new Vector3(0, 0, 0),
                    new Vector3(),
                    new Vector3(1f)));
                EcsComponentRef meshRef = screenQuad.AddComponent(new Mesh(MeshGeneration.Create2DQuad(), transform));
                screenQuad.AddComponent(new MeshRenderer(
                    meshRef, 
                    screenQuad.AddComponent(
                        new Material()), 
                    cameraRef,
                    "Volumetric"));
            }
        }

        public static void Update(float dt)
        {
            cameraController.Update();
        }

        public static void FixedUpdate(float dt)
        {
            
        }
    }
}