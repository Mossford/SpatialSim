using SpatialSim.Engine.Core;
using SpatialSim.Engine.Rendering.Vulkan;

namespace SpatialSim.Engine.Rendering
{
    public static class ShaderManager
    {
        public static Dictionary<string, int> shaderLocToIndex;
        public static List<Shader> shaders;
        
        public static void Init()
        {
            shaderLocToIndex = new Dictionary<string, int>();
            shaders = new List<Shader>();
        }

        public static bool LoadShader(ShaderSettings settings)
        {
            if (!File.Exists(Resources.ShaderPath + settings.file))
                return false;
            
            if (shaderLocToIndex.TryAdd(settings.file, shaders.Count))
            {
                Shader shader = new Shader();
                shader.Create(settings);
                shaders.Add(shader);
                return true;
            }

            return false;
        }

        public static Shader RetrieveShader(ShaderSettings settings)
        {
            if (shaderLocToIndex.TryGetValue(settings.file, out int index))
            {
                return shaders[index];
            }
            else
            {
                if (LoadShader(settings))
                {
                    return shaders[shaderLocToIndex[settings.file]];
                }
            }

            // TODO Add defualt shader
            Debug.Warning("Shader " + settings.file + " not found");
            return null;
        }
        
        public static Shader RetrieveShader(string file)
        {
            if (shaderLocToIndex.TryGetValue(file, out int index))
            {
                return shaders[index];
            }

            // TODO Add defualt shader
            Debug.Warning("Shader " + file + " not found");
            return null;
        }
        
        public static Shader RetrieveShader(int shader)
        {
            if (shader >= 0 && shader < shaders.Count)
            {
                return shaders[shader];
            }
            
            // TODO Add defualt shader
            return null;
        }

        public static void Clean()
        {
            for (int i = 0; i < shaders.Count; i++)
            {
                shaders[i].Clean();
            }
            
            Debug.LogInfo("Cleaned up shader manager");
        }
    }
}