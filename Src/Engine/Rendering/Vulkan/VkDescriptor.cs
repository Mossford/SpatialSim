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
                        MaxSets = (uint)(VkSettings.MaxDescriptorsInPool * poolSizes.Length),
                        //TODO decide to restrict only to the samplers
                        Flags = DescriptorPoolCreateFlags.UpdateAfterBindBit
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
            
            Debug.LogDebug($"Created descriptor set with def {descriptorDef}");
        }
        
        public unsafe void SetTexturesToDescriptorSet(Texture[] textures, int[] bindings)
        {
            if (textures.Length != bindings.Length)
            {
                Debug.Error("Passed in textures to set descriptor set does not match count of bindings, skipping");
                return;
            }
            
            WriteDescriptorSet[] descriptorWrites = new WriteDescriptorSet[textures.Length];

            for (int i = 0; i < descriptorWrites.Length; i++)
            {
                VkTexture texture = ((VkTexture)textures[i].texture);
                
                DescriptorImageInfo imageInfo = new()
                {
                    ImageLayout = ImageLayout.ShaderReadOnlyOptimal,
                    ImageView = texture.imageView,
                    Sampler = texture.sampler
                };
            
                descriptorWrites[i] = new()
                {
                    SType = StructureType.WriteDescriptorSet,
                    DstSet = descriptorSet,
                    //SdlGpu has the binding as i and the array element as 0 look into why
                    DstBinding = (uint)bindings[i],
                    DstArrayElement = 0,
                    DescriptorType = DescriptorType.CombinedImageSampler,
                    DescriptorCount = 1,
                    PImageInfo = &imageInfo
                };
            }

            fixed (WriteDescriptorSet* writes = descriptorWrites)
            {
                AppState.appContext.GetContext<VkContext>().vk.UpdateDescriptorSets(
                    VkDevices.device,
                    (uint)descriptorWrites.Length,
                    writes,
                    0,
                    null);
            }
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