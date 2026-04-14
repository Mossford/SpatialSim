using System.Numerics;

namespace SpatialSim.Game.Math
{
    public static class MathUtil
    {
        /// <summary>
        /// sizes in mm
        /// </summary>
        public static float GetFovFromFocalLength(float sensorHeight, float focalLength)
        {
            return 360f * MathF.Atan2(sensorHeight, 2f * focalLength) / MathF.PI;
        }
        
        public static float GetFocalLengthFromFov(float sensorHeight, float fov)
        {
            return sensorHeight / (2f * MathF.Tan(fov * MathF.PI / 360f));
        }
        
        public static float GetScaleFromAngularSize(float angularSize, float distance = 1.0f)
        {
            return distance * MathF.Tan(angularSize * MathF.PI / 180f / 2f);
        }
    }
}