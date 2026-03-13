using Silk.NET.Vulkan;
using SpatialSim.Engine.Core;
using SpatialSim.Engine.Core.Vulkan;

namespace SpatialSim.Engine.Rendering.Vulkan
{
    public class VkDescriptor : IDisposable
    {
        public static DescriptorPool descriptorPool;
        public static bool createdPool;
        public static StableList<DescriptorSetLayout> setLayouts;

        public DescriptorSet descriptorSet;
        public int layoutIndex;
        public int binding;
        public VkDescriptorUsage[] usage;

        //allow for multiple usages
        public unsafe void Create(int binding, in VkDescriptorUsage[] usage)
        {
            this.binding = binding;
            this.usage = usage;
            
            if (!createdPool)
            {
                DescriptorPoolSize poolSize = new()
                {
                    Type = DescriptorType.UniformBufferDynamic,
                    DescriptorCount = VkSettings.MaxDescriptorsInPool
                };

                DescriptorPoolCreateInfo poolInfo = new()
                {
                    SType = StructureType.DescriptorPoolCreateInfo,
                    PoolSizeCount = 1,
                    PPoolSizes = &poolSize,
                    MaxSets = VkSettings.MaxDescriptorsInPool,
                };

                fixed (DescriptorPool* descriptorPoolPtr = &descriptorPool)
                {
                    Result resultPool = AppState.appContext.GetContext<VkContext>().vk.CreateDescriptorPool(
                        VkDevices.device, in poolInfo, null, descriptorPoolPtr);
                    if (resultPool != Result.Success)
                    {
                        Debug.Error($"Failed to create descriptor pool {resultPool}");
                        throw new Exception($"Failed to create descriptor pool {resultPool}");
                    }

                }
                
                createdPool = true;
                setLayouts = new StableList<DescriptorSetLayout>();
            }
            
            ShaderStageFlags stageFlags = ShaderStageFlags.None;
            switch (usage[0])
            {
                case VkDescriptorUsage.Vertex:
                {
                    stageFlags = ShaderStageFlags.VertexBit;
                    break;
                }
                case VkDescriptorUsage.Fragment:
                {
                    stageFlags = ShaderStageFlags.FragmentBit;
                    break;
                }
            }
            for (int i = 1; i < usage.Length; i++)
            {
                switch (usage[i])
                {
                    case VkDescriptorUsage.Vertex:
                    {
                        stageFlags |= ShaderStageFlags.VertexBit;
                        break;
                    }
                    case VkDescriptorUsage.Fragment:
                    {
                        stageFlags |= ShaderStageFlags.FragmentBit;
                        break;
                    }
                }
            }
            
            DescriptorSetLayoutBinding uboLayoutBindings = new()
            {
                Binding = (uint)binding,
                DescriptorCount = 1,
                DescriptorType = DescriptorType.UniformBufferDynamic,
                PImmutableSamplers = null,
                StageFlags = ShaderStageFlags.VertexBit,
            };

            DescriptorSetLayoutCreateInfo layoutInfo = new()
            {
                SType = StructureType.DescriptorSetLayoutCreateInfo,
                BindingCount = 1,
                PBindings = &uboLayoutBindings,
            };

            Result result = AppState.appContext.GetContext<VkContext>().vk
                .CreateDescriptorSetLayout(VkDevices.device, in layoutInfo, null, out DescriptorSetLayout layout);
            if (result != Result.Success)
            {
                Debug.Error($"Failed to create descriptor set layout {result}");
                throw new Exception($"Failed to create descriptor set layout {result}");
            }
                
            DescriptorSetAllocateInfo allocateInfo = new()
            {
                SType = StructureType.DescriptorSetAllocateInfo,
                DescriptorPool = descriptorPool,
                DescriptorSetCount = 1,
                PSetLayouts = &layout,
            };
                
            fixed (DescriptorSet* descriptorSetsPtr = &descriptorSet)
            {
                result = AppState.appContext.GetContext<VkContext>().vk.AllocateDescriptorSets(
                    VkDevices.device, in allocateInfo, descriptorSetsPtr);
                
                if (result != Result.Success)
                {
                    Debug.Error($"Failed to allocate descriptor sets {result}");
                    throw new Exception($"Failed to allocate descriptor sets {result}");
                }
            }

            layoutIndex = setLayouts.Add(layout);
            
            Debug.LogDebug($"Created descriptor set with layout at {layoutIndex}");
        }

        public unsafe void Clean()
        {
            AppState.appContext.GetContext<VkContext>().vk.DestroyDescriptorSetLayout(VkDevices.device, setLayouts[layoutIndex], null);
            setLayouts.RemoveAt(layoutIndex);
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
            Console.WriteLine("test");
            Clean();
        }
    }
}