using System.Numerics;
using Glslang.NET;
using SpatialSim.Engine.Core;
using SpatialSim.Engine.Rendering.Vulkan;

namespace SpatialSim.Engine.Rendering
{
    public sealed class Shader : IDisposable
    {
        public ShaderSettings settings;

        /// <summary>
        /// Stores the api shader information
        /// </summary>
        public IShaderDevice? shader;
        public Dictionary<ShaderDescriptorDef, List<byte>> uniformData;
        
        ShaderDescriptorDef uniformDef;

        public void Create(ShaderSettings settings)
        {
            this.settings = settings;
            uniformData = new Dictionary<ShaderDescriptorDef, List<byte>>();

            for (int i = 0; i < settings.descriptorDef.Length; i++)
            {
                if (settings.descriptorDef[i].usage == ShaderDescriptorUsage.Uniform)
                {
                    if(!uniformData.TryAdd(settings.descriptorDef[i], new List<byte>()))
                    {
                        Debug.Warning("Tried to add uniform data buffer to shader, but already exists, skipping");
                    }
                }
            }

            uniformDef = new ShaderDescriptorDef();
            uniformDef.type = settings.type;
            uniformDef.usage = ShaderDescriptorUsage.Uniform;
            switch (uniformDef.type)
            {
                case ShaderType.Vertex:
                {
                    uniformDef.set = RendererSettings.VertexUniformSet;
                    break;
                }
                case ShaderType.Fragment:
                {
                    uniformDef.set = RendererSettings.FragmentUniformSet;
                    break;
                }
            }
            
            if (!File.Exists(Resources.ShaderPath + settings.file))
            {
                Debug.Error("Could not find shader " + settings.file);
                return;
            }
            
            shader = AppState.appContext.DeviceFactory.CreateShaderDevice(settings, GLSLToSpirv(settings.type));
            
            Debug.LogDebug($"Created shader from {settings.file}");
        }

        public void Clean()
        {
            shader?.Clean();
            Debug.LogDebug($"Cleaned up shader from {settings.file}");
        }
        
        static IncludeResult IncludeFunction(string headerName, string includerName, uint depth, bool isSystemFile)
        {
            IncludeResult result;

            result.headerData = "// Nothing to see here";
            result.headerName = headerName;

            return result;
        }

        public byte[] GLSLToSpirv(ShaderType type)
        {
            byte[] shaderProgram = new byte[0];
            string data;
            try
            {
                data = File.ReadAllText(Resources.ShaderPath + settings.file);
            }
            catch (Exception e)
            {
                Debug.Error(e.Message);
                return shaderProgram;
            }
            
            ShaderStage stage = ShaderStage.Vertex;
            switch (type)
            {
                case ShaderType.Vertex:
                {
                    stage = ShaderStage.Vertex;
                    break;
                }
                case ShaderType.Fragment:
                {
                    stage = ShaderStage.Fragment;
                    break;
                }
                case ShaderType.Compute:
                {
                    stage = ShaderStage.Compute;
                    break;
                }
            }
            
            CompilationInput input = new CompilationInput()
            {
                language = SourceType.GLSL,
                stage = stage,
                client = ClientType.Vulkan,
                clientVersion = TargetClientVersion.Vulkan_1_3,
                targetLanguage = TargetLanguage.SPV,
                targetLanguageVersion = TargetLanguageVersion.SPV_1_5,
                code = data,
                sourceEntrypoint = "main",
                defaultVersion = 450,
                defaultProfile = ShaderProfile.None,
                forceDefaultVersionAndProfile = false,
                forwardCompatible = false,
                fileIncluder = IncludeFunction,
                messages = MessageType.Enhanced | MessageType.VulkanRules | MessageType.SpvRules,
            };

            Glslang.NET.Shader shader = new Glslang.NET.Shader(input);

            shader.SetOptions(ShaderOptions.AutoMapBindings | ShaderOptions.AutoMapLocations | ShaderOptions.MapUnusedUniforms);

            if (!shader.Preprocess())
            {
                Debug.Error("GLSL preprocessing failed");
                Debug.Error(shader.GetInfoLog());
                Debug.Error(shader.GetDebugLog());
                return shaderProgram;
            }

            if (!shader.Parse())
            {
                Debug.Error("GLSL parsing failed");
                Debug.Error(shader.GetInfoLog());
                Debug.Error(shader.GetDebugLog());
                Debug.Error(shader.GetPreprocessedCode());
                return shaderProgram;
            }

            Glslang.NET.Program program = new Glslang.NET.Program();

            program.AddShader(shader);

            if (!program.Link(MessageType.SpvRules | MessageType.VulkanRules))
            {
                Debug.Error("GLSL linking failed");
                Debug.Error(shader.GetInfoLog());
                Debug.Error(shader.GetDebugLog());
                return shaderProgram;
            }

            program.GenerateSPIRV(out uint[] words, input.stage);
            
            shaderProgram = new byte[words.Length * 4];
            Buffer.BlockCopy(words, 0, shaderProgram, 0, words.Length * 4);

            return shaderProgram;
        }
        
        public void AddBool(int binding, bool value)
        {
            uniformDef.binding = binding;
            
            if (uniformData.Count == 0)
            {
                Debug.Warning("Tried to add data to shader that has no uniforms");
            }
            
            byte[] data = BitConverter.GetBytes(value);
            if (uniformData[uniformDef].Count + data.Length > VkSettings.MaxBlockUniformMemory)
            {
                Debug.Warning("Tried to push uniform data past max block memory limit");
                return;
            }
            uniformData[uniformDef].AddRange(data);
        }

        public void AddInt(int binding, int value)
        {
            uniformDef.binding = binding;
            
            if (uniformData.Count == 0)
            {
                Debug.Warning("Tried to add data to shader that has no uniforms");
            }
            
            byte[] data = BitConverter.GetBytes(value);
            if (uniformData[uniformDef].Count + data.Length > VkSettings.MaxBlockUniformMemory)
            {
                Debug.Warning("Tried to push uniform data past max block memory limit");
                return;
            }
            uniformData[uniformDef].AddRange(data);
        }

        public void AddFloat(int binding, float value)
        {
            uniformDef.binding = binding;
            
            if (uniformData.Count == 0)
            {
                Debug.Warning("Tried to add data to shader that has no uniforms");
            }
            
            byte[] data = BitConverter.GetBytes(value);
            if (uniformData[uniformDef].Count + data.Length > VkSettings.MaxBlockUniformMemory)
            {
                Debug.Warning("Tried to push uniform data past max block memory limit");
                return;
            }
            uniformData[uniformDef].AddRange(data);
        }

        public void AddVec2(int binding, Vector2 value)
        {
            uniformDef.binding = binding;
            
            if (uniformData.Count == 0)
            {
                Debug.Warning("Tried to add data to shader that has no uniforms");
            }
            
            for (int i = 0; i < 2; i++)
            {
                byte[] data = BitConverter.GetBytes(value[i]);
                if (uniformData[uniformDef].Count + data.Length > VkSettings.MaxBlockUniformMemory)
                {
                    Debug.Warning("Tried to push uniform data past max block memory limit");
                    return;
                }
                uniformData[uniformDef].AddRange(data);
            }
        }

        public void AddVec3(int binding, Vector3 value)
        {
            uniformDef.binding = binding;
            
            if (uniformData.Count == 0)
            {
                Debug.Warning("Tried to add data to shader that has no uniforms");
            }
            
            for (int i = 0; i < 3; i++)
            {
                byte[] data = BitConverter.GetBytes(value[i]);
                if (uniformData[uniformDef].Count + data.Length > VkSettings.MaxBlockUniformMemory)
                {
                    Debug.Warning("Tried to push uniform data past max block memory limit");
                    return;
                }
                uniformData[uniformDef].AddRange(data);
            }
        }

        public void AddVec4(int binding, Vector4 value)
        {
            uniformDef.binding = binding;
            
            if (uniformData.Count == 0)
            {
                Debug.Warning("Tried to add data to shader that has no uniforms");
            }
            
            for (int i = 0; i < 4; i++)
            {
                byte[] data = BitConverter.GetBytes(value[i]);
                if (uniformData[uniformDef].Count + data.Length > VkSettings.MaxBlockUniformMemory)
                {
                    Debug.Warning("Tried to push uniform data past max block memory limit");
                    return;
                }
                uniformData[uniformDef].AddRange(data);
            }
        }

        public void AddMat4(int binding, Matrix4x4 value)
        {
            uniformDef.binding = binding;

            if (uniformData.Count == 0)
            {
                Debug.Warning("Tried to add data to shader that has no uniforms");
            }
            
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    byte[] data = BitConverter.GetBytes(value[i][j]);
                    if (uniformData[uniformDef].Count + data.Length > VkSettings.MaxBlockUniformMemory)
                    {
                        Debug.Warning("Tried to push uniform data past max block memory limit");
                        return;
                    }
                    uniformData[uniformDef].AddRange(data);
                }
            }
        }

        public void Dispose()
        {
            Clean();
        }
    }
}
