using System.Numerics;
using SpatialGame;

namespace SpatialSim.Game.Rendering.StarRendering
{
    public struct Star
    {
        //Better to keep position in spherical coords?
        public Vector3 position;
        public Vector4Byte color;
        public float intensity;
    }
}