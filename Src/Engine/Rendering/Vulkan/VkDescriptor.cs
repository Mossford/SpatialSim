using Silk.NET.Vulkan;
using SpatialSim.Engine.Core;
using SpatialSim.Engine.Core.Vulkan;
using AppContext = SpatialSim.Engine.Core.AppContext;

namespace SpatialSim.Engine.Rendering.Vulkan
{
    public class VkDescriptor : IDisposable
    {
        public static DescriptorPool descriptorPool;
        public static bool createdPool;
        public DescriptorSet descriptorSet;
        
        public ShaderDescriptorDef descriptorDef;

        //allow for multiple usages
        public unsafe void Create(VkPipeline pipeline, ShaderDescriptorDef descriptorDef)
        {
            this.descriptorDef = descriptorDef;
            
            if (!createdPool)
            {
                //The pool should support the types that may be used
                DescriptorPoolSize[] poolSizes =
                {
                    new DescriptorPoolSize
                    {
                        Type = DescriptorType.UniformBufferDynamic,
                        DescriptorCount = VkSettings.MaxDescriptorsInPool
                    },
                    new DescriptorPoolSize
                    {
                        Type = DescriptorType.CombinedImageSampler,
                        DescriptorCount = VkSettings.MaxDescriptorsInPool
                    }
                };

                fixed (DescriptorPoolSize* poolSizesPtr = poolSizes)
                {
                    DescriptorPoolCreateInfo poolInfo = new()
                    {
                        SType = StructureType.DescriptorPoolCreateInfo,
                        PoolSizeCount = (uint)poolSizes.Length,
                        PPoolSizes = poolSizesPtr,
                        MaxSets = (uint)(VkSettings.MaxDescriptorsInPool * poolSizes.Length)
                    };

                    Result resultPool = AppState.appContext.GetContext<VkContext>().vk.CreateDescriptorPool(
                        VkDevices.device, in poolInfo, null, out descriptorPool);
                    if (resultPool != Result.Success)
                    {
                        Debug.Error($"Failed to create descriptor pool {resultPool}");
                        throw new Exception($"Failed to create descriptor pool {resultPool}");
                    }
                }
                
                createdPool = true;
            }
            
            DescriptorSetLayout layout = pipeline.descriptorSetLayouts[descriptorDef];
            DescriptorSetAllocateInfo allocateInfo = new()
            {
                SType = StructureType.DescriptorSetAllocateInfo,
                DescriptorPool = descriptorPool,
                DescriptorSetCount = 1,
                PSetLayouts = &layout,
            };
                
            fixed (DescriptorSet* descriptorSetsPtr = &descriptorSet)
            {
                Result result = AppState.appContext.GetContext<VkContext>().vk.AllocateDescriptorSets(
                    VkDevices.device, in allocateInfo, descriptorSetsPtr);
                
                if (result != Result.Success)
                {
                    Debug.Error($"Failed to allocate descriptor sets {result}");
                    throw new Exception($"Failed to allocate descriptor sets {result}");
                }
            }
            
            Debug.LogDebug($"Created descriptor set {descriptorDef.usage} with layout at set: {descriptorDef.set} and binding: {descriptorDef.binding}");
        }

        public unsafe void Clean()
        {
            
        }

        public static unsafe void CleanPool()
        {
            if(!createdPool)
                return;
            
            createdPool = false;
            AppState.appContext.GetContext<VkContext>().vk.DestroyDescriptorPool(VkDevices.device, descriptorPool, null);
        }

        public void Dispose()
        {
            Clean();
        }
    }
}