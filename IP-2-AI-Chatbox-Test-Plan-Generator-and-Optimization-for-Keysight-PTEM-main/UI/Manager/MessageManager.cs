using ChatboxPlugin.Theme;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace ChatboxPlugin.UI.Manager
{
    /// <summary>
    /// Manages the display and addition of messages in the chatbox.
    /// </summary>
    public class MessageManager
    {
        private readonly StackPanel _messagesPanel;
        private readonly ScrollViewer _conversationArea;
        private readonly ThemeManager _themeManager;

        /// <summary>
        /// Initializes a new instance of the MessageManager class.
        /// </summary>
        /// <param name="messagesPanel">The panel where messages are displayed.</param>
        /// <param name="conversationArea">The scrollable conversation area.</param>
        /// <param name="themeManager">The theme manager for styling.</param>
        public MessageManager(StackPanel messagesPanel, ScrollViewer conversationArea, ThemeManager themeManager)
        {
            _messagesPanel = messagesPanel;
            _conversationArea = conversationArea;
            _themeManager = themeManager;
        }

        /// <summary>
        /// Adds a message bubble to the conversation area with animation.
        /// </summary>
        /// <param name="text">The message text.</param>
        /// <param name="isUser">True if the message is from the user, false if from AI.</param>
        public void AddMessage(string text, bool isUser)
        {
            // Create a border for the message bubble
            var messageBorder = new Border
            {
                CornerRadius = new CornerRadius(10),
                Margin = isUser ? new Thickness(50, 5, 5, 5) : new Thickness(5, 5, 50, 5),
                Padding = new Thickness(10),
                HorizontalAlignment = isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                Opacity = 0,
                Effect = new DropShadowEffect { ShadowDepth = 1, Opacity = 0.3, BlurRadius = 3 }
            };

            // Create a text block for the message content
            var textBlock = new TextBlock 
            { 
                Text = text, 
                TextWrapping = TextWrapping.Wrap 
            };
            
            messageBorder.Child = textBlock;
            _themeManager.ApplyTheme(messageBorder, isUser);
            _messagesPanel.Children.Add(messageBorder);

            // Fade in animation for the message bubble
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.3));
            messageBorder.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            _conversationArea.ScrollToEnd();
        }

        /// <summary>
        /// Clears all messages from the conversation area.
        /// </summary>
        public void ClearMessages()
        {
            _messagesPanel.Children.Clear();
        }
    }
}