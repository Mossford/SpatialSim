using System.Numerics;
using SpatialSim.Engine.Core;
using SpatialSim.Engine.Rendering;

namespace SpatialSim.Game
{
    public static class GameManager
    {
        public static void Init()
        {
            Entity camera = EcsManager.AddEntity();
            EcsComponentRef cameraRef = camera.AddComponent(new Camera(
                camera.AddComponent(
                    new Transform(
                        new Vector3(0f), 
                        Quaternion.CreateFromYawPitchRoll(0, 0, 0), 
                        new Vector3(1.0f))), 
                2f * MathF.Atan2(23.9f, 2f * 600) * 180f / MathF.PI));

            Entity mesh = EcsManager.AddEntity();
            EcsComponentRef transform = mesh.AddComponent(new Transform(
                new Vector3(0, 0, 1),
                Quaternion.CreateFromYawPitchRoll(90 * MathF.PI / 180f, 0, 0),
                new Vector3(MathF.Tan(31 / 60f * MathF.PI / 180f / 2f))));
            EcsComponentRef meshRef = mesh.AddComponent(new Mesh(MeshGeneration.CreateSphereMesh(1f, 4), transform));
            mesh.AddComponent(new MeshRenderer(meshRef, mesh.AddComponent(new Material
            {
                textureRef = "3840px-Moon_texture.jpg"
            }), cameraRef));
        }

        public static void Update(float dt)
        {
            
        }

        public static void FixedUpdate(float dt)
        {
            
        }
    }
}