using ChatboxPlugin.Model;
using ChatboxPlugin.Service.AI;
using ChatboxPlugin.Service.Manager;
using ChatboxPlugin.UI.Manager;
using ChatboxPlugin.UI.Processor;
using OpenTap;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ChatboxPlugin.UI
{
    /// <summary>
    /// Controls the core logic for chatbox interactions, processing user input and AI responses.
    /// </summary>
    public class ChatboxLogic
    {
        private readonly MessageManager _messageManager;
        private readonly TypingIndicatorManager _typingIndicatorManager;
        private readonly InputManager _inputManager;
        private readonly AiInteractor _aiInteractor;
        private readonly TestPlanManager _testPlanManager;
        private readonly LocalQueryHandler _localQueryHandler;
        private readonly ResponseProcessor _responseProcessor;
        private readonly ResponseFormatter _responseFormatter;
        private readonly InstrumentContextProvider _instrumentContextProvider;
        private readonly PromptBuilder _promptBuilder;
        
        private readonly TraceSource _log = Log.CreateSource("ChatboxController");

        /// <summary>
        /// Initializes a new instance of the ChatboxController class.
        /// </summary>
        /// <param name="messageManager">Manager for displaying messages in the UI.</param>
        /// <param name="typingIndicatorManager">Manager for the typing indicator.</param>
        /// <param name="inputManager">Manager for user input.</param>
        /// <param name="aiInteractor">Service for interacting with AI.</param>
        /// <param name="testPlanManager">Manager for test plan operations.</param>
        /// <param name="localQueryHandler">Handler for local queries.</param>
        public ChatboxLogic(
            MessageManager messageManager, 
            TypingIndicatorManager typingIndicatorManager,
            InputManager inputManager,
            AiInteractor aiInteractor,
            TestPlanManager testPlanManager,
            LocalQueryHandler localQueryHandler)
        {
            _messageManager = messageManager;
            _typingIndicatorManager = typingIndicatorManager;
            _inputManager = inputManager;
            _aiInteractor = aiInteractor;
            _testPlanManager = testPlanManager;
            _localQueryHandler = localQueryHandler;
            
            _responseProcessor = new ResponseProcessor();
            _responseFormatter = new ResponseFormatter();
            _instrumentContextProvider = new InstrumentContextProvider();
            _promptBuilder = new PromptBuilder();
        }

        /// <summary>
        /// Sends the user's message, processes it with AI if needed, and potentially generates or optimizes a test plan.
        /// Handles local commands, obtains instrument context, and updates the test plan based on AI response.
        /// </summary>
        /// <param name="query">The user's query text.</param>
        public async Task SendMessage(string query)
        {
            // Validate the query
            if (string.IsNullOrEmpty(query))
                return;

            // Display user message and clear input
            _messageManager.AddMessage(query, true);
            _inputManager.ClearAndFocus();

            // Check if this is a local command that doesn't need AI processing
            if (_localQueryHandler.HandleLocalQuery(query))
                return;

            // Show typing indicator while processing
            _typingIndicatorManager.ShowTypingIndicator();

            try
            {
                await ProcessQueryWithAI(query);
            }
            catch (Exception ex)
            {
                // Log and display any errors that occur during processing
                _messageManager.AddMessage($"Error: {ex.Message}", false);
                _log.Error($"Exception in SendMessage: {ex}");
            }
            finally
            {
                // Ensure typing indicator is removed whether successful or not
                _typingIndicatorManager.RemoveTypingIndicator();
                
                // Ensure textbox is fully reset after AI processing
                _inputManager.ResetAfterTypingIndicator();
            }
        }

        /// <summary>
        /// Processes a user query using AI to generate or modify a test plan.
        /// Obtains instrument information, test plan context, and handles the AI response.
        /// </summary>
        /// <param name="query">The user's query to process.</param>
        private async Task ProcessQueryWithAI(string query)
        {
            // Get instrument context and verify an instrument is defined
            string instrumentContext = _instrumentContextProvider.GetInstrumentContext();
            if (string.IsNullOrEmpty(instrumentContext))
            {
                _messageManager.AddMessage("Error: No instruments defined. Please add an instrument first.", false);
                return;
            }

            // Extract manufacturer and model information from instrument context
            var instrumentInfo = _instrumentContextProvider.ExtractInstrumentInfo(instrumentContext);
            string manufacturer = instrumentInfo.Item1;
            string model = instrumentInfo.Item2;

            // Verify instrument is connected and has valid identification
            if (manufacturer == "Unknown" && model == "Unknown")
            {
                _messageManager.AddMessage("Error: Instrument is not connected. Please connect to an instrument first.", false);
                return;
            }

            // Get or create a test plan context
            TestPlanContext testPlanContext = _testPlanManager.GetOrCreateTestPlanContext();
            if (testPlanContext.Error != null)
            {
                _messageManager.AddMessage(testPlanContext.Error, false);
                return;
            }

            // Serialise current test plan to JSON for context in the AI prompt
            string currentTestPlanJson = _testPlanManager.SerializeTestPlanToJson(testPlanContext.TestPlan);

            // Build prompt with all necessary context for the AI
            string aiPrompt = _promptBuilder.BuildAiPrompt(query, instrumentContext, currentTestPlanJson, manufacturer, model);
            _log.Debug($"AI prompt: {aiPrompt}");

            // Send prompt to AI and get response
            string aiResponse = await _aiInteractor.GetAiResponseAsync(aiPrompt);

            // Process the AI response and update test plan if valid
            ProcessAiResponse(aiResponse, testPlanContext, manufacturer, model);
        }

        /// <summary>
        /// Processes the AI response, extracts JSON, validates it, displays a formatted version,
        /// and updates the test plan if valid.
        /// </summary>
        /// <param name="aiResponse">The raw response from the AI service</param>
        /// <param name="testPlanContext">The test plan context to update</param>
        /// <param name="manufacturer">The instrument manufacturer name</param>
        /// <param name="model">The instrument model name</param>
        private void ProcessAiResponse(string aiResponse, TestPlanContext testPlanContext, string manufacturer, string model)
        {
            if (aiResponse.StartsWith("Error"))
            {
                _messageManager.AddMessage(aiResponse, false); // Display any error message
                return;
            }

            // Extract structured JSON from the AI's potentially mixed-format response
            string jsonStr = _responseProcessor.ExtractJsonFromResponse(aiResponse);
            if (jsonStr == null)
            {
                _messageManager.AddMessage("Error: Could not extract JSON from AI response.", false);
                _messageManager.AddMessage("Original response: " + aiResponse, false);
                return;
            }

            // Parse the JSON string into a structured TestPlanJson object
            TestPlanJson testPlanJson = _responseProcessor.ParseAiResponseJson(jsonStr);
            if (testPlanJson == null || !_responseProcessor.ValidateTestPlanJson(testPlanJson))
            {
                _messageManager.AddMessage("Error: Invalid JSON structure in AI response.", false);
                _messageManager.AddMessage("Raw JSON: " + jsonStr, false);
                return;
            }

            // Get all available instruments and create a lookup dictionary by name
            var instruments = InstrumentSettings.Current.OfType<ScpiInstrument>();
            var instrumentDictionary = instruments.ToDictionary(i => i.Name, i => i);
            
            // Ensure we have at least one instrument
            if (instrumentDictionary.Count == 0)
            {
                _messageManager.AddMessage("Error: No instruments found in settings.", false);
                return;
            }
            
            // Use the first available instrument as the default
            string defaultInstrumentName = instrumentDictionary.Keys.First();

            // Format the test plan in a user-friendly way and display it
            string displayMessage = _responseFormatter.FormatJsonDisplay(testPlanJson, manufacturer, model);
            _messageManager.AddMessage(displayMessage, false);

            // Update the actual test plan with the new steps from the AI
            _testPlanManager.CreateTestStepsFromJson(testPlanContext.TestPlan, testPlanJson.Steps, instrumentDictionary, defaultInstrumentName);

            // Save the updated test plan to disk
            testPlanContext.TestPlan.Save(testPlanContext.TestPlanPath);

            // Log the save path and notify the user of success
            _log.Info($"Test plan saved successfully to: {testPlanContext.TestPlanPath}");
            _messageManager.AddMessage("Test plan updated successfully, please reload the test plan.", false);
        }
    }
} 