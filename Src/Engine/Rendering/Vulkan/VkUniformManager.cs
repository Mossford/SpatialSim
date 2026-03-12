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
        public List<VkDescriptor> descriptors;
        public int currentBuffer;
        
        public unsafe void Init()
        {
            // TODO Make the shader provide this information
            CreateUniformBuffers();
            CreateDescriptorSets();

            currentBuffer = 0;
        }
        
        void CreateUniformBuffers()
        {
            //create a copy for each swapchain we have
            uniformBuffers = new List<Buffer<byte>>();
            for (int i = 0; i < VkSettings.MaxUniformsPerStage; i++)
            {
                uniformBuffers.Add(new Buffer<byte>());
                uniformBuffers[i].Create(VkSettings.MaxUniformSize, BufferUsage.Uniform, BufferMemoryUsage.Cpu);
            }
        }
        
        void CreateDescriptorSets()
        {
            descriptors = new List<VkDescriptor>();
            for (int i = 0; i < VkSettings.MaxUniformsPerStage; i++)
            {
                descriptors.Add(new VkDescriptor());
                descriptors[i].Create(0);
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
                        DstSet = descriptors[i].descriptorSet,
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

            for (int i = 0; i < descriptors.Count; i++)
            {
                descriptors[i].Clean();
            }

            VkDescriptor.CleanPool();
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
            return descriptors[currentBuffer].descriptorSet;
        }

    }
}