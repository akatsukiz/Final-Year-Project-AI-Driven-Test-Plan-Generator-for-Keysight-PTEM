using OpenTap;
using OpenTap.Plugins.BasicSteps;
using System;
using System.Collections.Generic;

namespace ChatboxPlugin.Service.TestStep
{
    /// <summary>
    /// Custom SCPI step that extends SCPIRegexStep to log SCPI commands and queries to a CSV result listener.
    /// Logs essential details: step name, action, query, response, timestamp, and step duration.
    /// </summary>
    [Display("Custom SCPI Step", Groups: new[] { "Chatbox" })]
    public class CustomSCPIStep : SCPIRegexStep
    {
        private const string DefaultResultName = "SCPIResult";
        private const string NotApplicable = "N/A";

        /// <summary>
        /// A unique identifier for this step's results in the CSV output.
        /// </summary>
        [Display("Result Name", Description: "Name used to identify this step's results in the CSV output.", Order: 1)]
        public new string ResultName { get; set; }

        /// <summary>
        /// Initializes a new instance of the CustomSCPIStep class.
        /// </summary>
        public CustomSCPIStep()
        {
            ResultName = DefaultResultName;
        }

        /// <summary>
        /// Executes the SCPI command or query and logs the details to a CSV listener.
        /// Logs step name, action (Query/Command), query, response, timestamp, and step duration.
        /// Uses "N/A" for fields that are not applicable (e.g., response for a command).
        /// </summary>
        public override void Run()
        {
            try
            {
                // Capture the start time to calculate step duration
                DateTime startTime = DateTime.Now;

                // Execute the SCPI command or query using the base implementation
                base.Run();

                // Log the results to CSV
                LogResultsToCsv(startTime);
            }
            catch (Exception ex)
            {
                Log.Error($"Error executing SCPI step: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Logs the results of the SCPI operation to a CSV result listener.
        /// </summary>
        /// <param name="startTime">The time when the step execution started.</param>
        private void LogResultsToCsv(DateTime startTime)
        {
            // Determine the action type (Query or Command)
            string action = Action == SCPIAction.Query ? "Query" : "Command";

            // Get the query/command string
            string query = Query;

            // Get the response (only for queries, otherwise "N/A")
            string response = Action == SCPIAction.Query ? Response : NotApplicable;

            // Use "N/A" if the response is empty or null (for queries)
            if (string.IsNullOrEmpty(response) && Action == SCPIAction.Query)
            {
                response = NotApplicable;
            }

            // Prepare data for CSV output
            var actions = new string[] { action };
            var queries = new string[] { query };
            var responses = new string[] { response };
            var timestamps = new DateTime[] { startTime };

            // Publish the log data to the CSV listener
            Results.PublishTable(
                ResultName,
                new List<string> { "Action", "Query", "Response", "Timestamp" },
                actions, queries, responses, timestamps
            );
        }
    }
}