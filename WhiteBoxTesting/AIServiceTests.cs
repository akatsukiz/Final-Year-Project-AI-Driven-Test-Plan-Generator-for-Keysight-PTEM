using Microsoft.VisualStudio.TestTools.UnitTesting;
using ChatboxPlugin.Service.AI;
using System.Reflection;
using System.Net.Http;
using Moq;
using Moq.Protected;
using System.Net;

namespace WhiteBoxTesting
{
    [TestClass]
    public class AIServiceTests
    {
        [TestMethod]
        public async Task GetAiResponseAsync_WithValidApiKey_ReturnsResponse()
        {
            // Arrange
            Environment.SetEnvironmentVariable("API_KEY", "test_api_key");
            
            // Create a mock HttpMessageHandler that returns a success response
            var handlerMock = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"{
                    ""candidates"": [
                        {
                            ""content"": {
                                ""parts"": [
                                    {
                                        ""text"": ""This is a test response from the AI.""
                                    }
                                ]
                            }
                        }
                    ]
                }")
            };

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Create a test instance of AIService with the mocked HttpClient
            var httpClient = new HttpClient(handlerMock.Object);
            var aiService = new AIService();
            
            // Use reflection to set the private _httpClient field with our mocked client
            typeof(AIService)
                .GetField("_httpClient", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(aiService, httpClient);

            // Act
            var result = await aiService.GetAiResponseAsync("Generate a test plan for voltage measurement");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("This is a test response from the AI.", result);
            
            // Verify that the HttpClient was called with the expected parameters
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().Contains("gemini-2.5-pro-exp-03-25") &&
                    req.RequestUri.ToString().Contains("key=test_api_key")
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [TestMethod]
        public async Task GetAiResponseAsync_WithInvalidApiKey_ReturnsErrorMessage()
        {
            // Arrange
            Environment.SetEnvironmentVariable("API_KEY", "invalid_key");
            
            // Create a mock HttpMessageHandler that returns an error response
            var handlerMock = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent(@"{
                    ""error"": {
                        ""code"": 401,
                        ""message"": ""API key not valid. Please pass a valid API key.""
                    }
                }")
            };

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Create a test instance of AIService with the mocked HttpClient
            var httpClient = new HttpClient(handlerMock.Object);
            var aiService = new AIService();
            
            // Use reflection to set the private _httpClient field with our mocked client
            typeof(AIService)
                .GetField("_httpClient", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(aiService, httpClient);

            // Act
            var result = await aiService.GetAiResponseAsync("Generate a test plan for voltage measurement");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.StartsWith("Error connecting to Google AI API:"));
        }
    }
} 