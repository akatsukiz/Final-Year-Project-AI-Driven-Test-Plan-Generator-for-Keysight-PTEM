using ChatboxPlugin.Model;
using System.Text;

namespace ChatboxPlugin.UI.Processor
{
    /// <summary>
    /// Formats AI responses for display in the UI.
    /// </summary>
    public class ResponseFormatter
    {
        /// <summary>
        /// Formats the extracted JSON into a single, user-friendly message with explanations and steps.
        /// </summary>
        /// <param name="testPlanJson">The test plan JSON to format</param>
        /// <param name="manufacturer">The instrument manufacturer name</param>
        /// <param name="model">The instrument model name</param>
        /// <returns>A formatted string for display in the UI</returns>
        public string FormatJsonDisplay(TestPlanJson testPlanJson, string manufacturer, string model)
        {
            var message = new StringBuilder();

            // First explanation provides context (should include manufacturer and model)
            if (testPlanJson.Explanation.Count > 0)
            {
                message.AppendLine(testPlanJson.Explanation[0]);
            }
            else
            {
                message.AppendLine($"This test plan consists of {testPlanJson.Steps.Count} step(s) designed for a {manufacturer} {model}.");
            }

            message.AppendLine(); // Blank line for readability

            // Iterate through steps and format each one
            for (int i = 0; i < testPlanJson.Steps.Count; i++)
            {
                var step = testPlanJson.Steps[i];

                // Fetch explanation if available or use a fallback message
                string explanation = i + 1 < testPlanJson.Explanation.Count
                    ? testPlanJson.Explanation[i + 1]
                    : $"Performs a {step.StepType} operation.";

                // Step header
                message.AppendLine($"Step {step.StepOrder}: {explanation}");
                message.AppendLine(); // Add a blank line between explanation and step details
                message.AppendLine($"Operation: {step.StepType}");
                message.AppendLine(new string('-', 40)); // Delimiter line

                // List parameters with indentation
                foreach (var param in step.Parameters)
                {
                    message.AppendLine($"    {param.Key}: {param.Value}");
                }

                // Format and display nested ChildSteps for TimeGuard steps
                FormatChildSteps(message, step);

                message.AppendLine(new string('-', 40)); // End delimiter for the step
                message.AppendLine(); // Add a blank line between steps
            }

            return message.ToString().TrimEnd(); // Remove trailing whitespace
        }

        /// <summary>
        /// Formats child steps for display in the message.
        /// </summary>
        /// <param name="message">The StringBuilder to append to</param>
        /// <param name="step">The parent step containing child steps</param>
        private void FormatChildSteps(StringBuilder message, TestStepJson step)
        {
            if (step.ChildSteps != null && step.ChildSteps.Count > 0)
            {
                message.AppendLine("    Nested Operations:");
                foreach (var childStep in step.ChildSteps)
                {
                    message.AppendLine($"      {childStep.StepType}:");
                    message.AppendLine("      " + new string('-', 30)); // Nested delimiter line
                    foreach (var param in childStep.Parameters)
                    {
                            message.AppendLine($"         {param.Key}: {param.Value}");
                    }
                    message.AppendLine("      " + new string('-', 30)); // End nested delimiter line
                }
            }
        }
    }
} 