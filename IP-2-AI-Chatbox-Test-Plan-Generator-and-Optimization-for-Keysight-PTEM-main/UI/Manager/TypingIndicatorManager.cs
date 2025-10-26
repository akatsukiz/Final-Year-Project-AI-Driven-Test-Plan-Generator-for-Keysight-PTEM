using ChatboxPlugin.Theme;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ChatboxPlugin.UI.Manager
{
    /// <summary>
    /// Manages the typing indicator animation in the chatbox.
    /// </summary>
    public class TypingIndicatorManager
    {
        private Border _typingIndicator;
        private readonly StackPanel _messagesPanel;
        private readonly ScrollViewer _conversationArea;
        private readonly ThemeManager _themeManager;
        private const int DotCount = 3;

        /// <summary>
        /// Initializes a new instance of the TypingIndicatorManager class.
        /// </summary>
        /// <param name="messagesPanel">The panel where the typing indicator is displayed.</param>
        /// <param name="conversationArea">The scrollable conversation area.</param>
        /// <param name="themeManager">The theme manager for styling.</param>
        public TypingIndicatorManager(StackPanel messagesPanel, ScrollViewer conversationArea, ThemeManager themeManager)
        {
            _messagesPanel = messagesPanel;
            _conversationArea = conversationArea;
            _themeManager = themeManager;

            // Subscribe to theme changes
            _themeManager.PropertyChanged += ThemeManager_PropertyChanged;
        }

        /// <summary>
        /// Handles the property changed event from the theme manager.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments containing the property name.</param>
        private void ThemeManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Check if the theme changed and the typing indicator is active
            if (e.PropertyName == nameof(_themeManager.IsDarkMode) && _typingIndicator != null)
            {
                UpdateTypingIndicatorColors();
            }
        }

        /// <summary>
        /// Updates the colors of the typing indicator based on the current theme.
        /// </summary>
        private void UpdateTypingIndicatorColors()
        {
            // Update the background of the typing indicator
            _typingIndicator.Background = _themeManager.TypingIndicatorBackground;

            // Update the fill color of each dot
            if (_typingIndicator.Child is StackPanel typingPanel)
            {
                foreach (var child in typingPanel.Children)
                {
                    if (child is Ellipse dot)
                    {
                        dot.Fill = _themeManager.TypingIndicatorDotColor;
                    }
                }
            }
        }

        /// <summary>
        /// Displays a typing indicator with animated dots in the conversation area.
        /// </summary>
        public void ShowTypingIndicator()
        {
            // Create typing indicator container
            _typingIndicator = new Border
            {
                CornerRadius = new CornerRadius(10),
                Margin = new Thickness(5, 5, 50, 5),
                Padding = new Thickness(12, 8, 12, 8),
                HorizontalAlignment = HorizontalAlignment.Left,
                MinWidth = 60
            };

            // Create a panel with three animated dots for the typing indicator
            var typingPanel = new StackPanel { Orientation = Orientation.Horizontal };
            
            // Add the animated dots
            for (int i = 0; i < DotCount; i++)
            {
                var dot = CreateAnimatedDot(i);
                typingPanel.Children.Add(dot);
            }

            _typingIndicator.Child = typingPanel;
            _typingIndicator.Background = _themeManager.TypingIndicatorBackground;
            _messagesPanel.Children.Add(_typingIndicator);
            _conversationArea.ScrollToEnd();
        }

        /// <summary>
        /// Creates an animated dot for the typing indicator.
        /// </summary>
        /// <param name="index">The index of the dot, used to stagger animations.</param>
        /// <returns>An ellipse with animation applied.</returns>
        private Ellipse CreateAnimatedDot(int index)
        {
            var dot = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = _themeManager.TypingIndicatorDotColor,
                Margin = new Thickness(3)
            };
            
            var animation = new DoubleAnimation
            {
                From = 0.3,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.6),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                BeginTime = TimeSpan.FromSeconds(0.2 * index)
            };
            
            dot.BeginAnimation(UIElement.OpacityProperty, animation);
            return dot;
        }

        /// <summary>
        /// Removes the typing indicator from the conversation area.
        /// </summary>
        public void RemoveTypingIndicator()
        {
            if (_typingIndicator != null && _messagesPanel.Children.Contains(_typingIndicator))
            {
                _messagesPanel.Children.Remove(_typingIndicator);
                _typingIndicator = null;
            }
        }
    }
}