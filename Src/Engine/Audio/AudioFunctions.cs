using System.Runtime.CompilerServices;

namespace SpatialSim.Engine.Audio
{
    public static class AudioFunctions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float TriangleWave(float t)
        {
            return MathF.Acos(MathF.Sin(t)) / 1.5708f - 1f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SinWave(float t)
        {
            return MathF.Sin(t);
        }
    }
}