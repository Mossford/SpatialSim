using SpatialSim.Engine.Core;
using SpatialSim.Engine.Rendering.Vulkan;

namespace SpatialSim.Engine.Rendering
{
    public static class TextureManager
    {
        public static Dictionary<string, int> textureLocToIndex;
        public static List<Texture> textures;
        
        public static void Init()
        {
            textureLocToIndex = new Dictionary<string, int>();
            textures = new List<Texture>();
            textures.Add(new Texture());
            // TODO If sampler is not at binding 0 this causes issues
            textures[^1].LoadMissingTexture();
            
            Debug.LogInfo("Successful texture manager creation");
        }

        public static bool IsTextureStored(string texture)
        {
            return textureLocToIndex.ContainsKey(texture);
        }

        public static bool LoadTexture(string texture, TextureFormat format)
        {
            if (!File.Exists(Resources.ImagePath + texture) && texture.Length != 0)
            {
                Debug.Warning($"Could not find file at path {Resources.ImagePath + texture}");
                return false;
            }
            
            if (!textureLocToIndex.ContainsKey(texture))
            {
                textures.Add(new Texture());
                if (!textures[^1].LoadTexture(texture, format))
                {
                    textures.RemoveAt(textures.Count - 1);
                    return false;
                }

                textureLocToIndex.Add(texture, textures.Count - 1);
                return true;
            }

            Debug.Warning($"Could not add texture {texture} possible duplicate");
            return false;
        }

        public static Texture RetrieveTexture(string texture, TextureFormat format = TextureFormat.R8G8B8A8Unorm)
        {
            if (textureLocToIndex.TryGetValue(texture, out int index))
            {
                return textures[index];
            }
            else
            {
                if (LoadTexture(texture, format))
                {
                    return textures[textureLocToIndex[texture]];
                }
            }
            
            //return a missing texture
            return textures[0];
        }
        
        public static Texture RetrieveTexture(int texture)
        {
            if (texture >= 0 && texture < textures.Count)
            {
                return textures[texture];
            }
            
            //return a missing texture
            return textures[0];
        }

        public static int RetrieveTextureIndex(string texture)
        {
            if (textureLocToIndex.TryGetValue(texture, out int index))
            {
                return index;
            }
            
            return 0;
        }

        public static void Clean()
        {
            for (int i = 0; i < textures.Count; i++)
            {
                textures[i].Clean();
            }
            
            Debug.LogInfo("Cleaned up texture manager");
        }
    }
}