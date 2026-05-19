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

    public struct Timer()
    {
        public ulong startTime;
        public ulong endTime;
        public ulong elapsed;
        public TimerMode mode;

        public enum TimerMode
        {
            Seconds,
            Milliseconds,
            Microseconds
        }

        public void SignalStart(TimerMode mode = TimerMode.Seconds)
        {
            this.mode = mode;

            switch(mode)
            {
                case TimerMode.Seconds:
                    {
                        startTime = (ulong)DateTime.Now.Second;
                        break;
                    }
                case TimerMode.Milliseconds:
                    {
                        startTime = 1000 * (ulong)DateTime.Now.Second + (ulong)DateTime.Now.Millisecond;
                        break;
                    }
                case TimerMode.Microseconds:
                    {
                        startTime = 1000000 * (ulong)DateTime.Now.Second + 1000 * (ulong)DateTime.Now.Millisecond + (ulong)DateTime.Now.Microsecond;
                        break;
                    }
            }
        }

        public void SignalEnd()
        {
            switch (mode)
            {
                case TimerMode.Seconds:
                    {
                        endTime = (ulong)DateTime.Now.Second;
                        break;
                    }
                case TimerMode.Milliseconds:
                    {
                        endTime = 1000 * (ulong)DateTime.Now.Second + (ulong)DateTime.Now.Millisecond;
                        break;
                    }
                case TimerMode.Microseconds:
                    {
                        endTime = 1000000 * (ulong)DateTime.Now.Second + 1000 * (ulong)DateTime.Now.Millisecond + (ulong)DateTime.Now.Microsecond;
                        break;
                    }
            }

            elapsed = endTime - startTime;
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

        #region Timing

        public static Timer startUpTimer;

        #endregion
    }
}