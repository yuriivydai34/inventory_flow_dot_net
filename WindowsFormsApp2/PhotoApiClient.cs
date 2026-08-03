using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace InventoryFlow
{
    public class PhotoInfo
    {
        public string id { get; set; }
        public string inventory_number { get; set; }
        public string original_name { get; set; }
        public string mime_type { get; set; }
        public long size_bytes { get; set; }
        public string created_at { get; set; }
    }

    public class PhotoApiClient
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;

        public PhotoApiClient(string baseUrl, string apiKey)
        {
            _baseUrl = (baseUrl ?? "").TrimEnd('/');
            _http = new HttpClient();
            if (!string.IsNullOrEmpty(apiKey))
                _http.DefaultRequestHeaders.Add("x-api-key", apiKey);
        }

        public async Task<List<PhotoInfo>> ListAsync(string inventoryNumber)
        {
            var url = _baseUrl + "/photos?inventory_number=" + Uri.EscapeDataString(inventoryNumber);
            var response = await _http.GetAsync(url);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Не вдалося отримати список фото ({(int)response.StatusCode}): {body}");
            return new JavaScriptSerializer().Deserialize<List<PhotoInfo>>(body);
        }

        public async Task<PhotoInfo> UploadAsync(string inventoryNumber, string filePath)
        {
            using (var content = new MultipartFormDataContent())
            {
                var bytes = File.ReadAllBytes(filePath);
                var fileContent = new ByteArrayContent(bytes);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(GetMimeType(filePath));
                content.Add(fileContent, "photo", Path.GetFileName(filePath));
                content.Add(new StringContent(inventoryNumber), "inventory_number");

                var response = await _http.PostAsync(_baseUrl + "/photos", content);
                var body = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Не вдалося завантажити фото ({(int)response.StatusCode}): {body}");
                return new JavaScriptSerializer().Deserialize<PhotoInfo>(body);
            }
        }

        public async Task DownloadAsync(string photoId, string destPath)
        {
            var response = await _http.GetAsync(_baseUrl + "/photos/" + Uri.EscapeDataString(photoId));
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Не вдалося завантажити файл ({(int)response.StatusCode})");
            var bytes = await response.Content.ReadAsByteArrayAsync();
            File.WriteAllBytes(destPath, bytes);
        }

        public async Task DeleteAsync(string photoId)
        {
            var response = await _http.DeleteAsync(_baseUrl + "/photos/" + Uri.EscapeDataString(photoId));
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Не вдалося видалити фото ({(int)response.StatusCode})");
        }

        private static string GetMimeType(string filePath)
        {
            switch (Path.GetExtension(filePath).ToLowerInvariant())
            {
                case ".jpg":
                case ".jpeg": return "image/jpeg";
                case ".png": return "image/png";
                case ".webp": return "image/webp";
                case ".heic": return "image/heic";
                case ".heif": return "image/heif";
                default: return "application/octet-stream";
            }
        }
    }
}
