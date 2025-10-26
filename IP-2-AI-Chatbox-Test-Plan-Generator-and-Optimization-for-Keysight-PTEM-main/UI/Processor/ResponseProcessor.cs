using ChatboxPlugin.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenTap;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatboxPlugin.UI.Processor
{
    /// <summary>
    /// Processes and validates AI responses, extracts JSON, and formats it for display.
    /// </summary>
    public class ResponseProcessor
    {
        private readonly TraceSource _log = Log.CreateSource("ResponseProcessor");

        /// <summary>
        /// Extracts a valid JSON string from the AI response, handling various formats.
        /// Makes multiple attempts using different extraction strategies to find well-formed JSON.
        /// </summary>
        /// <param name="response">The raw response from the AI service</param>
        /// <returns>The extracted JSON string, or null if extraction failed</returns>
        public string ExtractJsonFromResponse(string response)
        {
            // Validate input
            if (string.IsNullOrEmpty(response))
            {
                _log.Error("AI response was null or empty");
                return null;
            }

            // Strategy 1: Extract from code blocks with triple backticks
            string extractedFromCodeBlock = ExtractFromCodeBlock(response);
            if (extractedFromCodeBlock != null)
            {
                _log.Debug("Successfully extracted JSON from code block");
                return extractedFromCodeBlock;
            }

            // Strategy 2: Try to locate a JSON object with "Steps" and "Explanation" keys
            string extractedFromKeys = ExtractByStepAndExplanationKeys(response);
            if (extractedFromKeys != null)
            {
                _log.Debug("Successfully extracted JSON by finding Steps and Explanation keys");
                return extractedFromKeys;
            }

            // Strategy 3: Last resort - look for the outermost braces
            string extractedFromBraces = ExtractByOutermostBraces(response);
            if (extractedFromBraces != null)
            {
                _log.Debug("Used fallback extraction using outermost braces");
                return extractedFromBraces;
            }

            _log.Error("Failed to extract JSON using any available method");
            return null;
        }

        /// <summary>
        /// Extracts JSON from a code block marked with triple backticks.
        /// </summary>
        /// <param name="response">The raw response text</param>
        /// <returns>The extracted JSON or null if not found</returns>
        private string ExtractFromCodeBlock(string response)
        {
            int startCode = response.IndexOf("```");
            if (startCode == -1)
                return null;

            startCode += 3; // Move past the opening ```

            // Skip language specifier if present (e.g., ```json)
            if (response.Substring(startCode).TrimStart().StartsWith("json", StringComparison.OrdinalIgnoreCase))
            {
                int newlineIndex = response.IndexOf('\n', startCode);
                if (newlineIndex != -1)
                    startCode = newlineIndex + 1;
            }

            int endCode = response.IndexOf("```", startCode);
            if (endCode == -1)
                return null;

            return response.Substring(startCode, endCode - startCode).Trim();
        }

        /// <summary>
        /// Extracts JSON by finding an object containing both "Steps" and "Explanation" keys.
        /// </summary>
        /// <param name="response">The raw response text</param>
        /// <returns>The extracted JSON or null if not found</returns>
        private string ExtractByStepAndExplanationKeys(string response)
        {
            int stepsIndex = response.IndexOf("\"Steps\"");
            int explanationIndex = response.IndexOf("\"Explanation\"");

            if (stepsIndex == -1 || explanationIndex == -1)
                return null;

            // Find the nearest opening brace before the first key
            int startIndex = Math.Min(stepsIndex, explanationIndex);
            while (startIndex > 0 && response[startIndex] != '{')
            {
                startIndex--;
            }

            if (startIndex < 0 || response[startIndex] != '{')
                return null;

            // Match braces to find the closing brace
            int braceCount = 1;
            int endIndex = startIndex + 1;

            while (endIndex < response.Length && braceCount > 0)
            {
                if (response[endIndex] == '{')
                    braceCount++;
                else if (response[endIndex] == '}')
                    braceCount--;
                
                endIndex++;
            }

            if (braceCount != 0)
                return null;

            return response.Substring(startIndex, endIndex - startIndex);
        }

        /// <summary>
        /// Extracts JSON by finding the outermost curly braces in the text.
        /// This is a fallback method when other strategies fail.
        /// </summary>
        /// <param name="response">The raw response text</param>
        /// <returns>The extracted JSON or null if not found</returns>
        private string ExtractByOutermostBraces(string response)
        {
            int firstBrace = response.IndexOf('{');
            int lastBrace = response.LastIndexOf('}');

            if (firstBrace == -1 || lastBrace <= firstBrace)
                return null;

            return response.Substring(firstBrace, lastBrace - firstBrace + 1);
        }

        /// <summary>
        /// Parses the AI's JSON response into a TestPlanJson object.
        /// </summary>
        public TestPlanJson ParseAiResponseJson(string jsonStr)
        {
            if (string.IsNullOrEmpty(jsonStr))
                return null;

            try
            {
                // Log the JSON for debugging
                _log.Debug("Attempting to parse JSON: " + jsonStr);

                // Parse using JObject
                JObject jsonObj = JObject.Parse(jsonStr);

                // Check if we have a Steps property
                if (jsonObj["Steps"] == null)
                {
                    _log.Error("JSON is missing Steps property");
                    return null;
                }

                // Process steps with special handling for TimeGuard child steps
                var steps = ProcessJsonSteps(jsonObj["Steps"]);

                // Create TestPlanJson with the processed Steps
                var testPlanJson = new TestPlanJson
                {
                    Steps = steps
                };

                // Add Explanation if it exists, or create default explanations
                testPlanJson.Explanation = jsonObj["Explanation"]?.ToObject<List<string>>() ??
                    CreateDefaultExplanations(steps);

                return testPlanJson;
            }
            catch (JsonException ex)
            {
                _log.Error("JSON parsing error: " + ex.Message);
                _log.Debug("Problematic JSON: " + jsonStr);
                return null;
            }
            catch (Exception ex)
            {
                _log.Error("General error parsing JSON: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Processes the JSON steps array and converts it to a list of TestStepJson objects.
        /// </summary>
        private List<TestStepJson> ProcessJsonSteps(JToken stepsToken)
        {
            var steps = new List<TestStepJson>();

            foreach (var stepToken in stepsToken)
            {
                var step = new TestStepJson
                {
                    StepOrder = stepToken["StepOrder"].Value<int>(),
                    StepType = stepToken["StepType"].Value<string>(),
                    Parameters = stepToken["Parameters"].ToObject<Dictionary<string, object>>()
                };

                // Special handling for TimeGuard ChildSteps
                if (step.StepType == "TimeGuard" && stepToken["ChildSteps"] != null)
                {
                    step.ChildSteps = ProcessJsonSteps(stepToken["ChildSteps"]);
                }

                steps.Add(step);
            }

            return steps;
        }

        /// <summary>
        /// Creates default explanations for steps when they are not provided in the AI response.
        /// </summary>
        private List<string> CreateDefaultExplanations(List<TestStepJson> steps)
        {
            var explanations = new List<string>
            {
                // First explanation is always a summary
                $"This test plan consists of {steps.Count} step(s) designed for the connected instrument."
            };

            // Add default explanations for each step
            foreach (var step in steps)
            {
                explanations.Add($"Step {step.StepOrder}: Executes a {step.StepType} operation.");
            }

            return explanations;
        }

        /// <summary>
        /// Ensures the TestPlanJson has a valid structure with all required properties.
        /// </summary>
        public bool ValidateTestPlanJson(TestPlanJson testPlanJson)
        {
            if (testPlanJson == null || testPlanJson.Steps == null || testPlanJson.Steps.Count == 0)
            {
                _log.Error("TestPlanJson is null or Steps collection is empty");
                return false;
            }

            try
            {
                // Check that all steps have required properties
                foreach (var step in testPlanJson.Steps)
                {
                    if (string.IsNullOrEmpty(step.StepType) || step.Parameters == null)
                    {
                        _log.Error($"Step missing required properties: StepType={step.StepType}, Parameters={step.Parameters != null}");
                        return false;
                    }

                    // Validate based on step type
                    if (!ValidateStepByType(step))
                    {
                        return false;
                    }

                    // For TimeGuard steps, also validate child steps
                    if (step.StepType == "TimeGuard" && (step.ChildSteps == null || step.ChildSteps.Count == 0))
                    {
                        _log.Error("TimeGuard step missing ChildSteps property or ChildSteps is empty");
                        return false;
                    }
                    else if (step.StepType == "TimeGuard")
                    {
                        foreach (var childStep in step.ChildSteps)
                        {
                            if (string.IsNullOrEmpty(childStep.StepType) || childStep.Parameters == null ||
                                !ValidateStepByType(childStep))
                            {
                                _log.Error("Child step has invalid structure");
                                return false;
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _log.Error($"Exception during validation: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Validates step parameters based on step type.
        /// </summary>
        private bool ValidateStepByType(TestStepJson step)
        {
            switch (step.StepType)
            {
                case "SCPI":
                    if (!step.Parameters.ContainsKey("Action") || !step.Parameters.ContainsKey("Query") || 
                        !step.Parameters.ContainsKey("Instrument"))
                    {
                        _log.Error($"SCPI step missing required parameters: Action={step.Parameters.ContainsKey("Action")}, " +
                                 $"Query={step.Parameters.ContainsKey("Query")}, " +
                                 $"Instrument={step.Parameters.ContainsKey("Instrument")}");
                        return false;
                    }
                    
                    // Optional RegularExpressionPattern consistency checks
                    if (step.Parameters.ContainsKey("RegularExpressionPattern"))
                    {
                        // If we have a pattern, we should also have verdict parameters
                        if (!step.Parameters.ContainsKey("VerdictOnMatch") || !step.Parameters.ContainsKey("VerdictOnNoMatch"))
                        {
                            _log.Error("SCPI step has RegularExpressionPattern but is missing verdict parameters");
                            // Don't return false, as these are optional - just log the warning
                        }
                    }
                    
                    // Optional ResultRegularExpressionPattern consistency checks
                    if (step.Parameters.ContainsKey("ResultRegularExpressionPattern"))
                    {
                        // If we have a result pattern, we should also have behavior
                        if (!step.Parameters.ContainsKey("Behavior"))
                        {
                            _log.Error("SCPI step has ResultRegularExpressionPattern but is missing Behavior parameter");
                            // Don't return false, as these are optional - just log the warning
                        }
                        
                        // If we have DimensionTitles, make sure we also have a result pattern and behavior
                        if (step.Parameters.ContainsKey("DimensionTitles") && !step.Parameters.ContainsKey("Behavior"))
                        {
                            _log.Error("SCPI step has DimensionTitles but is missing Behavior parameter");
                            // Don't return false, as these are optional - just log the warning
                        }
                    }
                    else if (step.Parameters.ContainsKey("DimensionTitles"))
                    {
                        _log.Error("SCPI step has DimensionTitles but is missing ResultRegularExpressionPattern parameter");
                        // Don't return false, as these are optional - just log the warning
                    }
                    break;
                case "Delay":
                    if (!step.Parameters.ContainsKey("DelaySecs"))
                    {
                        _log.Error("Delay step missing DelaySecs parameter");
                        return false;
                    }
                    break;
                case "TimeGuard":
                    if (!step.Parameters.ContainsKey("Timeout") ||
                        !step.Parameters.ContainsKey("StopOnTimeout") ||
                        !step.Parameters.ContainsKey("TimeoutVerdict"))
                    {
                        _log.Error($"TimeGuard step missing required parameters: Timeout={step.Parameters.ContainsKey("Timeout")}, " +
                                  $"StopOnTimeout={step.Parameters.ContainsKey("StopOnTimeout")}, " +
                                  $"TimeoutVerdict={step.Parameters.ContainsKey("TimeoutVerdict")}");
                        return false;
                    }
                    break;
                default:
                    _log.Error($"Unknown step type: {step.StepType}");
                    return false;
            }

            return true;
        }
    }
} 