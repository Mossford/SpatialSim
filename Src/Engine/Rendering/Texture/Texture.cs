using SpatialSim.Engine.Core;

namespace SpatialSim.Engine.Rendering
{
    public class Texture : IDisposable
    {
        public ITextureDevice texture;
        public TextureData data;
        
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
                data.format = TextureFormat.R8G8B8Unorm;
                data.usage = TextureUsage.Sampler;
                data.memoryUsage = TextureMemoryUsage.gpu;
            }
            
            texture = AppState.appContext.DeviceFactory.CreateTextureDevice(data);
        }

        public void Clean()
        {
            texture?.Clean();
        }

        public void Dispose()
        {
            Clean();
        }
    }
}