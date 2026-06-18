using System.Numerics;
using SpatialSim.Engine.Core;

namespace SpatialSim.Engine.Rendering
{
    public class Camera : IComponent
    {
        public int type => EcsComponentType.Camera.GetId();
        public int id { get; set; } = -1;

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
        
        public Matrix4x4 view;
        public Matrix4x4 proj;
        public float fov;
        

        public Camera(EcsComponentRef transform, float fov)
        {
            transform.CheckComponent(EcsComponentType.Transform.GetId());
            this.transform = transform;
            this.fov = fov;
        }
        
        public void GenerateTransforms()
        {
            view = Matrix4x4.CreateLookAt(
                transformRef.position, transformRef.position + transformRef.forward, transformRef.up);
            if (fov < 0.001f)
                fov = 0.001f;
            proj = Matrix4x4.CreatePerspectiveFieldOfView(
                fov * MathF.PI / 180.0f,
                Window.size.X / Window.size.Y,
                0.1f,
                10f);
            
            //apply reverse depth buffer
            Matrix4x4 transform = new Matrix4x4(
                1.0f, 0.0f,  0.0f, 0.0f,
                0.0f, 1.0f,  0.0f, 0.0f,
                0.0f, 0.0f, -1.0f, 0.0f,
                0.0f, 0.0f,  1.0f, 1.0f);
            
            proj *= transform;
            
            transform = new Matrix4x4(
                1.0f, 0.0f, 0.0f, 0.0f,
                0.0f, 1.0f, 0.0f, 0.0f,
                0.0f, 0.0f, 0.5f, 0.0f,
                0.0f, 0.0f, 0.5f, 1.0f);
            
            proj *= transform;
            
        }

        public void Dispose()
        {
            
        }
    }
}