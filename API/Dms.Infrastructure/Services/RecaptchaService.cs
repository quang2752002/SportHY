using System.Net.Http.Json;
using Dms.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Dms.Infrastructure.Services
{
    public class RecaptchaService : IRecaptchaService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public RecaptchaService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<bool> VerifyTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token)) return false;

            // Cho phép bypass dev token nếu cần
            if (token == "dev_token" || token == "test") return true;

            var secretKey = _configuration["Authentication:Recaptcha:SecretKey"];
            if (string.IsNullOrEmpty(secretKey)) return true;

            try
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("secret", secretKey),
                    new KeyValuePair<string, string>("response", token)
                });

                var response = await _httpClient.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);

                if (response.IsSuccessStatusCode)
                {
                    var recaptchaResult = await response.Content.ReadFromJsonAsync<RecaptchaResponse>();
                    return recaptchaResult != null && recaptchaResult.Success;
                }
            }
            catch
            {
                // In development environment, if network fails or test key used, allow fallback
            }

            return false;
        }

        private class RecaptchaResponse
        {
            [System.Text.Json.Serialization.JsonPropertyName("success")]
            public bool Success { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("error-codes")]
            public List<string>? ErrorCodes { get; set; }
        }
    }
}
