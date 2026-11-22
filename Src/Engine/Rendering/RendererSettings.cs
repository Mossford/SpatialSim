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
        
        public const int ComputeSamplerSet = 3;
        public const int ComputeStorageSet = 2;
        public const int ComputeUniformSet = 1;
        public const int ComputeImageStorageSet = 0;
        public const int TotalComputeSetCount = 4;
        
        public static Color clearColor = Color.FromArgb(255, 3, 3, 3);
    }
}