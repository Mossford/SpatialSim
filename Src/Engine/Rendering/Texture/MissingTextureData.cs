namespace SpatialSim.Engine.Rendering
{
    public static class MissingTextureData
    {
        public static byte[] pixels;
        public static int size = 48;
        static bool created;

        public static void Create()
        {
            if (created)
            {
                return;
            }

            pixels = new byte[size * size * 4];

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    int baseIndex = (y * size + x) * 4;

                    if ((x / 4 + y / 4) % 2 == 0)
                    {
                        pixels[baseIndex + 0] = 255;
                        pixels[baseIndex + 1] = 0;
                        pixels[baseIndex + 2] = 255;
                        pixels[baseIndex + 3] = 255;
                    }
                    else
                    {
                        pixels[baseIndex + 0] = 20;
                        pixels[baseIndex + 1] = 20;
                        pixels[baseIndex + 2] = 20;
                        pixels[baseIndex + 3] = 255;
                    }
                }
            }

            created = true;
        }
    }
}