using SpatialSim.Engine.Core;
using StbImageSharp;

namespace SpatialSim.Engine.Rendering
{
    public class Texture : IDisposable
    {
        public ITextureDevice texture;
        public TextureData data;
        public ulong dataSize;
        
        public void LoadTexture(string file)
        {
            TextureData data = new TextureData();
            if (!File.Exists(Resources.ImagePath + file))
            {
                if(file != "")
                    Debug.Warning($"Loaded image with no found path at {file}, loading missing texture");
                MissingTextureData.Create();
                data.data = MissingTextureData.pixels;
                data.width = (uint)MissingTextureData.size;
                data.height = (uint)MissingTextureData.size;
                data.format = TextureFormat.R8G8B8A8Srgb;
                data.usage = TextureUsage.Sampler;
                data.memoryUsage = TextureMemoryUsage.gpu;
            }
            else
            {
                StbImage.stbi_set_flip_vertically_on_load(1);
                ImageResult result = ImageResult.FromMemory(File.ReadAllBytes(Resources.ImagePath + file), ColorComponents.RedGreenBlueAlpha);
                data.data = result.Data;
                data.width = (uint)result.Width;
                data.height = (uint)result.Height;
                data.format = TextureFormat.R8G8B8A8Srgb;
                data.usage = TextureUsage.Sampler;
                data.memoryUsage = TextureMemoryUsage.gpu;
            }
            
            texture = AppState.appContext.DeviceFactory.CreateTextureDevice(data);
            dataSize = (ulong)data.data.Length;
            Ticks.gpuMemoryAllocation.created += dataSize;
            
            Debug.LogDebug($"Loaded texture at {file}");
        }

        public void Clean()
        {
            Ticks.gpuMemoryAllocation.deleted += dataSize;
            texture?.Clean();
            Debug.LogDebug($"Cleaned texture");
        }

        public void Dispose()
        {
            Clean();
        }
    }
}