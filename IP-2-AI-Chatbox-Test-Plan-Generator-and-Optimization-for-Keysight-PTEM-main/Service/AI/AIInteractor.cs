using System.Threading.Tasks;
using OpenTap;

namespace ChatboxPlugin.Service.AI
{
    /// <summary>
    /// Handles interactions with the AI service.
    /// </summary>
    public class AiInteractor
    {
        private readonly AIService _aiService;
        private readonly TraceSource _log = Log.CreateSource("AIInteractor");

        /// <summary>
        /// Initializes a new instance of the AiInteractor class.
        /// </summary>
        /// <param name="aiService">The AI service to interact with.</param>
        public AiInteractor(AIService aiService)
        {
            _aiService = aiService;
        }

        /// <summary>
        /// Sends a prompt to the AI service and retrieves the response.
        /// </summary>
        /// <param name="prompt">The prompt to send to the AI.</param>
        /// <returns>The AI-generated response.</returns>
        public async Task<string> GetAiResponseAsync(string prompt)
        {
            const int maxRetries = 6; // Maximum number of retry attempts
            string[] retryableErrors = new[]
            {
                "Error: Empty response from AI service.", // Retry on empty response, common issue with the service
                "Error connecting to OpenRouter AI service:" // Retry on network-related errors
            };

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                _log.Debug($"Attempt {attempt + 1} of {maxRetries} to get AI response for prompt: {prompt}");
                var response = await _aiService.GetAiResponseAsync(prompt);

                // Check if the response is retryable
                if (!IsRetryableError(response, retryableErrors))
                {
                    _log.Debug("Received a valid response or non-retryable error");
                    return response; // Return if response is valid or an error we shouldn’t retry
                }

                _log.Debug($"Received a retryable error {response}, retrying...");
                await Task.Delay(3000); // Wait 3 second before retrying to avoid overwhelming the service
            }

            _log.Error("Failed to get a valid response after maximum retries");
            return "Error: Failed to get a valid response from AI service after multiple attempts.";
        }

        private bool IsRetryableError(string response, string[] retryableErrors)
        {
            if (string.IsNullOrEmpty(response))
                return true; // Treat null or empty response as retryable

            foreach (var error in retryableErrors)
            {
                if (response.StartsWith(error))
                    return true; // Retry if response matches a retryable error
            }
            return false; // No retry if it’s a different error or valid response
        }
    }
}