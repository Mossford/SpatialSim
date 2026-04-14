using SpatialSim.Engine.Core;

namespace SpatialSim.Engine.Network
{
    public static class Downloader
    {
        public static async Task<bool> DownloadFileFromUrl(string url, string fileName, string path)
        {
            try
            {
                HttpClient client = new HttpClient()
                {
                    Timeout = TimeSpan.FromSeconds(60)
                };
                HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();
                Stream fileStream = await response.Content.ReadAsStreamAsync();
                FileStream writeStream = File.Open(path + fileName, FileMode.Create);
                await fileStream.CopyToAsync(writeStream);
                
                fileStream.Close();
                writeStream.Close();
            }
            catch (Exception e)
            {
                Debug.Error($"Could not download file from {url}, " + e);
                return false;
            }

            Debug.LogDebug($"Downloaded file from {url} to {path + fileName}");
            return true;
        }

        public static async Task<byte[]> DownloadBytesFromUrl(string url)
        {
            byte[] data = [];

            try
            {
                HttpClient client = new HttpClient()
                {
                    Timeout = TimeSpan.FromSeconds(60)
                };
                HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();
                Stream fileStream = await response.Content.ReadAsStreamAsync();
                Span<byte> streamData = [];
                fileStream.ReadExactly(streamData);
                fileStream.Close();
                data = streamData.ToArray();
                
                Debug.LogDebug($"Downloaded file from {url}");
            }
            catch (Exception e)
            {
                Debug.Error($"Could not download file from {url}, " + e);
            }
            
            return data;
        }
    }
}