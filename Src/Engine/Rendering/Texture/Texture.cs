using SpatialSim.Engine.Core;
using StbImageSharp;
using StbImageWriteSharp;
using ColorComponents = StbImageSharp.ColorComponents;

namespace SpatialSim.Engine.Rendering
{
    public sealed class Texture : IDisposable
    {
        public ITextureDevice? texture;
        public TextureData data;
        public ulong dataSize;

        public void Create()
        {
            texture = AppState.appContext.DeviceFactory.CreateTextureDevice(new TextureData());
        }
        
        public bool LoadTexture(string file, TextureFormat format)
        {
            //check if we can load the image
            try
            {
                FileStream stream = File.Open(Resources.ImagePath + file, FileMode.Open);
                stream.Close();
            }
            catch (Exception e)
            {
                return false;
            }
            
            data = new TextureData();
            if (!File.Exists(Resources.ImagePath + file))
            {
                if(file != "")
                    Debug.Warning($"Loaded image with no found path at {file}, loading missing texture");
                return false;
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
                    return false;
                }
            }

            texture = AppState.appContext.DeviceFactory.CreateTextureDevice(data);
            dataSize = (ulong)data.data.Length;
            Ticks.gpuMemoryAllocation.created += dataSize;
            
            Debug.LogDebug($"Loaded texture at {file}");

            return true;
        }
        
        public bool LoadMissingTexture()
        {
            data = new TextureData();
            MissingTextureData.Create();
            data.data = MissingTextureData.pixels;
            data.info.width = (uint)MissingTextureData.size;
            data.info.height = (uint)MissingTextureData.size;
            data.info.format = TextureFormat.R8G8B8A8Unorm;
            data.info.usage = TextureUsage.Sampler;
            data.info.memoryUsage = TextureMemoryUsage.gpu;
            texture = AppState.appContext.DeviceFactory.CreateTextureDevice(data);
            dataSize = (ulong)data.data.Length;
            Ticks.gpuMemoryAllocation.created += dataSize;

            return true;
        }

        public async void SaveToFile(string path, string file)
        {
            SaveToInternalData();
            
            try
            {

                await Task.Run(() =>
                {
                    ImageWriter writer = new ImageWriter();
                    using var stream = File.Open(path + file, FileMode.Create);

                    writer.WritePng(
                        data.data,
                        (int)data.info.width,
                        (int)data.info.height,
                        StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha,
                        stream
                    );
                });
            }
            catch (Exception e)
            {
                Debug.Error("Could not save texture to png " + e);
            }
        }

        /// <summary>
        /// Writes whats stored on the gpu side to the internal texture data
        /// </summary>
        public void SaveToInternalData()
        {
            texture?.WriteGpuToCpu(ref data);
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