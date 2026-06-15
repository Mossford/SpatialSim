using System.Numerics;
using Silk.NET.Vulkan;
using SpatialSim.Engine.Rendering;

namespace SpatialSim.Engine.Core
{
    public struct NoiseSettings
    {
        public int[] dimensions;
        public int cellSize;

        public NoiseSettings(int[] dimensions, int cellSize)
        {
            this.dimensions = dimensions;
            this.cellSize = cellSize;
        }
    }
    
    public static class Noise
    {
        public static Texture CreateWorleyNoise(in NoiseSettings settings)
        {
            Texture texture = new Texture();

            if (settings.dimensions.Length == 0 || settings.dimensions.Length > 3)
            {
                Debug.Warning($"Tried to create worley noise texture with wrong dimension size {settings.dimensions.Length} returning empty texture");
                return texture;
            }

            int width = settings.dimensions[0];
            int height = settings.dimensions.Length == 2 ? settings.dimensions[1] : 0;
            int depth = settings.dimensions.Length == 3 ? settings.dimensions[2] : 1;
                
            TextureData textureData = new TextureData()
            {
                info = new TextureInfo()
                {
                    width = (uint)width,
                    height = (uint)height,
                    depth = (uint)depth,
                    format = TextureFormat.R8Unorm,
                    memoryUsage = TextureMemoryUsage.cpu,
                    filter = TextureFilter.Linear,
                    type = settings.dimensions.Length == 3 ? TextureType.Type3D : TextureType.Type2D
                },
            };

            textureData.data = settings.dimensions.Length == 3
                ? Create3DWorleyNoise(settings)
                : Create2DWorleyNoise(settings);

            texture.data = textureData;
            texture.Create();

            return texture;
        }
        
        public static Texture CreateWorleyNoiseNormals(in NoiseSettings settings)
        {
            Texture texture = new Texture();

            if (settings.dimensions.Length == 0 || settings.dimensions.Length > 3)
            {
                Debug.Warning($"Tried to create worley noise texture with wrong dimension size {settings.dimensions.Length} returning empty texture");
                return texture;
            }

            int width = settings.dimensions[0];
            int height = settings.dimensions.Length == 2 ? settings.dimensions[1] : 0;
            int depth = settings.dimensions.Length == 3 ? settings.dimensions[2] : 1;
                
            TextureData textureData = new TextureData()
            {
                info = new TextureInfo()
                {
                    width = (uint)width,
                    height = (uint)height,
                    depth = (uint)depth,
                    format = TextureFormat.R8G8B8A8Unorm,
                    memoryUsage = TextureMemoryUsage.cpu,
                    filter = TextureFilter.Linear,
                    type = settings.dimensions.Length == 3 ? TextureType.Type3D : TextureType.Type2D
                },
            };

            textureData.data = settings.dimensions.Length == 3
                ? Create3DWorleyNoise(settings)
                : Create2DWorleyNoiseNormals(settings);

            texture.data = textureData;
            texture.Create();

            return texture;
        }
        
        static byte[] Create3DWorleyNoise(in NoiseSettings settings)
        {
            int width = settings.dimensions[0];
            int height = settings.dimensions[1];
            int depth = settings.dimensions[2];
            byte[] data = new byte[width * height * depth];

            int cellSize = settings.cellSize;
            int cellsWidth = width / cellSize;
            int cellsHeight = height / cellSize;
            int cellsDepth = depth / cellSize;
            
            Vector3[] points = new Vector3[cellsWidth * cellsHeight * cellsDepth];

            //TODO use some static seeded random
            Random random = new Random();
            
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = new Vector3(random.NextSingle() * cellSize, random.NextSingle() * cellSize, random.NextSingle() * cellSize);
            }

            float maxDistance = cellSize * MathF.Sqrt(3f);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        int cellX = x / cellSize;
                        int cellY = y / cellSize;
                        int cellZ = z / cellSize;

                        float closestDistance = float.MaxValue;

                        for (int cx = -1; cx <= 1; cx++)
                        {
                            for (int cy = -1; cy <= 1; cy++)
                            {
                                for (int cz = -1; cz <= 1; cz++)
                                {
                                    int surCellX = cellX + cx;
                                    int surCellY = cellY + cy;
                                    int surCellZ = cellZ + cz;

                                    if (surCellX < 0 || surCellX >= cellsWidth ||
                                        surCellY < 0 || surCellY >= cellsHeight ||
                                        surCellZ < 0 || surCellZ >= cellsDepth)
                                    {
                                        continue;
                                    }

                                    int pointIndex = surCellX * cellsHeight + surCellY;
                                    Vector3 pointOffset = points[pointIndex] + new Vector3(surCellX * cellSize, surCellY * cellSize, surCellZ * cellSize);

                                    float distance = Vector3.Distance(new Vector3(x, y, z), pointOffset);

                                    if (distance < closestDistance)
                                    {
                                        closestDistance = distance;
                                    }   
                                }
                            }
                        }

                        int index = z * height * width + y * width + x;
                        data[index] = (byte)(Math.Clamp(closestDistance / maxDistance, 0f, 1f) * 255f);
                    }
                }
            }
            
            return data;
        }
        
        static byte[] Create2DWorleyNoise(in NoiseSettings settings)
        {
            int width = settings.dimensions[0];
            int height = settings.dimensions[1];
            byte[] data = new byte[width * height];

            int cellSize = settings.cellSize;
            int cellsWidth = width / cellSize;
            int cellsHeight = height / cellSize;
            
            Vector2[] points = new Vector2[cellsWidth * cellsHeight];

            //TODO use some static seeded random
            Random random = new Random();
            
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = new Vector2(random.NextSingle() * cellSize, random.NextSingle() * cellSize);
            }

            float maxDistance = cellSize * MathF.Sqrt(2f);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int cellX = x / cellSize;
                    int cellY = y / cellSize;

                    float closestDistance = float.MaxValue;

                    for (int cx = -1; cx <= 1; cx++)
                    {
                        for (int cy = -1; cy <= 1; cy++)
                        {
                            int surCellX = cellX + cx;
                            int surCellY = cellY + cy;

                            if (surCellX < 0 || surCellX >= cellsWidth ||
                                surCellY < 0 || surCellY >= cellsHeight)
                            {
                                continue;
                            }

                            int pointIndex = surCellX * cellsHeight + surCellY;
                            Vector2 pointOffset = points[pointIndex] + new Vector2(surCellX * cellSize, surCellY * cellSize);

                            float distance = Vector2.Distance(new Vector2(x, y), pointOffset);

                            if (distance < closestDistance)
                            {
                                closestDistance = distance;
                            }
                        }
                    }

                    int index = y * width + x;
                    data[index] = (byte)(Math.Clamp(closestDistance / maxDistance, 0f, 1f) * 255f);
                }
            }
            
            return data;
        }
        
        static byte[] Create2DWorleyNoiseNormals(in NoiseSettings settings)
        {
            int width = settings.dimensions[0];
            int height = settings.dimensions[1];
            byte[] data = new byte[width * height * TextureFormat.R8G8B8A8Unorm.GetBytePerPixel()];

            int cellSize = settings.cellSize;
            int cellsWidth = width / cellSize;
            int cellsHeight = height / cellSize;
            
            Vector2[] points = new Vector2[cellsWidth * cellsHeight];

            //TODO use some static seeded random
            Random random = new Random();
            
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = new Vector2(random.NextSingle() * cellSize, random.NextSingle() * cellSize);
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int cellX = x / cellSize;
                    int cellY = y / cellSize;

                    float closestDistance = float.MaxValue;
                    Vector3 dir = Vector3.Zero;

                    for (int cx = -1; cx <= 1; cx++)
                    {
                        for (int cy = -1; cy <= 1; cy++)
                        {
                            int surCellX = cellX + cx;
                            int surCellY = cellY + cy;

                            if (surCellX < 0 || surCellX >= cellsWidth ||
                                surCellY < 0 || surCellY >= cellsHeight)
                            {
                                continue;
                            }

                            int pointIndex = surCellX * cellsHeight + surCellY;
                            Vector2 pointOffset = points[pointIndex] + new Vector2(surCellX * cellSize, surCellY * cellSize);

                            Vector2 pixel = new Vector2(x, y);
                            float distance = Vector2.Distance(pixel, pointOffset);

                            if (distance < closestDistance)
                            {
                                closestDistance = distance;
                                Vector2 gradient = Vector2.Normalize(pointOffset - pixel);
                                dir = Vector3.Normalize(new Vector3(gradient, 1.0f));
                            }
                        }
                    }

                    int index = (y * width + x) * TextureFormat.R8G8B8A8Unorm.GetBytePerPixel();
                    data[index] = (byte)((dir.X + 1f) * 255f / 2f);
                    data[index + 1] = (byte)((dir.Y + 1f) * 255f / 2f);
                    data[index + 2] = (byte)((dir.Z + 1f) * 255f / 2f);
                    data[index + 3] = 255;
                }
            }
            
            return data;
        }
    }
}