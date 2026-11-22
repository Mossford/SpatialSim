using SpatialSim.Engine.Rendering;

namespace SpatialSim.Engine.Core
{
    public static class EcsRendererManager
    {
        /// <summary>
        /// Probably a better way to do this
        /// sorts by layer of pipeline, and then index for the pipeline so groups of renderers of the same pipeline can just get one api call for the pipeline bind
        /// </summary>
        public static SortedList<int, SortedList<int, List<int>>> renderOrder;

        public static void Init()
        {
            renderOrder = new SortedList<int, SortedList<int, List<int>>>();
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
                Pipeline pipeline = PipelineManager.RetrievePipeline(renderer.pipelineRef);
                //This should not happen but it could, if a mesh renderer is assigned a pipeline that is not a raster
                if (pipeline.settings.type != PipelineType.Raster)
                {
                    Debug.Error($"A meshRenderer was assigned a {pipeline.settings.type} pipeline, should be a raster pipeline to work");
                    continue;
                }
                int pipelineLayer = pipeline.layer;
                if (renderOrder.TryAdd(pipelineLayer, new SortedList<int, List<int>>()))
                {
                    if (renderOrder[pipelineLayer].TryAdd(pipeline.id, new List<int>()))
                    {
                        renderOrder[pipelineLayer][pipeline.id].Add(i);
                    }
                    else
                    {
                        renderOrder[pipelineLayer][pipeline.id].Add(i);
                    }
                }
                else
                {
                    if (renderOrder[pipelineLayer].TryAdd(pipeline.id, new List<int>()))
                    {
                        renderOrder[pipelineLayer][pipeline.id].Add(i);
                    }
                    else
                    {
                        renderOrder[pipelineLayer][pipeline.id].Add(i);
                    }
                }
            }
        }

        /// <summary>
        /// Render to swapchain
        /// TODO Remove once swapchain gets texture
        /// </summary>
        public static void Render(CommandBuffer commandBuffer, int frame)
        {
            commandBuffer.BeginRendering(frame);
            commandBuffer.SetViewport(Window.size);
            commandBuffer.SetScissor(Window.size);
            for (int i = 0; i < renderOrder.Count; i++)
            {
                SortedList<int, List<int>> layers = renderOrder.GetValueAtIndex(i);
                for (int j = 0; j < layers.Count; j++)
                {
                    int pipelineId = layers.GetKeyAtIndex(j);
                    commandBuffer.BindPipeLine(PipelineManager.RetrievePipeline(pipelineId));

                    List<int> pipelines = layers.GetValueAtIndex(j);
                    for (int k = 0; k < pipelines.Count; k++)
                    {
                        int index = pipelines[k];
                        MeshRenderer renderer = (MeshRenderer)EcsManager.componentPools[EcsComponentType.MeshRenderer.GetId()].components.Get(index);
                        renderer.Draw(commandBuffer);
                    }
                }
            }
            commandBuffer.EndRendering();
        }
        
        /// <summary>
        /// Render to a texture
        /// </summary>
        public static void Render(CommandBuffer commandBuffer, Texture texture)
        {
            commandBuffer.BeginRendering(texture);
            commandBuffer.SetViewport(Window.size);
            commandBuffer.SetScissor(Window.size);
            for (int i = 0; i < renderOrder.Count; i++)
            {
                SortedList<int, List<int>> layers = renderOrder.GetValueAtIndex(i);
                for (int j = 0; j < layers.Count; j++)
                {
                    int pipelineId = layers.GetKeyAtIndex(j);
                    commandBuffer.BindPipeLine(PipelineManager.RetrievePipeline(pipelineId));

                    List<int> pipelines = layers.GetValueAtIndex(j);
                    for (int k = 0; k < pipelines.Count; k++)
                    {
                        int index = pipelines[k];
                        MeshRenderer renderer = (MeshRenderer)EcsManager.componentPools[EcsComponentType.MeshRenderer.GetId()].components.Get(index);
                        renderer.Draw(commandBuffer);
                    }
                }
            }
            commandBuffer.EndRendering();
        }
    }
}