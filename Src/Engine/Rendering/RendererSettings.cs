using System.Drawing;

namespace SpatialSim.Engine.Rendering
{
    public static class RendererSettings
    {
        public const int VertexUniformSet = 0;
        public const int TotalVertexSetCount = 1;
        
        public const int FragmentStorageSet = 3;
        public const int FragmentUniformSet = 2;
        public const int FragmentSamplerSet = 1;
        public const int TotalFragmentSetCount = 3;

        public static Color clearColor = Color.CadetBlue;
    }
}