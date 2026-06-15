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
        static Entity light;
        
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
                 MathUtil.GetFovFromFocalLength(23.9f,600f)));

            cameraController = new CameraController(cameraRef);
            MainImgui.menus.Add(new CameraMenu());

            Texture noise = Noise.CreateWorleyNoise(new([256, 256], 8));
            TextureManager.LoadTexture("noise", noise);
            
            Texture noiseNormals = Noise.CreateWorleyNoiseNormals(new([256, 256], 8));
            TextureManager.LoadTexture("noiseNormals", noiseNormals);
            
            moon = EcsManager.AddEntity();
            {
                EcsComponentRef transform = moon.AddComponent(new Transform(
                    new Vector3(0, 0, 1),
                    new Vector3(85 * MathF.PI / 180f, 253 * MathF.PI / 180f, 0),
                    MathUtil.GetScaleFromAngularSize(25)));
                EcsComponentRef meshRef = moon.AddComponent(
                    new Mesh(ModelLoader.LoadModelFile("UvSphere.fbx"),
                        transform));
                moon.AddComponent(new MeshRenderer(meshRef, moon.AddComponent(new Material
                {
                    textureRef = "noise",
                    normalMapRef = "noiseNormals",
                }), cameraRef));
            }
            
            light = EcsManager.AddEntity();
            {
                EcsComponentRef transform = light.AddComponent(new Transform(
                    new Vector3(0, 0, 1),
                    new Vector3(85 * MathF.PI / 180f, 253 * MathF.PI / 180f, 0),
                    MathUtil.GetScaleFromAngularSize(35 / 60f)));
                EcsComponentRef meshRef = light.AddComponent(
                    new Mesh(ModelLoader.LoadModelFile("UvSphere.fbx"),
                        transform));
                light.AddComponent(new MeshRenderer(meshRef, light.AddComponent(new Material
                {
                    textureRef = "noise",
                }), cameraRef));
            }
            
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
                        new Material()), 
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
        }

        public static void Update(float dt)
        {
            cameraController.Update();
            
            light.GetFirstComponent<Transform>(EcsComponentType.Transform).position = new Vector3(MathF.Cos((float)AppState.GetSeconds()), 0.01f, 1 + MathF.Sin((float)AppState.GetSeconds()));
        }

        public static void FixedUpdate(float dt)
        {
            
        }
    }
}