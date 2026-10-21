using System.Text;
using System.Text.Json;

namespace RaceDay.Web.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        private void AddSessionHeaders()
        {
            var userId = _httpContextAccessor.HttpContext?.Session.GetString("UserId");
            var role = _httpContextAccessor.HttpContext?.Session.GetString("UserRole");

            _httpClient.DefaultRequestHeaders.Remove("X-UserId");
            _httpClient.DefaultRequestHeaders.Remove("X-UserRole");

            if (!string.IsNullOrEmpty(userId))
                _httpClient.DefaultRequestHeaders.Add("X-UserId", userId);
            
            if (!string.IsNullOrEmpty(role))
                _httpClient.DefaultRequestHeaders.Add("X-UserRole", role);
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            AddSessionHeaders();
            var response = await _httpClient.GetAsync(endpoint);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            return default;
        }

        public async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T data)
        {
            AddSessionHeaders();
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await _httpClient.PostAsync(endpoint, content);
        }

        public async Task<HttpResponseMessage> PutAsync<T>(string endpoint, T data)
        {
            AddSessionHeaders();
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await _httpClient.PutAsync(endpoint, content);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string endpoint)
        {
            AddSessionHeaders();
            return await _httpClient.DeleteAsync(endpoint);
        }

        public async Task<TResponse?> PostAsync<T, TResponse>(string endpoint, T data)
        {
            AddSessionHeaders();
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(endpoint, content);
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TResponse>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            return default;
        }
    }
}