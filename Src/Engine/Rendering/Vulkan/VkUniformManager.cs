using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using SpatialSim.Engine.Core;
using SpatialSim.Engine.Core.Vulkan;

namespace SpatialSim.Engine.Rendering.Vulkan
{
    public class VkUniformStageData
    {
        public List<Buffer<byte>> uniformBuffers;
        public List<VkDescriptor> descriptors;
        public int currentBuffer;
    }
    
    /// <summary>
    /// One for each pipeline
    /// </summary>
    public class VkUniformManager
    {
        //store the blocks of each shaders sections
        public Dictionary<ShaderType, Dictionary<ShaderDescriptorDef, VkUniformStageData>> uniforms;
        public VkPipeline currentPipeline;
        
        //Take in array of descriptions of all the uniforms from a pipeline
        public unsafe void Init(VkPipeline pipeline, ShaderType[] shaderTypes, ShaderDescriptorDef[] uniformDescriptions)
        {
            this.currentPipeline = pipeline;
            
            uniforms = new Dictionary<ShaderType, Dictionary<ShaderDescriptorDef, VkUniformStageData>>();

            for (int i = 0; i < shaderTypes.Length; i++)
            {
                if (!uniforms.TryAdd(shaderTypes[i], new Dictionary<ShaderDescriptorDef, VkUniformStageData>()))
                {
                    Debug.Error($"Tried to add Uniform stage {shaderTypes[i]} for already existing stage, skipping");
                }
                else
                {
                    // TODO Make the shader provide this information
                    for (int j = 0; j < uniformDescriptions.Length; j++)
                    {
                        //skip the descriptions that dont match this stage
                        if (uniformDescriptions[j].type != shaderTypes[i])
                            continue;
                        
                        if (!uniforms[shaderTypes[i]].TryAdd(uniformDescriptions[j], new VkUniformStageData()))
                        {
                            Debug.Error($"Tried to add a descriptor set layout " +
                                          $"{uniformDescriptions[j].set} " +
                                          $"{uniformDescriptions[j].bindings} " +
                                          $"{uniformDescriptions[j].usage} that already exists, skipping");
                        }
                        else
                        {
                            uniforms[shaderTypes[i]][uniformDescriptions[j]].uniformBuffers = new List<Buffer<byte>>();
                            uniforms[shaderTypes[i]][uniformDescriptions[j]].descriptors = new List<VkDescriptor>();
                            for (int g = 0; g < VkSettings.MaxUniformsPerStage; g++)
                            {
                                AddUniformBuffer(shaderTypes[i], uniformDescriptions[j], pipeline);
                            }
                            uniforms[shaderTypes[i]][uniformDescriptions[j]].currentBuffer = 0;
                        }
                    }
                }
            }
        }

        unsafe void AddUniformBuffer(ShaderType type, ShaderDescriptorDef def, VkPipeline pipeline)
        {
            uniforms[type][def].uniformBuffers.Add(new Buffer<byte>());
            uniforms[type][def].uniformBuffers[^1].Create(VkSettings.MaxUniformSize, BufferUsage.Uniform, BufferMemoryUsage.Cpu);
            uniforms[type][def].descriptors.Add(new VkDescriptor());
            uniforms[type][def].descriptors[^1].Create(pipeline, def);
            
            DescriptorBufferInfo bufferInfo = new()
            {
                Buffer = ((VkBuffer<byte>)uniforms[type][def].uniformBuffers[^1].buffer!).buffer,
                Offset = 0,
                //Set the range to be the maximum block for a shader
                Range = VkSettings.MaxBlockUniformMemory,
            };
            
            WriteDescriptorSet descriptorWrite = new()
            {
                SType = StructureType.WriteDescriptorSet,
                DstSet = uniforms[type][def].descriptors[^1].descriptorSet,
                //This should always have a value?
                DstBinding = (uint)def.bindings[0],
                DstArrayElement = 0,
                DescriptorType = DescriptorType.UniformBufferDynamic,
                DescriptorCount = 1,
                PBufferInfo = &bufferInfo,
            };
            
            AppState.appContext.GetContext<VkContext>().vk.UpdateDescriptorSets(VkDevices.device, 1, in descriptorWrite, 0, null);
        }

        public unsafe void Clean()
        {
            Dictionary<ShaderDescriptorDef, VkUniformStageData>[] arr = uniforms.Values.ToArray();
            for (int i = 0; i < arr.Length; i++)
            {
                VkUniformStageData[] data = arr[i].Values.ToArray();
                for (int j = 0; j < data.Length; j++)
                {
                    for (int k = 0; k < data[j].uniformBuffers.Count; k++)
                    {
                        data[j].uniformBuffers[k].Clean();
                    }
                    
                    for (int k = 0; k < data[j].uniformBuffers.Count; k++)
                    {
                        data[j].descriptors[k].Clean();
                    }
                }
            }
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

        public Buffer<byte> GetBuffer(ShaderDescriptorDef def, int dataLength)
        {
            VkBuffer<byte> buff = (VkBuffer<byte>)uniforms[def.type][def].uniformBuffers[uniforms[def.type][def].currentBuffer].buffer!;
            uint blockSize = PadUniformSize((uint)dataLength);
            
            if (blockSize + buff.memoryOffset + VkSettings.MaxBlockUniformMemory > VkSettings.MaxUniformSize)
            {
                uniforms[def.type][def].currentBuffer++;

                if (uniforms[def.type][def].currentBuffer >= uniforms[def.type][def].uniformBuffers.Count)
                {
                    Debug.LogDebug("Run out of buffer space, creating new uniform buffer");
                    AddUniformBuffer(def.type, def, currentPipeline);
                    uniforms[def.type][def].currentBuffer = uniforms[def.type][def].uniformBuffers.Count - 1;
                }
                
                buff = (VkBuffer<byte>)uniforms[def.type][def].uniformBuffers[uniforms[def.type][def].currentBuffer].buffer!;
                buff.memoryOffset = 0;
                buff.drawOffset = 0;
            }
            
            return uniforms[def.type][def].uniformBuffers[uniforms[def.type][def].currentBuffer];
        }
        
        public Buffer<byte> GetBuffer(ShaderDescriptorDef def)
        {
            return uniforms[def.type][def].uniformBuffers[uniforms[def.type][def].currentBuffer];
        }

        public DescriptorSet GetDescriptorSet(ShaderDescriptorDef def)
        {
            return uniforms[def.type][def].descriptors[uniforms[def.type][def].currentBuffer].descriptorSet;
        }

        public void ResetUniforms()
        {
            Dictionary<ShaderDescriptorDef, VkUniformStageData>[] arr = uniforms.Values.ToArray();
            for (int i = 0; i < arr.Length; i++)
            {
                VkUniformStageData[] data = arr[i].Values.ToArray();
                for (int j = 0; j < data.Length; j++)
                {
                    data[j].currentBuffer = 0;
                    
                    for (int k = 0; k < data[j].uniformBuffers.Count; k++)
                    {
                        ((VkBuffer<byte>)data[j].uniformBuffers[k].buffer!).drawOffset = 0;
                        ((VkBuffer<byte>)data[j].uniformBuffers[k].buffer!).memoryOffset = 0;
                    }
                }
            }
        }
        
    }
}