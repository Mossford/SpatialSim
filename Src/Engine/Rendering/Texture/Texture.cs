using SpatialSim.Engine.Core;
using StbImageSharp;

namespace SpatialSim.Engine.Rendering
{
    public sealed class Texture : IDisposable
    {
        public ITextureDevice texture;
        public TextureData data;
        public ulong dataSize;
        
        public void LoadTexture(string file, TextureFormat format)
        {
            data = new TextureData();
            if (!File.Exists(Resources.ImagePath + file))
            {
                if(file != "")
                    Debug.Warning($"Loaded image with no found path at {file}, loading missing texture");
                MissingTextureData.Create();
                data.data = MissingTextureData.pixels;
                data.info.width = (uint)MissingTextureData.size;
                data.info.height = (uint)MissingTextureData.size;
                data.info.format = TextureFormat.R8G8B8A8Srgb;
                data.info.usage = TextureUsage.Sampler;
                data.info.memoryUsage = TextureMemoryUsage.gpu;
            }
            else
            {
                StbImage.stbi_set_flip_vertically_on_load(1);
                try
                {
                    ImageResult result = ImageResult.FromMemory(File.ReadAllBytes(Resources.ImagePath + file), ColorComponents.RedGreenBlueAlpha);
                    data.data = result.Data;
                    data.info.width = (uint)result.Width;
                    data.info.height = (uint)result.Height;
                    data.info.format = format;
                    data.info.usage = TextureUsage.Sampler;
                    data.info.memoryUsage = TextureMemoryUsage.gpu;
                }
                catch (Exception e)
                {
                    Debug.Error($"Tried to load image with error, {e}");
                    MissingTextureData.Create();
                    data.data = MissingTextureData.pixels;
                    data.info.width = (uint)MissingTextureData.size;
                    data.info.height = (uint)MissingTextureData.size;
                    data.info.format = TextureFormat.R8G8B8A8Srgb;
                    data.info.usage = TextureUsage.Sampler;
                    data.info.memoryUsage = TextureMemoryUsage.gpu;
                }
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