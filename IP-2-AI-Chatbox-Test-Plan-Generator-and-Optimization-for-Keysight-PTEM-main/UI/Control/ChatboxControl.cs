using ChatboxPlugin.Service.AI;
using ChatboxPlugin.Service.Manager;
using ChatboxPlugin.Theme;
using ChatboxPlugin.UI.Manager;
using Keysight.OpenTap.Wpf;
using System.Windows.Controls;
using System.Windows.Input;
using InputManager = ChatboxPlugin.UI.Manager.InputManager;

namespace ChatboxPlugin.UI.Control
{
    /// <summary>
    /// A UserControl that provides an interactive chatbox interface for generating and optimising
    /// test plans using AI. Includes message bubbles, input field, and typing indicator.
    /// </summary>
    public class ChatboxControl : UserControl
    {
        private readonly ChatboxUI _ui;
        private readonly ThemeManager _themeManager;
        private readonly ThemeMonitor _themeMonitor;
        private readonly MessageManager _messageManager;
        private readonly TypingIndicatorManager _typingIndicatorManager;
        private readonly InputManager _inputManager;
        private readonly ChatboxLogic _chatboxController;

        /// <summary>
        /// Initializes a new instance of the ChatboxControl class.
        /// Sets up the UI components, service managers, and event handlers for the chatbox.
        /// </summary>
        /// <param name="dockContext">The TAP dock context providing access to the current test plan.</param>
        public ChatboxControl(ITapDockContext dockContext = null)
        {
            // Initialize the theme manager for dynamic theming
            _themeManager = new ThemeManager();

            // Initialize the AI service for generating test plans
            var aiService = new AIService();

            // Initialize the UI components and set as control content
            _ui = new ChatboxUI(_themeManager);
            Content = _ui.MainGrid;

            // Initialize specialised managers for different aspects of functionality
            _messageManager = new MessageManager(_ui.MessagesPanel, _ui.ConversationArea, _themeManager);
            _typingIndicatorManager = new TypingIndicatorManager(_ui.MessagesPanel, _ui.ConversationArea, _themeManager);
            _inputManager = new InputManager(_ui.QueryInput);
            
            // Initialize theme monitor
            _themeMonitor = new ThemeMonitor(_themeManager, _ui);

            // Initialize services
            var aiInteractor = new AiInteractor(aiService);
            var testPlanManager = new TestPlanManager(dockContext);
            var localQueryHandler = new LocalQueryHandler(_messageManager, testPlanManager);

            // Initialize main controller
            _chatboxController = new ChatboxLogic(
                _messageManager,
                _typingIndicatorManager,
                _inputManager,
                aiInteractor,
                testPlanManager,
                localQueryHandler
            );

            // Configure event handlers
            SetupEventHandlers();

            // Begin monitoring the application theme for changes
            _themeMonitor.StartThemeMonitoring();
        }

        /// <summary>
        /// Sets up all event handlers for user interaction.
        /// </summary>
        private void SetupEventHandlers()
        {
            // Handle send button clicks
            _ui.SendButton.Click += async (s, e) => await _chatboxController.SendMessage(_ui.QueryInput.Text.Trim());
            
            // Handle Enter key presses in the input box
            _ui.QueryInput.PreviewKeyDown += async (s, e) =>
            {
                if (e.Key == Key.Enter && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                {
                    await _chatboxController.SendMessage(_ui.QueryInput.Text.Trim());
                    e.Handled = true;
                }
            };
            
            // Adjust text box height as content changes
            _ui.QueryInput.TextChanged += (s, e) => _inputManager.AdjustTextBoxHeight();
            
            // Configure UI when fully loaded
            _ui.MainGrid.Loaded += (s, e) =>
            {
                // Apply theme to UI elements
                _themeManager.ApplyTheme(_ui.QueryInput);
                _themeManager.ApplyTheme(_ui.TextboxBorder);
                
                // Focus the input text box for immediate typing
                _ui.QueryInput.Focus();
                
                // Ensure correct initial height
                _inputManager.AdjustTextBoxHeight();
            };
        }
    }
}