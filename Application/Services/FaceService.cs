using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace VeriWork_Admin.Application.Services
{
    public class FaceService
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly double _confidenceThreshold = 0.60; // Adjust as needed

        public FaceService(IHttpClientFactory httpFactory)
        {
            _httpFactory = httpFactory;
        }

        /// <summary>
        /// Detects a face in an image and returns the Face ID.
        /// </summary>
        public async Task<string?> DetectFaceIdAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return null;

            var client = _httpFactory.CreateClient("AzureFace");
            var requestUri = "face/v1.1/detect?returnFaceId=true";

            var payload = JsonSerializer.Serialize(new { url = imageUrl });
            using var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(requestUri, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Face API Error: {response.StatusCode} - {errorBody}");
                return null;
            }

            var stream = await response.Content.ReadAsStreamAsync();
            var arr = await JsonSerializer.DeserializeAsync<JsonElement[]>(stream);
            if (arr == null || arr.Length == 0)
                return null;

            if (arr[0].TryGetProperty("faceId", out var faceIdProp))
                return faceIdProp.GetString();

            return null;
        }

        /// <summary>
        /// Verifies if two Face IDs belong to the same person.
        /// </summary>
        public async Task<(bool isMatch, double confidence)> VerifyFacesAsync(string faceId1, string faceId2)
        {
            var client = _httpFactory.CreateClient("AzureFace");
            var uri = "face/v1.1/verify";

            var payload = JsonSerializer.Serialize(new { faceId1, faceId2 });
            using var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(uri, content);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Face verification error: {error}");
                return (false, 0);
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var isIdentical = root.GetProperty("isIdentical").GetBoolean();
            var confidence = root.GetProperty("confidence").GetDouble();

            return (isIdentical && confidence >= _confidenceThreshold, confidence);
        }

        /// <summary>
        /// Convenience method using image URLs.
        /// </summary>
        public async Task<(bool isMatch, double confidence, string? error)> VerifyByImageUrlsAsync(string urlA, string urlB)
        {
            try
            {
                var idA = await DetectFaceIdAsync(urlA);
                var idB = await DetectFaceIdAsync(urlB);

                if (idA == null || idB == null)
                    return (false, 0, "Could not detect face in one or both images.");

                var (isMatch, confidence) = await VerifyFacesAsync(idA, idB);
                return (isMatch, confidence, null);
            }
            catch (Exception ex)
            {
                return (false, 0, ex.Message);
            }
        }
    }
}
