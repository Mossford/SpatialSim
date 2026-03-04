using System.Numerics;
using Glslang.NET;
using SpatialSim.Engine.Core;

namespace SpatialSim.Engine.Rendering
{
    public interface IShaderDevice
    {
        public void Create(ShaderSettings settings, in byte[] code);
        public void Clean();
    }

    public sealed class Shader : IDisposable
    {
        public ShaderSettings settings;

        /// <summary>
        /// Stores the api shader information
        /// </summary>
        public IShaderDevice? shader;

        /// <summary>
        /// Max uniform section size of 4096 bytes
        /// </summary>
        public const int MaxBlockUniformMemory = 1 << 12;

        public List<byte> uniformData;

        public void Create(ShaderSettings settings)
        {
            this.settings = settings;
            uniformData = new List<byte>();
            
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
        
        public void AddBool(bool value)
        {
            byte[] data = BitConverter.GetBytes(value);
            if (uniformData.Count + data.Length > MaxBlockUniformMemory)
            {
                Debug.Warning("Tried to push uniform data past max allowed memory limit");
                return;
            }
            uniformData.AddRange(data);
        }

        public void AddInt(int value)
        {
            byte[] data = BitConverter.GetBytes(value);
            if (uniformData.Count + data.Length > MaxBlockUniformMemory)
            {
                Debug.Warning("Tried to push uniform data past max allowed memory limit");
                return;
            }
            uniformData.AddRange(data);
        }

        public void AddFloat(float value)
        {
            byte[] data = BitConverter.GetBytes(value);
            if (uniformData.Count + data.Length > MaxBlockUniformMemory)
            {
                Debug.Warning("Tried to push uniform data past max allowed memory limit");
                return;
            }
            uniformData.AddRange(data);
        }

        public void AddVec2(Vector2 value)
        {
            for (int i = 0; i < 2; i++)
            {
                byte[] data = BitConverter.GetBytes(value[i]);
                if (uniformData.Count + data.Length > MaxBlockUniformMemory)
                {
                    Debug.Warning("Tried to push uniform data past max allowed memory limit");
                    return;
                }
                uniformData.AddRange(data);
            }
        }

        public void AddVec3(Vector3 value)
        {
            for (int i = 0; i < 3; i++)
            {
                byte[] data = BitConverter.GetBytes(value[i]);
                if (uniformData.Count + data.Length > MaxBlockUniformMemory)
                {
                    Debug.Warning("Tried to push uniform data past max allowed memory limit");
                    return;
                }
                uniformData.AddRange(data);
            }
        }

        public void AddVec4(Vector4 value)
        {
            for (int i = 0; i < 4; i++)
            {
                byte[] data = BitConverter.GetBytes(value[i]);
                if (uniformData.Count + data.Length > MaxBlockUniformMemory)
                {
                    Debug.Warning("Tried to push uniform data past max allowed memory limit");
                    return;
                }
                uniformData.AddRange(data);
            }
        }

        public void AddMat4(Matrix4x4 value)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    byte[] data = BitConverter.GetBytes(value[i][j]);
                    if (uniformData.Count + data.Length > MaxBlockUniformMemory)
                    {
                        Debug.Warning("Tried to push uniform data past max allowed memory limit");
                        return;
                    }
                    uniformData.AddRange(data);
                }
            }
        }

        public void Dispose()
        {
            Clean();
        }
    }
}
