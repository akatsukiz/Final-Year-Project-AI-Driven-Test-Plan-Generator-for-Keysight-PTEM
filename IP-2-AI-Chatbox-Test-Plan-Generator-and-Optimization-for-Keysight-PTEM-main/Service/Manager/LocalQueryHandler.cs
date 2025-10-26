using ChatboxPlugin.Service.Utility;
using ChatboxPlugin.UI.Manager;

namespace ChatboxPlugin.Service.Manager
{
    /// <summary>
    /// Handles queries that can be processed locally without AI interaction.
    /// </summary>
    public class LocalQueryHandler
    {
        private readonly MessageManager _messageManager;
        private readonly TestPlanManager _testPlanManager;

        /// <summary>
        /// Initializes a new instance of the LocalQueryHandler class.
        /// </summary>
        /// <param name="messageManager">The manager for displaying messages.</param>
        /// <param name="testPlanManager">The manager for test plan operations.</param>
        public LocalQueryHandler(MessageManager messageManager, TestPlanManager testPlanManager)
        {
            _messageManager = messageManager;
            _testPlanManager = testPlanManager;
        }

        /// <summary>
        /// Handles queries that can be processed locally without sending to AI.
        /// </summary>
        /// <param name="query">The user's query.</param>
        /// <returns>True if the query was handled locally, false otherwise.</returns>
        public bool HandleLocalQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return false;

            string lowerQuery = query.ToLower().Trim();

            if (lowerQuery == "help" || lowerQuery == "?")
                return ShowHelp();
            if (lowerQuery == "list instruments" || lowerQuery == "list instrument")
                return ListInstruments();
            if (lowerQuery == "list scpi")
                return ListScpiSteps();
            if (lowerQuery == "clear")
                return ClearConversation();

            return false;
        }

        /// <summary>
        /// Displays the help message with available commands.
        /// </summary>
        /// <returns>True to indicate the query was handled.</returns>
        private bool ShowHelp()
        {
            _messageManager.AddMessage("Available commands:\n" +
                                      "- help: Show this help message\n" +
                                      "- list scpi: Show current SCPI steps\n" +
                                      "- list instruments: Show current instruments\n" +
                                      "- clear: Clear the conversation", false);
            return true;
        }

        /// <summary>
        /// Lists the available instruments.
        /// </summary>
        /// <returns>True to indicate the query was handled.</returns>
        private bool ListInstruments()
        {
            string instrumentContext = ChatboxUtilities.GetInstrumentContext();
            if (string.IsNullOrEmpty(instrumentContext))
            {
                _messageManager.AddMessage("No instruments found in the current session.", false);
            }
            else
            {
                // The information is already formatted in GetInstrumentContext, so we can use it directly
                _messageManager.AddMessage(instrumentContext, false);
            }
            return true;
        }

        /// <summary>
        /// Lists the SCPI steps in the current test plan.
        /// </summary>
        /// <returns>True to indicate the query was handled.</returns>
        private bool ListScpiSteps()
        {
            var context = _testPlanManager.GetOrCreateTestPlanContext();
            if (context.Error != null)
            {
                _messageManager.AddMessage(context.Error, false);
            }
            else
            {
                string scpiStepsContext = ChatboxUtilities.GetScpiStepsContext(context.TestPlan);
                _messageManager.AddMessage(scpiStepsContext, false);
            }
            return true;
        }

        /// <summary>
        /// Clears the conversation history.
        /// </summary>
        /// <returns>True to indicate the query was handled.</returns>
        private bool ClearConversation()
        {
            _messageManager.ClearMessages();
            _messageManager.AddMessage("Conversation cleared.", false);
            return true;
        }
    }
}