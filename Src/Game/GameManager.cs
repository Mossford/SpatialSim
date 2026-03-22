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
                camera.AddComponent(new Transform(new Vector3(0f), Quaternion.Identity, new Vector3(1.0f))), 3));

            for (int i = 0; i < 1; i++)
            {
                for (int j = 0; j < 1; j++)
                {
                    ModelLoader.LoadModelFile("cube.obj", "", new Transform(
                        new Vector3(1f, 0, 0),
                        Quaternion.Identity,
                        new Vector3(MathF.Tan(3 * MathF.PI / 180f / 2f))), cameraRef);
                }
            }
        }

        public static void Update(float dt)
        {
            
        }

        public static void FixedUpdate(float dt)
        {
            
        }
    }
}