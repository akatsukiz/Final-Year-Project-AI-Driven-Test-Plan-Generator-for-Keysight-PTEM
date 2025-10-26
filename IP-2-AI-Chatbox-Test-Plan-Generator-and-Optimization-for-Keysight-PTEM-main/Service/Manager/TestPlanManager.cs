using ChatboxPlugin.Model;
using Keysight.OpenTap.Wpf;
using OpenTap;
using OpenTap.Plugins.BasicSteps;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ChatboxPlugin.Service.Utility;

namespace ChatboxPlugin.Service.Manager
{
    /// <summary>
    /// Manages test plan operations including creating, serialising, and deserialising test plans.
    /// </summary>
    public class TestPlanManager
    {
        private readonly ITapDockContext _dockContext;
        private readonly TraceSource _log = Log.CreateSource("TestPlanManager");

        /// <summary>
        /// Initializes a new instance of the TestPlanManager class.
        /// </summary>
        /// <param name="dockContext">The TAP dock context.</param>
        /// <param name="log">The logging source for debugging.</param>
        public TestPlanManager(ITapDockContext dockContext)
        {
            _dockContext = dockContext;
        }

        /// <summary>
        /// Gets or creates a test plan context, including SCPI, Delay, and TimeGuard steps.
        /// </summary>
        /// <returns>A populated TestPlanContext object.</returns>
        public TestPlanContext GetOrCreateTestPlanContext()
        {
            var context = new TestPlanContext();
            try
            {
                string testPlanPath = _dockContext?.Plan?.Path;
                context.TestPlanPath = testPlanPath;
                _log.Debug($"Test plan path from dock context: {context.TestPlanPath}");
                
                if (string.IsNullOrEmpty(context.TestPlanPath))
                {
                    _log.Info("No test plan path available from dock context");
                    context.TestPlan = new TestPlan();
                    context.TestPlanPath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        $"{context.TestPlan.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.TapPlan");
                    context.TestPlan.Save(context.TestPlanPath);
                    _log.Info($"Created new test plan to be saved at: {context.TestPlanPath}");
                }
                else
                {
                    _log.Debug($"Loading test plan from: {testPlanPath}");
                    context.TestPlan = TestPlan.Load(testPlanPath);
                    if (context.TestPlan == null)
                    {
                        context.Error = "Error: Failed to load test plan.";
                        return context;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Error in GetOrCreateTestPlanContext: {ex.Message}");
                context.Error = $"Error: Failed to retrieve test plan: {ex.Message}";
            }
            return context;
        }

        /// <summary>
        /// Serializes the current test plan to JSON format for the AI.
        /// </summary>
        /// <param name="testPlan">The test plan to serialise.</param>
        /// <returns>A JSON string representation of the test plan.</returns>
        public string SerializeTestPlanToJson(TestPlan testPlan)
        {
            var steps = SerializeSteps(testPlan.ChildTestSteps);
            var testPlanJson = new TestPlanJson
            {
                Steps = steps,
                Explanation = new List<string>() // Empty for current plan
            };
            return JsonConvert.SerializeObject(testPlanJson, Formatting.Indented);
        }

        /// <summary>
        /// Recursively serializes test steps into JSON format.
        /// </summary>
        /// <param name="steps">The list of test steps to serialise.</param>
        /// <returns>A list of serialised test steps in JSON format.</returns>
        private List<TestStepJson> SerializeSteps(TestStepList steps)
        {
            var stepList = new List<TestStepJson>();
            int order = 1;
            
            foreach (var step in steps)
            {
                var stepJson = new TestStepJson
                {
                    StepOrder = order++,
                    StepType = GetStepType(step),
                    Parameters = GetStepParameters(step),
                    ChildSteps = step is TimeGuardStep timeGuardStep ? 
                                SerializeSteps(timeGuardStep.ChildTestSteps) : null
                };
                stepList.Add(stepJson);
            }
            return stepList;
        }

        /// <summary>
        /// Determines the step type as a string.
        /// </summary>
        /// <param name="step">The test step to categorise.</param>
        /// <returns>A string representation of the step type.</returns>
        private string GetStepType(ITestStep step)
        {
            if (step is SCPIRegexStep) return "SCPI";
            if (step is DelayStep) return "Delay";
            if (step is TimeGuardStep) return "TimeGuard";
            return "Unknown";
        }

        /// <summary>
        /// Extracts parameters for a given step type.
        /// </summary>
        /// <param name="step">The test step to extract parameters from.</param>
        /// <returns>A dictionary containing the step's parameters.</returns>
        private Dictionary<string, object> GetStepParameters(ITestStep step)
        {
            if (step is SCPIRegexStep scpiStep)
            {
                var parameters = new Dictionary<string, object>
                {
                    { "Action", scpiStep.Action.ToString() },
                    { "Query", scpiStep.Query }
                };
                
                // Add instrument name if available
                if (scpiStep.Instrument != null)
                {
                    parameters.Add("Instrument", scpiStep.Instrument.Name);
                }
                
                // Add verdict and regular expression pattern parameters if available
                if (scpiStep.RegularExpressionPattern.IsEnabled)
                {
                    parameters.Add("RegularExpressionPattern", scpiStep.RegularExpressionPattern.Value);
                    parameters.Add("VerdictOnMatch", scpiStep.VerdictOnMatch.ToString());
                    parameters.Add("VerdictOnNoMatch", scpiStep.VerdictOnNoMatch.ToString());
                }
                
                // Add result pattern and behavior parameters if available
                if (scpiStep.ResultRegularExpressionPattern.IsEnabled)
                {
                    parameters.Add("ResultRegularExpressionPattern", scpiStep.ResultRegularExpressionPattern.Value);
                    parameters.Add("Behavior", scpiStep.Behavior.ToString());
                    
                    // Add DimensionTitles if not empty
                    if (!string.IsNullOrEmpty(scpiStep.DimensionTitles))
                    {
                        parameters.Add("DimensionTitles", scpiStep.DimensionTitles);
                    }
                }
                
                return parameters;
            }
            else if (step is DelayStep delayStep)
            {
                return new Dictionary<string, object>
                {
                    { "DelaySecs", delayStep.DelaySecs }
                };
            }
            else if (step is TimeGuardStep timeGuardStep)
            {
                return new Dictionary<string, object>
                {
                    { "Timeout", timeGuardStep.Timeout },
                    { "StopOnTimeout", timeGuardStep.StopOnTimeout },
                    { "TimeoutVerdict", timeGuardStep.TimeoutVerdict.ToString() }
                };
            }
            return new Dictionary<string, object>();
        }

        /// <summary>
        /// Creates test steps from the AI's JSON response and updates the test plan.
        /// </summary>
        /// <param name="testPlan">The test plan to update.</param>
        /// <param name="steps">The list of test steps in JSON format.</param>
        /// <param name="instrumentDictionary">Dictionary of available instruments by name.</param>
        /// <param name="defaultInstrumentName">Default instrument name to use when no specific instrument is specified.</param>
        public void CreateTestStepsFromJson(TestPlan testPlan, List<TestStepJson> steps, 
            Dictionary<string, ScpiInstrument> instrumentDictionary, string defaultInstrumentName)
        {
            testPlan.ChildTestSteps.Clear(); // Replace existing steps
            
            foreach (var stepJson in steps.OrderBy(s => s.StepOrder))
            {
                ITestStep step = CreateStepFromJson(stepJson, instrumentDictionary, defaultInstrumentName);
                if (step != null)
                {
                    testPlan.ChildTestSteps.Add(step);
                }
            }
        }

        /// <summary>
        /// Creates a single test step from a JSON object, handling recursion for TimeGuard.
        /// </summary>
        /// <param name="stepJson">The JSON representation of a test step.</param>
        /// <param name="instrumentDictionary">Dictionary of available instruments by name.</param>
        /// <param name="defaultInstrumentName">Default instrument name to use when no specific instrument is specified.</param>
        /// <returns>A created test step instance.</returns>
        private ITestStep CreateStepFromJson(TestStepJson stepJson, Dictionary<string, ScpiInstrument> instrumentDictionary, string defaultInstrumentName)
        {
            switch (stepJson.StepType)
            {
                case "SCPI":
                    return CreateScpiStep(stepJson.Parameters, instrumentDictionary, defaultInstrumentName);
                case "Delay":
                    return CreateDelayStep(stepJson.Parameters);
                case "TimeGuard":
                    return CreateTimeGuardStep(stepJson.Parameters, stepJson.ChildSteps, instrumentDictionary, defaultInstrumentName);
                default:
                    _log.Warning($"Unknown StepType: {stepJson.StepType}");
                    return null;
            }
        }

        /// <summary>
        /// Creates an SCPI step from JSON parameters.
        /// </summary>
        /// <param name="parameters">Dictionary of step parameters.</param>
        /// <param name="instrumentDictionary">Dictionary of available instruments by name.</param>
        /// <param name="defaultInstrumentName">Default instrument name to use when no specific instrument is specified.</param>
        /// <returns>A configured SCPI test step.</returns>
        private ITestStep CreateScpiStep(Dictionary<string, object> parameters, Dictionary<string, ScpiInstrument> instrumentDictionary, string defaultInstrumentName)
        {
            var scpiStep = new TestStep.CustomSCPIStep();
            
            if (parameters.TryGetValue("Action", out object actionObj) && actionObj is string actionStr)
            {
                scpiStep.Action = (SCPIAction)Enum.Parse(typeof(SCPIAction), actionStr);
            }
            
            if (parameters.TryGetValue("Query", out object queryObj) && queryObj is string queryStr)
            {
                scpiStep.Query = ChatboxUtilities.SanitizeScpiCommand(queryStr);
            }
            
            // Set regular expression pattern and verdict parameters if provided
            if (parameters.TryGetValue("RegularExpressionPattern", out object regexObj))
            {
                string regexStr = null;
                if (regexObj is string str)
                    regexStr = str;
                else if (regexObj is JValue jValue && jValue.Type == JTokenType.String)
                    regexStr = jValue.Value<string>();
                
                if (!string.IsNullOrEmpty(regexStr))
                {
                    scpiStep.RegularExpressionPattern.Value = regexStr;
                    scpiStep.RegularExpressionPattern.IsEnabled = true;
                    
                    // Set verdict on match if provided
                    if (parameters.TryGetValue("VerdictOnMatch", out object matchVerdictObj))
                    {
                        string matchVerdictStr = null;
                        if (matchVerdictObj is string str2)
                            matchVerdictStr = str2;
                        else if (matchVerdictObj is JValue jValue2 && jValue2.Type == JTokenType.String)
                            matchVerdictStr = jValue2.Value<string>();
                        
                        if (!string.IsNullOrEmpty(matchVerdictStr))
                            scpiStep.VerdictOnMatch = (Verdict)Enum.Parse(typeof(Verdict), matchVerdictStr);
                    }
                    
                    // Set verdict on no match if provided
                    if (parameters.TryGetValue("VerdictOnNoMatch", out object noMatchVerdictObj))
                    {
                        string noMatchVerdictStr = null;
                        if (noMatchVerdictObj is string str3)
                            noMatchVerdictStr = str3;
                        else if (noMatchVerdictObj is JValue jValue3 && jValue3.Type == JTokenType.String)
                            noMatchVerdictStr = jValue3.Value<string>();
                        
                        if (!string.IsNullOrEmpty(noMatchVerdictStr))
                            scpiStep.VerdictOnNoMatch = (Verdict)Enum.Parse(typeof(Verdict), noMatchVerdictStr);
                    }
                }
            }
            
            // Set result regular expression pattern and behavior if provided
            if (parameters.TryGetValue("ResultRegularExpressionPattern", out object resultRegexObj))
            {
                string resultRegexStr = null;
                if (resultRegexObj is string str)
                    resultRegexStr = str;
                else if (resultRegexObj is JValue jValue && jValue.Type == JTokenType.String)
                    resultRegexStr = jValue.Value<string>();
                
                if (!string.IsNullOrEmpty(resultRegexStr))
                {
                    scpiStep.ResultRegularExpressionPattern.Value = resultRegexStr;
                    scpiStep.ResultRegularExpressionPattern.IsEnabled = true;
                    
                    // Set behavior if provided
                    if (parameters.TryGetValue("Behavior", out object behaviorObj))
                    {
                        string behaviorStr = null;
                        if (behaviorObj is string str2)
                            behaviorStr = str2;
                        else if (behaviorObj is JValue jValue2 && jValue2.Type == JTokenType.String)
                            behaviorStr = jValue2.Value<string>();
                        
                        if (!string.IsNullOrEmpty(behaviorStr))
                            scpiStep.Behavior = (SCPIRegexBehavior)Enum.Parse(typeof(SCPIRegexBehavior), behaviorStr);
                    }
                    
                    // Set dimension titles if provided
                    if (parameters.TryGetValue("DimensionTitles", out object dimensionTitlesObj))
                    {
                        string dimensionTitlesStr = null;
                        if (dimensionTitlesObj is string str3)
                            dimensionTitlesStr = str3;
                        else if (dimensionTitlesObj is JValue jValue3 && jValue3.Type == JTokenType.String)
                            dimensionTitlesStr = jValue3.Value<string>();
                        
                        if (!string.IsNullOrEmpty(dimensionTitlesStr))
                            scpiStep.DimensionTitles = dimensionTitlesStr;
                    }
                }
            }
            
            scpiStep.Name = $"SCPI {scpiStep.Action}: {(scpiStep.Query.Length > 20 ? scpiStep.Query.Substring(0, 20) + "..." : scpiStep.Query)}";
            
            // Get the instrument name from the parameters or use the default
            if (parameters.TryGetValue("Instrument", out object instrumentObj) && instrumentObj is string instrumentName && 
                !string.IsNullOrEmpty(instrumentName) && instrumentDictionary.TryGetValue(instrumentName, out var specifiedInstrument))
            {
                scpiStep.Instrument = specifiedInstrument;
                _log.Debug($"Using specified instrument: {instrumentName} for SCPI step: {scpiStep.Name}");
            }
            else
            {
                // Fallback to default instrument
                scpiStep.Instrument = instrumentDictionary.TryGetValue(defaultInstrumentName, out var defaultInstrument) ? 
                    defaultInstrument : null;
                
                if (scpiStep.Instrument == null)
                {
                    _log.Error($"Both specified instrument and default instrument not found for SCPI step: {scpiStep.Name}");
                }
                else
                {
                    _log.Debug($"Using default instrument: {defaultInstrumentName} for SCPI step: {scpiStep.Name}");
                }
            }

            return scpiStep;
        }

        /// <summary>
        /// Creates a Delay step from JSON parameters.
        /// </summary>
        /// <param name="parameters">Dictionary of step parameters.</param>
        /// <returns>A configured Delay test step.</returns>
        private ITestStep CreateDelayStep(Dictionary<string, object> parameters)
        {
            var delayStep = new DelayStep();
            
            if (parameters.TryGetValue("DelaySecs", out object delayObj))
            {
                // Handle both integer and double values
                if (delayObj is double d) delayStep.DelaySecs = d;
                else if (delayObj is long l) delayStep.DelaySecs = l;
            }
            
            delayStep.Name = $"Delay {delayStep.DelaySecs} seconds";
            
            return delayStep;
        }

        /// <summary>
        /// Creates a TimeGuard step from JSON parameters and its child steps.
        /// </summary>
        /// <param name="parameters">Dictionary of step parameters.</param>
        /// <param name="childSteps">List of child steps within the TimeGuard.</param>
        /// <param name="instrumentDictionary">Dictionary of available instruments by name.</param>
        /// <param name="defaultInstrumentName">Default instrument name to use when no specific instrument is specified.</param>
        /// <returns>A configured TimeGuard test step with child steps.</returns>
        private ITestStep CreateTimeGuardStep(Dictionary<string, object> parameters, List<TestStepJson> childSteps, Dictionary<string, ScpiInstrument> instrumentDictionary, string defaultInstrumentName)
        {
            var timeGuardStep = new TimeGuardStep();
            
            if (parameters.TryGetValue("Timeout", out object timeoutObj))
            {
                if (timeoutObj is double d) 
                    timeGuardStep.Timeout = d;
                else if (timeoutObj is long l) 
                    timeGuardStep.Timeout = l;
                else if (timeoutObj is JValue jValue && jValue.Type == JTokenType.Float) 
                    timeGuardStep.Timeout = jValue.Value<double>();
                else if (timeoutObj is JValue jIntValue && jIntValue.Type == JTokenType.Integer) 
                    timeGuardStep.Timeout = jIntValue.Value<double>();
            }
            
            if (parameters.TryGetValue("StopOnTimeout", out object stopObj))
            {
                if (stopObj is bool stop) 
                    timeGuardStep.StopOnTimeout = stop;
                else if (stopObj is JValue jValue && jValue.Type == JTokenType.Boolean) 
                    timeGuardStep.StopOnTimeout = jValue.Value<bool>();
            }
            
            if (parameters.TryGetValue("TimeoutVerdict", out object verdictObj))
            {
                if (verdictObj is string verdictStr) 
                    timeGuardStep.TimeoutVerdict = (Verdict)Enum.Parse(typeof(Verdict), verdictStr);
                else if (verdictObj is JValue jValue && jValue.Type == JTokenType.String)
                    timeGuardStep.TimeoutVerdict = (Verdict)Enum.Parse(typeof(Verdict), jValue.Value<string>());
            }
            
            timeGuardStep.Name = $"Time Guard ({timeGuardStep.Timeout}s)";

            // Recursively add child steps
            if (childSteps != null)
            {
                foreach (var childJson in childSteps.OrderBy(s => s.StepOrder))
                {
                    ITestStep childStep = CreateStepFromJson(childJson, instrumentDictionary, defaultInstrumentName);
                    if (childStep != null)
                    {
                        timeGuardStep.ChildTestSteps.Add(childStep);
                    }
                }
            }
            
            return timeGuardStep;
        }
    }
}