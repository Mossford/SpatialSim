namespace SpatialSim.Engine.Audio
{
    public static class WaveGenerator
    {
        public static float TriangleWave(float x)
        {
            return MathF.Acos(MathF.Sin(x)) / 1.5708f - 1f;
        }
    }
}