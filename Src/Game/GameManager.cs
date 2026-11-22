using System.Numerics;
using Silk.NET.Input;
using SpatialSim.Engine.Audio;
using SpatialSim.Engine.Core;
using SpatialSim.Engine.Network;
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
            PipelineManager.LoadPipeline(new VolumetricPipeline("Volumetric")
            {
                settings = new PipelineSettings()
                {
                    blendColor = false
                },
                layer = -1
            }, [
                ShaderManager.RetrieveShader(
                    new ShaderSettings(
                        ShaderType.Vertex,
                        [
                            new ShaderDescriptorDef(RendererSettings.VertexUniformSet, [0],
                                ShaderDescriptorUsage.Uniform, ShaderType.Vertex)
                        ],
                        "volumetric.vert")),
                ShaderManager.RetrieveShader(
                    new ShaderSettings(ShaderType.Fragment,
                        [
                            new ShaderDescriptorDef(RendererSettings.FragmentSamplerSet, [0],
                                ShaderDescriptorUsage.Sampler, ShaderType.Fragment),
                            new ShaderDescriptorDef(RendererSettings.FragmentUniformSet, [0],
                                ShaderDescriptorUsage.Uniform, ShaderType.Fragment)
                        ],
                        "volumetric.frag"))
            ]);
            
            Entity camera = EcsManager.AddEntity();
            EcsComponentRef cameraRef = camera.AddComponent(new Camera(
                camera.AddComponent(
                    new Transform(
                        new Vector3(), 
                        new Vector3(), 
                        1.0f)), 
                 MathUtil.GetFovFromFocalLength(24f, 200)));

            cameraController = new CameraController(cameraRef);
            MainImgui.menus.Add(new CameraMenu());
            
            TextureManager.LoadTexture("moonColor.png", TextureFormat.R8G8B8A8Unorm);
            TextureManager.LoadTexture("moonNormal.png", TextureFormat.R8G8B8A8Unorm);
            
            moon = EcsManager.AddEntity();
            {
                EcsComponentRef transform = moon.AddComponent(new Transform(
                    new Vector3(0, 0, 1),
                    new Vector3(85 * MathF.PI / 180f, 253 * MathF.PI / 180f, 0),
                    MathUtil.GetScaleFromAngularSize(30f / 60f)));
                EcsComponentRef meshRef = moon.AddComponent(
                    new Mesh(ModelLoader.LoadModelFile("UvSphere.fbx")));
                moon.AddComponent(new MeshRenderer(meshRef, moon.AddComponent(new Material
                {
                    textureRef = "moonColor.png",
                    normalMapRef = "moonNormal.png",
                }), cameraRef, transform));
            }
            
            //Texture noise = Noise.CreateWorleyNoise(new([128, 128, 128], 12));
            //TextureManager.LoadTexture("noise", noise);
            
            //Texture noiseNormals = Noise.CreateWorleyNoiseNormals(new([128, 128, 128], 12));
            //TextureManager.LoadTexture("noiseNormals", noiseNormals);
            
            /*Entity screenQuad = EcsManager.AddEntity();
            {
                EcsComponentRef transform = screenQuad.AddComponent(new Transform(
                    new Vector3(0, 0, 0),
                    new Vector3(),
                    1.0f));
                EcsComponentRef meshRef = screenQuad.AddComponent(new Mesh(MeshGeneration.Create2DQuad(), transform));
                screenQuad.AddComponent(new MeshRenderer(
                    meshRef, 
                    screenQuad.AddComponent(
                        new Material()
                        {
                            textureRef = "noise",
                            normalMapRef = "noiseNormals"
                        }),
                    cameraRef,
                    "Volumetric"));
            }*/

            //AudioManager.LoadAudioStream(new AudioStream($"test"));

            /*PostProcessManager.LoadPostProcessEffect(new PostProcessEffect("testeffect"),
                ShaderManager.RetrieveShader(
                    new ShaderSettings(ShaderType.Fragment,
                        [
                            new ShaderDescriptorDef(RendererSettings.FragmentSamplerSet, [0],
                                ShaderDescriptorUsage.Sampler, ShaderType.Fragment)
                        ],
                        "testeffect.frag")));*/
            
            /*PipelineManager.LoadPipeline(new TestComputePipeline("ComputeTest")
            {
                settings = new PipelineSettings()
                {
                    type = PipelineType.Compute,
                },
                layer = -1
            }, [
                ShaderManager.RetrieveShader(
                    new ShaderSettings(
                        ShaderType.Compute,
                        [
                            new ShaderDescriptorDef(RendererSettings.ComputeImageStorageSet, [0],
                                ShaderDescriptorUsage.ImageStorage, ShaderType.Compute),
                            new ShaderDescriptorDef(RendererSettings.ComputeUniformSet, [0],
                                ShaderDescriptorUsage.Uniform, ShaderType.Compute)
                        ],
                        "testCompute.comp")),
            ]);*/
        }

        public static void Update(float dt)
        {
            cameraController.Update();
        }

        public static void FixedUpdate(float dt)
        {
            
        }

        public static void Clean()
        {
            
        }
    }
}