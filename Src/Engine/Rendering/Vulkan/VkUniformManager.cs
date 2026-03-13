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

            currentBuffer = 0;
        }

        void AddUniformBuffer()
        {
            uniformBuffers.Add(new Buffer<byte>());
            uniformBuffers[^1].Create(VkSettings.MaxUniformSize, BufferUsage.Uniform, BufferMemoryUsage.Cpu);
            descriptors.Add(new VkDescriptor());
            descriptors[^1].Create(0, new [] { VkDescriptorUsage.Vertex });
            
            SetBufferToDescriptorSet();
        }
        
        void CreateUniformBuffers()
        {
            //create a copy for each swapchain we have
            uniformBuffers = new List<Buffer<byte>>();
            descriptors = new List<VkDescriptor>();
            for (int i = 0; i < VkSettings.MaxUniformsPerStage; i++)
            {
                AddUniformBuffer();
            }
        }

        public unsafe void SetBufferToDescriptorSet()
        {
            DescriptorBufferInfo bufferInfo = new()
            {
                Buffer = ((VkBuffer<byte>)uniformBuffers[^1].buffer!).buffer,
                Offset = 0,
                //Set the range to be the maximum block for a shader
                Range = VkSettings.MaxBlockUniformMemory,
            };
            
            WriteDescriptorSet descriptorWrite = new()
            {
                SType = StructureType.WriteDescriptorSet,
                DstSet = descriptors[^1].descriptorSet,
                //SdlGpu has the binding as i and the array element as 0 look into why
                DstBinding = 0,
                DstArrayElement = 0,
                DescriptorType = DescriptorType.UniformBufferDynamic,
                DescriptorCount = 1,
                PBufferInfo = &bufferInfo,
            };
            
            AppState.appContext.GetContext<VkContext>().vk.UpdateDescriptorSets(VkDevices.device, 1, in descriptorWrite, 0, null);
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

                if (currentBuffer >= uniformBuffers.Count)
                {
                    Debug.LogDebug("Run out of buffer space, creating new uniform buffer");
                    AddUniformBuffer();
                    currentBuffer = uniformBuffers.Count - 1;
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