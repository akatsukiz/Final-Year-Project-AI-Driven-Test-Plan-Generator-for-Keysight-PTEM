using ChatboxPlugin.UI.Control;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace ChatboxPlugin.Theme
{
    /// <summary>
    /// Monitors the application theme and applies appropriate styles to UI elements.
    /// </summary>
    public class ThemeMonitor
    {
        private readonly ThemeManager _themeManager;
        private readonly ChatboxUI _ui;

        /// <summary>
        /// Initializes a new instance of the ThemeMonitor class.
        /// </summary>
        /// <param name="themeManager">The theme manager to use.</param>
        /// <param name="ui">The UI to apply themes to.</param>
        public ThemeMonitor(ThemeManager themeManager, ChatboxUI ui)
        {
            _themeManager = themeManager;
            _ui = ui;
        }

        /// <summary>
        /// Starts monitoring the theme by listening to the main window's Background property changes.
        /// If the main window is not immediately available, sets up a timer to check periodically.
        /// </summary>
        public void StartThemeMonitoring()
        {
            var mainWindow = Application.Current.MainWindow;
            if (mainWindow != null)
            {
                // Perform initial theme detection
                DetectAndApplyTheme(mainWindow);

                // Set up a property change listener for future theme changes
                var propertyDescriptor = DependencyPropertyDescriptor.FromProperty(
                    Control.BackgroundProperty, 
                    typeof(Window)
                );
                propertyDescriptor.AddValueChanged(mainWindow, (s, e) => DetectAndApplyTheme(mainWindow));
            }
            else
            {
                // If main window isn't available yet, use a timer to check periodically
                var pollingTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
                pollingTimer.Tick += (s, e) =>
                {
                    if (Application.Current.MainWindow != null)
                    {
                        // Stop the timer after having the main window
                        DetectAndApplyTheme(Application.Current.MainWindow);
                        pollingTimer.Stop();
                        
                        // Set up proper monitoring
                        StartThemeMonitoring();
                    }
                };
                pollingTimer.Start();
            }
        }

        /// <summary>
        /// Detects the current theme from the window background and applies it to all UI elements.
        /// </summary>
        /// <param name="window">The window to analyse for theme detection.</param>
        private void DetectAndApplyTheme(Window window)
        {
            // Extract the appropriate background brush for theme detection
            SolidColorBrush backgroundBrush = ExtractBackgroundBrush(window);
            
            // Update the theme manager with the detected background
            _themeManager.DetectSystemTheme(backgroundBrush);

            // Apply the theme to all UI elements
            ApplyCurrentTheme();
        }
        
        /// <summary>
        /// Extracts the background brush from the window or application resources.
        /// </summary>
        /// <param name="window">The window to extract the background from.</param>
        /// <returns>The extracted SolidColorBrush, or null if none could be found.</returns>
        private SolidColorBrush ExtractBackgroundBrush(Window window)
        {
            if (window == null)
                return null;
                
            // First try to get the brush directly from the window background
            if (window.Background is SolidColorBrush directBrush)
                return directBrush;
                
            // If that fails, look for a "BackgroundBrush" in the application resources
            if (Application.Current.Resources.Contains("BackgroundBrush") && 
                Application.Current.Resources["BackgroundBrush"] is SolidColorBrush resourceBrush)
                return resourceBrush;
                
            // If no suitable brush found, return null
            return null;
        }
        
        /// <summary>
        /// Applies the current theme to all UI elements in the chatbox.
        /// </summary>
        private void ApplyCurrentTheme()
        {
            // Apply theme to input controls
            _themeManager.ApplyTheme(_ui.QueryInput);
            _themeManager.ApplyTheme(_ui.TextboxBorder);
            
            // Apply theme to all message bubbles
            foreach (var child in _ui.MessagesPanel.Children)
            {
                if (child is Border messageBorder && messageBorder.Child is TextBlock)
                {
                    bool isUserMessage = messageBorder.HorizontalAlignment == HorizontalAlignment.Right;
                    _themeManager.ApplyTheme(messageBorder, isUserMessage);
                }
            }
        }
    }
} 