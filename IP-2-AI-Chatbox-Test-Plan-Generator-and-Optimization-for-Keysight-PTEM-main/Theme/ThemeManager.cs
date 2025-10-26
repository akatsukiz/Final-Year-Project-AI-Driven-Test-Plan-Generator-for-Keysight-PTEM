using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace ChatboxPlugin.Theme
{
    /// <summary>
    /// Manages theme detection and color schemes for the chatbox.
    /// Provides properties for UI element colors based on the current theme,
    /// and methods to apply themes to various UI components.
    /// Implements INotifyPropertyChanged to update UI when theme changes.
    /// </summary>
    public class ThemeManager : INotifyPropertyChanged
    {
        private bool _isDarkMode;
        private ColorSchemes.ColorScheme _currentScheme;

        /// <summary>
        /// Indicates whether dark mode is active.
        /// When this property changes, it updates the current color scheme.
        /// </summary>
        public bool IsDarkMode
        {
            get => _isDarkMode;
            private set
            {
                if (_isDarkMode != value)
                {
                    _isDarkMode = value;
                    _currentScheme = _isDarkMode ? ColorSchemes.DarkMode : ColorSchemes.LightMode;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>Gets the background brush for user message bubbles.</summary>
        public SolidColorBrush UserBubbleBackground => _currentScheme.UserBubbleBackground;
        
        /// <summary>Gets the background brush for AI message bubbles.</summary>
        public SolidColorBrush AiBubbleBackground => _currentScheme.AiBubbleBackground;
        
        /// <summary>Gets the text color brush for user messages.</summary>
        public SolidColorBrush UserTextColor => _currentScheme.UserTextColor;
        
        /// <summary>Gets the text color brush for AI messages.</summary>
        public SolidColorBrush AiTextColor => _currentScheme.AiTextColor;
        
        /// <summary>Gets the background brush for the input text box.</summary>
        public SolidColorBrush InputBackground => _currentScheme.InputBackground;
        
        /// <summary>Gets the text color brush for the input text box.</summary>
        public SolidColorBrush InputTextColor => _currentScheme.InputTextColor;
        
        /// <summary>Gets the background brush for the typing indicator.</summary>
        public SolidColorBrush TypingIndicatorBackground => _currentScheme.TypingIndicatorBackground;
        
        /// <summary>Gets the color brush for the typing indicator dots.</summary>
        public SolidColorBrush TypingIndicatorDotColor => _currentScheme.TypingIndicatorDotColor;

        /// <summary>
        /// Initializes a new instance of the ThemeManager class with a default theme.
        /// Theme detection should be triggered later with DetectSystemTheme.
        /// </summary>
        public ThemeManager()
        {
            _isDarkMode = true; // Default to dark mode until detection occurs
            _currentScheme = _isDarkMode ? ColorSchemes.DarkMode : ColorSchemes.LightMode;
        }

        /// <summary>
        /// Detects the theme based on the provided background brush's brightness.
        /// Uses a perceptual brightness formula based on RGB values.
        /// </summary>
        /// <param name="backgroundBrush">The background brush to analyse.</param>
        public void DetectSystemTheme(SolidColorBrush backgroundBrush)
        {
            if (backgroundBrush != null)
            {
                Color bgColor = backgroundBrush.Color;
                // Perceptual brightness formula
                double brightness = (0.299 * bgColor.R + 0.587 * bgColor.G + 0.114 * bgColor.B) / 255;
                IsDarkMode = brightness < 0.5;
            }
            else
            {
                IsDarkMode = true; // Default to dark mode if no background brush is provided
            }
        }

        /// <summary>
        /// Applies the current theme to the specified UI element.
        /// Different UI elements receive different styling based on their type.
        /// </summary>
        /// <param name="element">The UI element to apply the theme to.</param>
        /// <param name="isUserMessage">Indicates if the element is a user message bubble.</param>
        public void ApplyTheme(UIElement element, bool isUserMessage = false)
        {
            if (element is Border border && border.Child is TextBlock textBlock)
            {
                // Apply theme to message bubbles
                border.Background = isUserMessage ? UserBubbleBackground : AiBubbleBackground;
                textBlock.Foreground = isUserMessage ? UserTextColor : AiTextColor;
            }
            else if (element is TextBox textBox)
            {
                // Apply theme to text boxes
                textBox.Background = InputBackground;
                textBox.Foreground = InputTextColor;
                textBox.CaretBrush = InputTextColor;
            }
            else if (element is Border textBoxBorder)
            {
                // Apply theme to borders around text boxes
                textBoxBorder.Background = InputBackground;
            }
        }

        /// <summary>
        /// Implements the INotifyPropertyChanged interface to notify UI of property changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        
        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}