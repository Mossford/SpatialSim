using SpatialSim.Engine.Rendering;

namespace SpatialSim.Engine.Core
{
    public static class EcsRendererManager
    {
        /// <summary>
        /// Probably a better way to do this
        /// </summary>
        public static SortedList<int, List<int>> renderOrder;

        public static void Init()
        {
            renderOrder = new SortedList<int, List<int>>();
        }
        
        public static void UpdateOrder()
        {
            int meshRendererCount = EcsManager.componentPools[EcsComponentType.MeshRenderer.GetId()].components.ValueCount;
            
            if (renderOrder.Count != meshRendererCount)
            {
                SortOrder();
            }
        }

        public static void SortOrder()
        {
            int meshRendererCount = EcsManager.componentPools[EcsComponentType.MeshRenderer.GetId()].components.ValueCount;
            renderOrder.Clear();
            for (int i = 0; i < meshRendererCount; i++)
            {
                MeshRenderer renderer = (MeshRenderer)EcsManager.componentPools[EcsComponentType.MeshRenderer.GetId()].components.Get(i);
                int pipelineLayer = PipelineManager.RetrievePipeline(renderer.pipelineRef).layer;
                if (renderOrder.TryAdd(pipelineLayer, new List<int>()))
                {
                    renderOrder[pipelineLayer].Add(i);
                }
                else
                {
                    renderOrder[pipelineLayer].Add(i);
                }
            }
        }

        public static void Render(CommandBuffer commandBuffer, int frame)
        {
            commandBuffer.BeginRendering(frame);
            for (int i = 0; i < renderOrder.Count; i++)
            {
                List<int> indexes = renderOrder.GetValueAtIndex(i);
                for (int j = 0; j < indexes.Count; j++)
                {
                    int index = indexes[j];
                    MeshRenderer renderer = (MeshRenderer)EcsManager.componentPools[EcsComponentType.MeshRenderer.GetId()].components.Get(index);
                    renderer.Draw(commandBuffer, frame);
                }
            }
            commandBuffer.EndRendering();
        }
    }
}