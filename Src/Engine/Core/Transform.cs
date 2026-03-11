using System.Numerics;

namespace SpatialSim.Engine.Core
{
    public class Transform : IComponent
    {
        public EcsComponentType type => EcsComponentType.Transform;
        public int id { get; set; } = -1;

        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public Matrix4x4 modelMat;

        public Transform()
        {
            position = new Vector3();
            rotation = new Quaternion();
            scale = new Vector3();
        }

        public Transform(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }

        public Matrix4x4 GetModelMat()
        {
            modelMat = Matrix4x4.Identity * Matrix4x4.CreateScale(scale) *
                       Matrix4x4.CreateTranslation(position) *
                       Matrix4x4.CreateFromQuaternion(rotation);
            return modelMat;
        }
        
        public void Dispose()
        {
            
        }
    }
}