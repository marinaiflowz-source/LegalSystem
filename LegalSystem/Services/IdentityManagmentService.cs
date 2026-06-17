using LegalSystem.DTOs;
using System.Text;
using System.Text.Json;

namespace LegalSystem.Services
{
    public class IdentityManagmentService
    {
        private readonly HttpClient _httpClient;

        private readonly string _apiKey;
        private readonly string _authApiUrl;
        public IdentityManagmentService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;

            _apiKey = config["AuthApiKey"] ?? throw new ArgumentNullException("AuthApiKey");
            _authApiUrl = config["AuthApiUrl"] ?? throw new ArgumentNullException("AuthApiUrl");

            _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
            _httpClient.BaseAddress = new Uri(_authApiUrl);
        }

        public async Task<IdentityResponse<AddIdentityUserResponse>> RegisterUser(AddIdentityUserCommand model)
        {
            var result = new IdentityResponse<AddIdentityUserResponse>();
            var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_authApiUrl}/AppUser/Register", jsonContent);
            if (response.IsSuccessStatusCode)
            {
                var desResult = await response.Content.ReadFromJsonAsync<IdentityResponse<AddIdentityUserResponse>>();
                return desResult ?? result;
            }
            else
                return result;
        }
    }
}
