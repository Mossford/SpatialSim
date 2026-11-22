using SpatialSim.Engine.Rendering;

namespace SpatialSim.Game.Rendering.StarRendering
{
    public class StarPipeline : Pipeline
    {
        public StarPipeline(string name) : base(name)
        {
            this.name = name;
        }

        public override void SetData(in CommandBuffer commandBuffer, int uniformBinding)
        {
            base.SetData(in commandBuffer, uniformBinding);
        }
    }
}