using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using SpatialSim.Engine.Core;
using SpatialSim.Engine.Core.Vulkan;

namespace SpatialSim.Engine.Rendering.Vulkan
{
    // TODO Needs uniform wrapping if go over memory limit of one buffer

    /// <summary>
    /// One for each pipeline
    /// </summary>
    public class VkUniformManager
    {
        public DescriptorPool descriptorPool;
        //store the blocks of each shaders sections
        public List<Buffer<byte>> uniformBuffers;
        //this matches with the buffer count
        public DescriptorSet[] descriptorSets;
        public DescriptorSetLayout[] descriptorSetLayouts;
        public int currentBuffer;
        
        public unsafe void Init()
        {
            // TODO Make the shader provide this information
            descriptorSetLayouts = new DescriptorSetLayout[VkSettings.MaxUniformsPerStage];
            
            for (int i = 0; i < descriptorSetLayouts.Length; i++)
            {
                DescriptorSetLayoutBinding uboLayoutBindings = new()
                {
                    Binding = 0,
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

                fixed (DescriptorSetLayout* descriptorSetLayoutPtr = &descriptorSetLayouts[i])
                {
                    if (AppState.appContext.GetContext<VkContext>().vk.CreateDescriptorSetLayout(VkDevices.device, in layoutInfo, null, descriptorSetLayoutPtr) != Result.Success)
                    {
                        Debug.Error("Failed to create descriptor set layout");
                        throw new Exception("Failed to create descriptor set layout");
                    }
                }
            }

            currentBuffer = 0;
        }
        
        public void CreateUniformBuffers()
        {
            //create a copy for each swapchain we have
            uniformBuffers = new List<Buffer<byte>>();
            for (int i = 0; i < VkSettings.MaxUniformsPerStage; i++)
            {
                uniformBuffers.Add(new Buffer<byte>());
                uniformBuffers[i].Create(VkSettings.MaxUniformSize, BufferUsage.Uniform, BufferMemoryUsage.Cpu);
            }
            
        }
        
        public unsafe void CreateDescriptorSets()
        {
            DescriptorPoolSize poolSize = new()
            {
                Type = DescriptorType.UniformBufferDynamic,
                DescriptorCount = VkSettings.MaxUniformsPerStage
            };

            DescriptorPoolCreateInfo poolInfo = new()
            {
                SType = StructureType.DescriptorPoolCreateInfo,
                PoolSizeCount = 1,
                PPoolSizes = &poolSize,
                MaxSets = VkSettings.MaxUniformsPerStage,
            };

            fixed (DescriptorPool* descriptorPoolPtr = &descriptorPool)
            {
                if (AppState.appContext.GetContext<VkContext>().vk.CreateDescriptorPool(
                        VkDevices.device, in poolInfo, null, descriptorPoolPtr) != Result.Success)
                {
                    Debug.Error("Failed to create descriptor pool");
                    throw new Exception("Failed to create descriptor pool");
                }

            }
            
            descriptorSets = new DescriptorSet[VkSettings.MaxUniformsPerStage];

            fixed (DescriptorSetLayout* layoutsPtr = descriptorSetLayouts)
            {
                DescriptorSetAllocateInfo allocateInfo = new()
                {
                    SType = StructureType.DescriptorSetAllocateInfo,
                    DescriptorPool = descriptorPool,
                    DescriptorSetCount = (uint)descriptorSets.Length,
                    PSetLayouts = layoutsPtr,
                };
                
                fixed (DescriptorSet* descriptorSetsPtr = descriptorSets)
                {
                    if (AppState.appContext.GetContext<VkContext>().vk.AllocateDescriptorSets(
                            VkDevices.device, in allocateInfo, descriptorSetsPtr) != Result.Success)
                    {
                        Debug.Error("Failed to allocate descriptor sets");
                        throw new Exception("Failed to allocate descriptor sets");
                    }
                }
            }

            SetBuffersToDescriptorSet();
        }

        public unsafe void SetBuffersToDescriptorSet()
        {
            WriteDescriptorSet[] descriptorWrites = new WriteDescriptorSet[uniformBuffers.Count];
            DescriptorBufferInfo[] bufferInfos = new DescriptorBufferInfo[uniformBuffers.Count];
            for (int i = 0; i < uniformBuffers.Count; i++)
            {
                bufferInfos[i] = new()
                {
                    Buffer = ((VkBuffer<byte>)uniformBuffers[i].buffer!).buffer,
                    Offset = 0,
                    //Set the range to be the maximum block for a shader
                    Range = VkSettings.MaxBlockUniformMemory,
                };

                fixed (DescriptorBufferInfo* bufPtr = &bufferInfos[i])
                {
                    descriptorWrites[i] = new()
                    {
                        SType = StructureType.WriteDescriptorSet,
                        DstSet = descriptorSets[i],
                        //SdlGpu has the binding as i and the array element as 0 look into why
                        DstBinding = 0,
                        DstArrayElement = 0,
                        DescriptorType = DescriptorType.UniformBufferDynamic,
                        DescriptorCount = 1,
                        PBufferInfo = bufPtr,
                    };
                }
            }
            
            fixed(WriteDescriptorSet* ptr = descriptorWrites)
                AppState.appContext.GetContext<VkContext>().vk.UpdateDescriptorSets(VkDevices.device, (uint)descriptorWrites.Length, ptr, 0, null);
        }

        public unsafe void Clean()
        {
            for (int i = 0; i < uniformBuffers.Count; i++)
            {
                uniformBuffers[i].Clean();
            }

            for (int i = 0; i < descriptorSetLayouts.Length; i++)
            {
                AppState.appContext.GetContext<VkContext>().vk.DestroyDescriptorSetLayout(VkDevices.device, descriptorSetLayouts[i], null);
            }
            
            AppState.appContext.GetContext<VkContext>().vk.DestroyDescriptorPool(VkDevices.device, descriptorPool, null);
        }
        
        public static uint PadUniformSize(uint originalSize)
        {
            uint minUboAlignment = (uint)VkDevices.properties.Limits.MinUniformBufferOffsetAlignment;
            uint alignedSize = originalSize;
            if (minUboAlignment > 0)
            {
                alignedSize = (alignedSize + minUboAlignment - 1) & ~(minUboAlignment - 1);
            }
            return alignedSize;
        }

        public Buffer<byte> GetBuffer(int dataLength)
        {
            VkBuffer<byte> buff = (VkBuffer<byte>)uniformBuffers[currentBuffer].buffer!;
            uint blockSize = PadUniformSize((uint)dataLength);
            
            if (blockSize + buff.memoryOffset + VkSettings.MaxBlockUniformMemory > VkSettings.MaxUniformSize)
            {
                currentBuffer++;

                if (currentBuffer >= VkSettings.MaxUniformsPerStage)
                {
                    Debug.Warning("Run out of buffer space, wrapping around");
                    currentBuffer = 0;
                }
                
                buff = (VkBuffer<byte>)uniformBuffers[currentBuffer].buffer!;
                buff.memoryOffset = 0;
                buff.drawOffset = 0;
            }
            
            return uniformBuffers[currentBuffer];
        }

        public DescriptorSet GetDescriptorSet()
        {
            return descriptorSets[currentBuffer];
        }

    }
}