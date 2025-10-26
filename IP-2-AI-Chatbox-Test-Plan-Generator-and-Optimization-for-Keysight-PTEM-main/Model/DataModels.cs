using OpenTap;
using OpenTap.Plugins.BasicSteps;
using System.Collections.Generic;

namespace ChatboxPlugin.Model
{
    /// <summary>
    /// Contains context information about the test plan and its steps.
    /// </summary>
    public class TestPlanContext
    {
        /// <summary>
        /// Gets or sets the test plan.
        /// </summary>
        public TestPlan TestPlan { get; set; }

        /// <summary>
        /// Gets or sets the current SCPI step being processed.
        /// </summary>
        public SCPIRegexStep ScpiStep { get; set; }

        /// <summary>
        /// Gets or sets error messages during test plan processing.
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// Gets or sets the file path of the test plan.
        /// </summary>
        public string TestPlanPath { get; set; }
    }

    /// <summary>
    /// Represents a test plan in JSON format as expected from the AI.
    /// </summary>
    public class TestPlanJson
    {
        /// <summary>
        /// Gets or sets the list of test steps.
        /// </summary>
        public List<TestStepJson> Steps { get; set; }

        /// <summary>
        /// Gets or sets explanations for the test plan steps.
        /// </summary>
        public List<string> Explanation { get; set; }
    }

    /// <summary>
    /// Represents a single test step in JSON format.
    /// </summary>
    public class TestStepJson
    {
        /// <summary>
        /// Gets or sets the execution order of this step.
        /// </summary>
        public int StepOrder { get; set; }

        /// <summary>
        /// Gets or sets the type of this step (e.g., SCPI, Delay, TimeGuard).
        /// </summary>
        public string StepType { get; set; }

        /// <summary>
        /// Gets or sets the parameters for this step.
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; }

        /// <summary>
        /// Gets or sets child steps for container step types like TimeGuard.
        /// </summary>
        public List<TestStepJson> ChildSteps { get; set; }
    }
}