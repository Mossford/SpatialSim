using SpatialSim.Engine.Core;

namespace SpatialSim.Engine.Rendering
{
    public static class TextureManager
    {
        public static Dictionary<string, int> textureLocToIndex;
        public static List<Texture> textures;
        static Texture missingTexture;
        
        public static void Init()
        {
            textureLocToIndex = new Dictionary<string, int>();
            textures = new List<Texture>();
            missingTexture = new Texture();
            missingTexture.LoadTexture("", "");
            
            Debug.LogInfo("Successful texture manager creation");
        }

        public static bool IsTextureStored(string texture)
        {
            return textureLocToIndex.ContainsKey(texture);
        }

        public static bool LoadTexture(string texture, string pipeline)
        {
            if (!File.Exists(Resources.ImagePath + texture) && texture.Length != 0)
            {
                Debug.Warning($"Could not find file at path {Resources.ImagePath + texture}");
                return false;
            }
            
            if (textureLocToIndex.TryAdd(texture, textures.Count))
            {
                textures.Add(new Texture());
                textures[^1].LoadTexture(texture, pipeline);
                return true;
            }

            Debug.Warning($"Could not add texture {texture} possible duplicate");
            return false;
        }

        public static Texture RetrieveTexture(string texture, string pipeline)
        {
            if (textureLocToIndex.TryGetValue(texture, out int index))
            {
                return textures[index];
            }
            else
            {
                if (LoadTexture(texture, pipeline))
                {
                    return textures[textureLocToIndex[texture]];
                }
            }
            
            //return a missing texture
            return missingTexture;
        }
        
        public static Texture RetrieveTexture(int texture)
        {
            if (texture >= 0 && texture < textures.Count)
            {
                return textures[texture];
            }
            
            //return a missing texture
            return missingTexture;
        }

        public static void Clean()
        {
            for (int i = 0; i < textures.Count; i++)
            {
                textures[i].Clean();
            }
            
            missingTexture.Clean();
            
            Debug.LogInfo("Cleaned up texture manager");
        }
    }
}