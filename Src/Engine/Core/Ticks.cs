namespace SpatialSim.Engine.Core
{
    public struct TickCounter()
    {
        public ulong created = 0;
        public ulong deleted = 0;
        public ulong total => created - deleted;

        public override string ToString()
        {
            return $"T: {total}, C: {created}, D: {deleted}";
        }
    }
    
    public static class Ticks
    {
        #region Rendering

        public static TickCounter bufferCount;
        public static TickCounter pipelineCount;
        public static TickCounter commandBufferCount;
        public static TickCounter swapchainRecreations;

        public static TickCounter gpuMemoryAllocation;

        #endregion
    }
}