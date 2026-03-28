using System.Numerics;

namespace SpatialSim.Engine.Core
{
    public class Transform : IComponent
    {
        public EcsComponentType type => EcsComponentType.Transform;
        public int id { get; set; } = -1;

        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        public Matrix4x4 modelMat;

        public Vector3 forward => GetForward();
        public Vector3 up => GetUp();
        public Vector3 right => GetRight();

        public Transform()
        {
            position = new Vector3();
            rotation = new Vector3();
            scale = new Vector3();
        }

        public Transform(Vector3 position, Vector3 rotation, Vector3 scale)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }

        public Matrix4x4 GetModelMat()
        {
            modelMat = Matrix4x4.Identity *
                       Matrix4x4.CreateScale(scale) *
                       Matrix4x4.CreateFromYawPitchRoll(rotation.X, rotation.Y, rotation.Z) *
                       Matrix4x4.CreateTranslation(position);
            return modelMat;
        }
        
        public Vector3 GetForward()
        {
            Vector3 target;
            target.X = -MathF.Sin(rotation.X*(MathF.PI/180.0f)) * MathF.Cos(rotation.Y*(MathF.PI/180.0f));
            target.Y = -MathF.Sin(rotation.Y*(MathF.PI/180.0f));
            target.Z = MathF.Cos(rotation.X*(MathF.PI/180.0f)) * MathF.Cos(rotation.Y*(MathF.PI/180.0f));
            return Vector3.Normalize(target);
        }

        public Vector3 GetRight()
        {
            return Vector3.Normalize(Vector3.Cross(-Vector3.UnitY, forward));
        }
        
        public Vector3 GetUp()
        {
            return Vector3.Cross(forward, right);
        }
        
        public void Dispose()
        {
            
        }
    }
}