using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using OpenTap;

namespace ChatboxPlugin.Service.AI
{
    /// <summary>
    /// Service class to interact with the AI API.
    /// </summary>
    public class AIService : IDisposable
    {
        private readonly HttpClient _httpClient;
        private const string ApiBaseUrl = "https://generativelanguage.googleapis.com/v1beta/models/";
        private const string ModelName = "gemini-2.5-pro-exp-03-25";
        private readonly string _apiKey;
        private readonly TraceSource _log = Log.CreateSource("AIService");

        /// <summary>
        /// Initializes a new instance of the AIService class.
        /// Reads the API Key from environment variables.
        /// </summary>
        public AIService()
        {
            _httpClient = new HttpClient();
            _apiKey = Environment.GetEnvironmentVariable("API_KEY");

            if (string.IsNullOrEmpty(_apiKey))
            {
                _log.Warning("API_KEY environment variable not found or is empty. API calls will fail.");
            }
        }

        /// <summary>
        /// Sends a prompt to the AI and returns the AI's response.
        /// </summary>
        /// <param name="prompt">The user's input to send to the AI.</param>
        /// <returns>The AI-generated response or an error message.</returns>
        public async Task<string> GetAiResponseAsync(string prompt)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                // Return error immediately if key wasn't loaded in constructor
                return "Error: API Key not configured. Please set a valid API key in the 'API_KEY' environment variable.";
            }

            // Construct the full API endpoint URL including the model and API key
            string apiEndpoint = $"{ApiBaseUrl}{ModelName}:generateContent?key={_apiKey}";

            // Configure generation parameters for AI API
            const int maxTokens = 20000; // maxOutputTokens in AI API
            const double temperature = 0.5; // Control randomness

            // Structure the request payload according to the AI API specification
            // See: https://ai.google.dev/api/rest/v1beta/models/generateContent
            var requestData = new
            {
                contents = new[] // Array of content blocks
                {
                    new {
                        // Single-turn requests, defaults to 'user'
                        parts = new[] { new { text = prompt } }
                    }
                },
                generationConfig = new // Optional generation configuration
                {
                    temperature = temperature,
                    maxOutputTokens = maxTokens
                }
            };

            // Serialize the request data to JSON format
            var jsonPayload = JsonConvert.SerializeObject(requestData, new JsonSerializerSettings
            { NullValueHandling = NullValueHandling.Ignore });
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync(apiEndpoint, content);
                // Read the response content BEFORE checking status code to potentially log error details
                string jsonResponse = await response.Content.ReadAsStringAsync();

                // Check if the request was successful
                if (!response.IsSuccessStatusCode)
                {
                    // Log the detailed error response from the API
                    _log.Error($"Error response from AI API: {response.StatusCode} - {jsonResponse}");
                    return $"Error connecting to Google AI API: {response.StatusCode}. Response: {jsonResponse}";
                }

                // Deserialize the successful response using the new helper classes
                var result = JsonConvert.DeserializeObject<AIResponse>(jsonResponse);

                // Extract the response text from the structure, handling potential nulls safely
                // AI response structure: candidates -> content -> parts -> text
                var aiMessage = result?.Candidates?.FirstOrDefault()? // Get the first candidate (usually only one for standard requests)
                                     .Content?.Parts?.FirstOrDefault()? // Get the first part within the candidate's content
                                     .Text?.Trim(); // Get the text content and trim whitespace

                // Log the extracted response text for debugging
                _log.Debug($"Raw AI response text: {aiMessage}");

                return aiMessage ?? "Error: Empty or invalid response structure from AI API.";
            }
            catch (HttpRequestException ex)
            {
                _log.Error("HTTP request failed when calling AI API.", ex);
                return $"Error connecting to Google AI API service: {ex.Message}";
            }
            catch (JsonException ex)
            {
                _log.Error("Failed to parse JSON response from AI API.", ex);
                return $"Error parsing AI API response: {ex.Message}";
            }
            catch (Exception ex) // Catch-all for other unexpected issues
            {
                _log.Error("An unexpected error occurred during the AI API call.", ex);
                return $"Error Unexpected: {ex.Message}";
            }
        }

        /// <summary>
        /// Disposes the HttpClient when the service is no longer needed.
        /// </summary>
        public void Dispose()
        {
            _httpClient?.Dispose();
            GC.SuppressFinalize(this); // Standard IDisposable pattern practice
        }

        // Helper classes for API JSON deserialization
        private class AIResponse
        {
            [JsonProperty("candidates")]
            public Candidate[] Candidates { get; set; }
        }

        private class Candidate
        {
            [JsonProperty("content")]
            public Content Content { get; set; }
        }

        private class Content
        {
            [JsonProperty("parts")]
            public Part[] Parts { get; set; }
        }

        private class Part
        {
            [JsonProperty("text")]
            public string Text { get; set; }
        }
    }
}