using System.Numerics;
using SpatialSim.Engine.Core;

namespace SpatialSim.Engine.Rendering
{
    public class Camera : IComponent
    {
        public EcsComponentType type => EcsComponentType.Camera;
        public int id { get; set; } = -1;

        public EcsComponentRef transform;
        
        public Matrix4x4 view;
        public Matrix4x4 proj;

        public float fov;

        public Camera(EcsComponentRef transform, float fov)
        {
            this.transform = transform;
            this.fov = fov;
        }
        
        public void GenerateTransforms()
        {
            view = Matrix4x4.CreateLookAt(
                new Vector3(MathF.Sin((float)AppState.GetSeconds()) * 13, 3,
                    MathF.Cos((float)AppState.GetSeconds()) * 13), new Vector3(0, 0, 0), new Vector3(0, -1, 0));
            proj = Matrix4x4.CreatePerspectiveFieldOfView(fov * MathF.PI / 180.0f, Window.size.X / Window.size.Y, 0.01f,
                40.0f);
        }

        public void Dispose()
        {
            
        }
    }
}