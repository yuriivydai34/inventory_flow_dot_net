using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace InventoryFlow
{
    public class MaterialsApiClient
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;
        private readonly JavaScriptSerializer _json = new JavaScriptSerializer();

        public MaterialsApiClient(string baseUrl, string apiKey)
        {
            _baseUrl = (baseUrl ?? "").TrimEnd('/');
            _http = new HttpClient();
            if (!string.IsNullOrEmpty(apiKey))
                _http.DefaultRequestHeaders.Add("x-api-key", apiKey);
        }

        public async Task<List<Dictionary<string, object>>> ListAsync()
        {
            var body = await GetStringAsync("/materials");
            return _json.Deserialize<List<Dictionary<string, object>>>(body);
        }

        public async Task<Dictionary<string, object>> GetAsync(int id)
        {
            var body = await GetStringAsync("/materials/" + id);
            return _json.Deserialize<Dictionary<string, object>>(body);
        }

        public async Task DeleteAsync(int id)
        {
            var response = await _http.DeleteAsync(_baseUrl + "/materials/" + id);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Не вдалося видалити позицію ({(int)response.StatusCode})");
        }

        public async Task<Dictionary<string, object>> UpdateAsync(int id, Dictionary<string, object> fields)
        {
            var content = new StringContent(_json.Serialize(fields), Encoding.UTF8, "application/json");
            var response = await _http.PutAsync(_baseUrl + "/materials/" + id, content);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Не вдалося оновити позицію ({(int)response.StatusCode}): {body}");
            return _json.Deserialize<Dictionary<string, object>>(body);
        }

        public async Task<Dictionary<string, object>> CreateAsync(Dictionary<string, object> fields)
        {
            var content = new StringContent(_json.Serialize(fields), Encoding.UTF8, "application/json");
            var response = await _http.PostAsync(_baseUrl + "/materials", content);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Не вдалося створити позицію ({(int)response.StatusCode}): {body}");
            return _json.Deserialize<Dictionary<string, object>>(body);
        }

        private async Task<string> GetStringAsync(string path)
        {
            var response = await _http.GetAsync(_baseUrl + path);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Запит {path} не вдався ({(int)response.StatusCode}): {body}");
            return body;
        }
    }
}
