namespace ChatboxPlugin.UI.Processor
{
    /// <summary>
    /// Builds prompts for AI interaction based on user queries and context.
    /// </summary>
    public class PromptBuilder
    {
        /// <summary>
        /// Builds a comprehensive prompt for the AI with all necessary context and instructions.
        /// Includes detailed formatting requirements for the AI's JSON response.
        /// </summary>
        /// <param name="query">The user's original query</param>
        /// <param name="instrumentContext">Information about the available instruments</param>
        /// <param name="currentTestPlanJson">The current test plan in JSON format, if any</param>
        /// <param name="manufacturer">The instrument manufacturer name</param>
        /// <param name="model">The instrument model name</param>
        /// <returns>A formatted prompt string ready to send to the AI</returns>
        public string BuildAiPrompt(string query, string instrumentContext, string currentTestPlanJson,
                                   string manufacturer, string model)
        {
            return $@"
                INSTRUCTIONS TO THE AI:
                You are to generate a complete test plan in JSON format based on the user's query and the current test plan (if any). The test plan must include SCPI, Delay, and TimeGuard steps ONLY, with NO OTHER STEP TYPES. Follow these rules exactly:
                1. The output must be a JSON object containing two keys at the top level:
                   - ""Steps"": an array of test step objects.
                   - ""Explanation"": an array of human-readable explanation strings corresponding to each step.
                2. Each test step object in ""Steps"" must have the following properties:
                   - ""StepOrder"": an integer indicating the order (starting at 1).
                   - ""StepType"": a string indicating the type of the step (ONLY ""SCPI"", ""Delay"", or ""TimeGuard"" are valid).
                   - ""Parameters"": an object containing key-value pairs relevant to that step.
                   - For TimeGuard steps only: also include a ""ChildSteps"" property which is an array of nested test steps.
   
                   Notes for specific steps:
                   - For SCPI steps: Each SCPI command must be in its own step. The ""Parameters"" object must include:
                         ""Action"": either ""Query"" or ""Command"",
                         ""Query"": the SCPI command text (e.g., ""*IDN?""),
                         ""Instrument"": string - the name of the instrument to use for this step.
                         Optional parameters:
                         ""RegularExpressionPattern"": string for pattern matching (e.g., ""^(20[0-9]|290)$"") - only add if response validation is needed,
                         ""VerdictOnMatch"": string (""Pass"", ""Inconclusive"", ""Fail"", ""Aborted"", ""Error"") - used to set test verdict when pattern matches,
                         ""VerdictOnNoMatch"": string (""Pass"", ""Inconclusive"", ""Fail"", ""Aborted"", ""Error"") - used to set test verdict when pattern doesn't match,
                         ""ResultRegularExpressionPattern"": string for capturing results (e.g., ""(.*)"") - used to extract specific data from the response,
                         ""Behavior"": string (""GroupsAsDimensions"" or ""GroupsAsResults"") - determines how captured groups are processed,
                         ""DimensionTitles"": string - titles for result dimensions, comma-separated if multiple dimensions.
                   - For Delay steps: The ""Parameters"" object must include:
                         ""DelaySecs"": a numeric value (e.g., 0.1 for 100ms).
                   - For TimeGuard steps: The ""Parameters"" object must include:
                         ""Timeout"": numeric value in seconds (e.g., 30),
                         ""StopOnTimeout"": boolean (true/false),
                         ""TimeoutVerdict"": string (""Pass"", ""Inconclusive"", ""Fail"", ""Aborted"", ""Error"").
                         IMPORTANT: TimeGuard is ONLY used to set a time limit for the execution of its child steps and fail if exceeded - it does NOT repeat the child steps.
                3. The ""Explanation"" array must have the following structure:
                   - The first element (index 0) MUST provide context about the test plan, mentioning all the instruments used in the test plan (manufacturers and models),
                     the identified user's intent, and should use the format: ""Using [manufacturers and models of all instruments] to [user's intent] the test plan is configured as:""
                   - All subsequent elements (index 1 and onwards) must explain each corresponding test step in plain language.
                4. Do not include any additional keys or commentary outside the JSON structure.
                5. Ensure that:
                   - Every SCPI command is output as a separate step.
                   - Add Delay or TimeGuard steps as if needed for a complete, robust test plan based on the query.
                   - CRITICAL: There is NO ""Repeat"" or ""Loop"" step type available. There is NO way to create loops or iterations programmatically.
                   - CRITICAL: TimeGuard does NOT repeat its child steps - it only sets a time limit for their execution.
                   - For repetitive actions (like taking multiple measurements over time), you MUST explicitly define each individual step in sequence.
                   - The structure may vary depending on the query's intent.
                   - You can use different instruments for different SCPI steps by specifying the appropriate instrument name in the ""Instrument"" parameter.
                   - If entire test plan is only using 1 instrument, you can mention the instrument manufacturer and model (do not mention instrument name) once in the first explanation and omit it in subsequent explanations.
                   - CRITICAL: For SCPI steps, only include the RegularExpressionPattern, VerdictOnMatch, VerdictOnNoMatch, ResultRegularExpressionPattern, and Behavior parameters when specifically requested in the user query. Otherwise, omit these optional parameters.
                   - The first explanation provides context, and subsequent explanations detail individual steps, the child step explanation should be together with the parent step explanation, keep the explanation concise and clear, the writing style should vary to feels human.
                
                IMPORTANT FOR DATA LOGGING AND REPETITIVE ACTIONS:
                - CRITICAL: This system has NO repeat, loop, or iteration functionality!
                - For data logging or taking multiple measurements over time, you MUST explicitly define each measurement step in sequence.
                - For example, to log 10 measurements with 1-second delays between them, you would need to create 20 steps in total:
                  10 SCPI Query steps (one for each measurement) alternating with 10 Delay steps (one after each measurement).
                - You can use a TimeGuard as a parent step to set a timeout for a single measurement operation, but it will NOT repeat the child steps.
                - TimeGuard is ONLY for setting time limits on operations, NOT for repetition.
                - If the user requests a very large number of repetitions (e.g., hundreds of measurements), create a reasonable subset (10-20) with appropriate explanation.
                
                Use this format to generate the test plan:
                EXAMPLE OUTPUT FORMAT FOR MULTIPLE MEASUREMENTS:
                {{
                  ""Steps"": [
                    {{
                      ""StepOrder"": 1,
                      ""StepType"": ""SCPI"",
                      ""Parameters"": {{
                         ""Action"": ""Command"",
                         ""Query"": ""*RST"",
                         ""Instrument"": ""SCPI1""
                      }}
                    }},
                    {{
                      ""StepOrder"": 2,
                      ""StepType"": ""SCPI"",
                      ""Parameters"": {{
                         ""Action"": ""Command"",
                         ""Query"": ""CONF:VOLT:DC"",
                         ""Instrument"": ""SCPI1""
                      }}
                    }},
                    {{
                      ""StepOrder"": 3,
                      ""StepType"": ""TimeGuard"",
                      ""Parameters"": {{
                         ""Timeout"": 10,
                         ""StopOnTimeout"": true,
                         ""TimeoutVerdict"": ""Fail""
                      }},
                      ""ChildSteps"": [
                          {{
                            ""StepOrder"": 1,
                            ""StepType"": ""SCPI"",
                            ""Parameters"": {{
                                ""Action"": ""Query"",
                                ""Query"": ""MEAS:VOLT:DC?"",
                                ""Instrument"": ""SCPI1"",
                                ""ResultRegularExpressionPattern"": ""([+-]?\\d+\\.\\d+E[+-]\\d+)"",
                                ""Behavior"": ""GroupsAsDimensions"",
                                ""DimensionTitles"": ""Voltage""
                            }}
                          }}
                      ]
                    }},
                    {{
                      ""StepOrder"": 4,
                      ""StepType"": ""Delay"",
                      ""Parameters"": {{
                          ""DelaySecs"": 1.0
                      }}
                    }},
                    {{
                      ""StepOrder"": 5,
                      ""StepType"": ""TimeGuard"",
                      ""Parameters"": {{
                         ""Timeout"": 10,
                         ""StopOnTimeout"": true,
                         ""TimeoutVerdict"": ""Fail""
                      }},
                      ""ChildSteps"": [
                          {{
                            ""StepOrder"": 1,
                            ""StepType"": ""SCPI"",
                            ""Parameters"": {{
                                ""Action"": ""Query"",
                                ""Query"": ""MEAS:VOLT:DC?"",
                                ""Instrument"": ""SCPI1"",
                                ""ResultRegularExpressionPattern"": ""([+-]?\\d+\\.\\d+E[+-]\\d+)"",
                                ""Behavior"": ""GroupsAsDimensions"",
                                ""DimensionTitles"": ""Voltage""
                            }}
                          }}
                      ]
                    }},
                    {{
                      ""StepOrder"": 6,
                      ""StepType"": ""Delay"",
                      ""Parameters"": {{
                          ""DelaySecs"": 1.0
                      }}
                    }}
                    // Additional similar steps would continue for more measurements...
                  ],
                  ""Explanation"": [
                      ""Using Keysight 34465A Digital Multimeter to log voltage measurements over a period, the test plan is configured as:"",
                      ""Resets the multimeter to its default state."",
                      ""Configures the multimeter for DC voltage measurements."",
                      ""Reads the first DC voltage measurement with a 10-second timeout, capturing the numerical value as 'Voltage'."",
                      ""Waits for 1 second between measurements."",
                      ""Reads the second DC voltage measurement with a 10-second timeout, capturing the numerical value as 'Voltage'."",
                      ""Waits for 1 second between measurements.""
                      // Further explanations would continue for additional steps...
                  ]
                }}
                Now, using the following context, manufacturer, model, current test plan (if any), and user query, generate your complete test plan:
                Instrument: {instrumentContext}
                {(string.IsNullOrEmpty(currentTestPlanJson) ? "" : "Current Test Plan JSON:\n" + currentTestPlanJson + "\n")}
                User Query: {query}
                Please provide your response wrapped in triple backticks (```) containing only the JSON object with 'Steps' and 'Explanation' keys, with no additional text outside the backticks.
                {{
                ""Steps"": [...],
                ""Explanation"": [...]
                }}
                ";
        }
    }
} 