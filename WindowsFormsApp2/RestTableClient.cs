using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace InventoryFlow
{
    // Generic client for the backend's generic CRUD router (crud.js) — one instance per table
    // (materials, income_log, outcome_log, manufacturers, sellers, projects_storages, users, ...).
    public class RestTableClient
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;
        private readonly string _table;
        private readonly JavaScriptSerializer _json = new JavaScriptSerializer();

        public RestTableClient(string baseUrl, string apiKey, string table)
        {
            _baseUrl = (baseUrl ?? "").TrimEnd('/');
            _table = table;
            _http = new HttpClient();
            if (!string.IsNullOrEmpty(apiKey))
                _http.DefaultRequestHeaders.Add("x-api-key", apiKey);
        }

        public async Task<List<Dictionary<string, object>>> ListAsync()
        {
            var body = await GetStringAsync("");
            return _json.Deserialize<List<Dictionary<string, object>>>(body);
        }

        public async Task<Dictionary<string, object>> GetAsync(int id)
        {
            var body = await GetStringAsync("/" + id);
            return _json.Deserialize<Dictionary<string, object>>(body);
        }

        public async Task DeleteAsync(int id)
        {
            var response = await _http.DeleteAsync(_baseUrl + "/" + _table + "/" + id);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Не вдалося видалити запис у {_table} ({(int)response.StatusCode})");
        }

        public async Task<Dictionary<string, object>> UpdateAsync(int id, Dictionary<string, object> fields)
        {
            var content = new StringContent(_json.Serialize(fields), Encoding.UTF8, "application/json");
            var response = await _http.PutAsync(_baseUrl + "/" + _table + "/" + id, content);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Не вдалося оновити запис у {_table} ({(int)response.StatusCode}): {body}");
            return _json.Deserialize<Dictionary<string, object>>(body);
        }

        public async Task<Dictionary<string, object>> CreateAsync(Dictionary<string, object> fields)
        {
            var content = new StringContent(_json.Serialize(fields), Encoding.UTF8, "application/json");
            var response = await _http.PostAsync(_baseUrl + "/" + _table, content);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Не вдалося створити запис у {_table} ({(int)response.StatusCode}): {body}");
            return _json.Deserialize<Dictionary<string, object>>(body);
        }

        private async Task<string> GetStringAsync(string suffix)
        {
            var response = await _http.GetAsync(_baseUrl + "/" + _table + suffix);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Запит {_table}{suffix} не вдався ({(int)response.StatusCode}): {body}");
            return body;
        }
    }

    public static class DictExtensions
    {
        public static object Get(this Dictionary<string, object> dict, string key)
        {
            return dict != null && dict.TryGetValue(key, out var v) ? v : null;
        }

        public static string GetString(this Dictionary<string, object> dict, string key)
        {
            var v = Get(dict, key);
            return v != null ? Convert.ToString(v) : "";
        }
    }
}
