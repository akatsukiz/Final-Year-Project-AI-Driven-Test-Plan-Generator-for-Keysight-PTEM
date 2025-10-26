using System.Windows.Media;

namespace ChatboxPlugin.Theme
{
    /// <summary>
    /// Defines color schemes for different themes in the chatbox UI.
    /// Provides predefined dark and light mode color schemes.
    /// </summary>
    public static class ColorSchemes
    {
        /// <summary>
        /// Represents a complete color scheme for a specific theme (dark or light).
        /// Contains brushes for all UI elements in the chatbox.
        /// </summary>
        public class ColorScheme
        {
            /// <summary>Gets the background brush for user message bubbles.</summary>
            public SolidColorBrush UserBubbleBackground { get; }
            
            /// <summary>Gets the background brush for AI message bubbles.</summary>
            public SolidColorBrush AiBubbleBackground { get; }
            
            /// <summary>Gets the text color brush for user messages.</summary>
            public SolidColorBrush UserTextColor { get; }
            
            /// <summary>Gets the text color brush for AI messages.</summary>
            public SolidColorBrush AiTextColor { get; }
            
            /// <summary>Gets the background brush for the input text box.</summary>
            public SolidColorBrush InputBackground { get; }
            
            /// <summary>Gets the text color brush for the input text box.</summary>
            public SolidColorBrush InputTextColor { get; }
            
            /// <summary>Gets the background brush for the typing indicator.</summary>
            public SolidColorBrush TypingIndicatorBackground { get; }
            
            /// <summary>Gets the color brush for the typing indicator dots.</summary>
            public SolidColorBrush TypingIndicatorDotColor { get; }

            /// <summary>
            /// Initializes a new instance of the ColorScheme class with the specified brushes.
            /// All brushes are frozen for performance and thread safety.
            /// </summary>
            /// <param name="userBubbleBackground">Background brush for user message bubbles</param>
            /// <param name="aiBubbleBackground">Background brush for AI message bubbles</param>
            /// <param name="userTextColor">Text color brush for user messages</param>
            /// <param name="aiTextColor">Text color brush for AI messages</param>
            /// <param name="inputBackground">Background brush for the input text box</param>
            /// <param name="inputTextColor">Text color brush for the input text box</param>
            /// <param name="typingIndicatorBackground">Background brush for the typing indicator</param>
            /// <param name="typingIndicatorDotColor">Color brush for the typing indicator dots</param>
            public ColorScheme(
                SolidColorBrush userBubbleBackground,
                SolidColorBrush aiBubbleBackground,
                SolidColorBrush userTextColor,
                SolidColorBrush aiTextColor,
                SolidColorBrush inputBackground,
                SolidColorBrush inputTextColor,
                SolidColorBrush typingIndicatorBackground,
                SolidColorBrush typingIndicatorDotColor)
            {
                UserBubbleBackground = userBubbleBackground;
                AiBubbleBackground = aiBubbleBackground;
                UserTextColor = userTextColor;
                AiTextColor = aiTextColor;
                InputBackground = inputBackground;
                InputTextColor = inputTextColor;
                TypingIndicatorBackground = typingIndicatorBackground;
                TypingIndicatorDotColor = typingIndicatorDotColor;

                // Ensure all brushes are frozen to make them immutable for better performance
                UserBubbleBackground.Freeze();
                AiBubbleBackground.Freeze();
                UserTextColor.Freeze();
                AiTextColor.Freeze();
                InputBackground.Freeze();
                InputTextColor.Freeze();
                TypingIndicatorBackground.Freeze();
                TypingIndicatorDotColor.Freeze();
            }
        }

        /// <summary>
        /// Predefined dark mode color scheme with appropriate contrast for dark backgrounds.
        /// User bubbles are blue, AI bubbles are dark gray, and text is white for contrast.
        /// </summary>
        public static readonly ColorScheme DarkMode = new ColorScheme(
            userBubbleBackground: ColorPalette.CreateBrush(ColorPalette.Blue),
            aiBubbleBackground: ColorPalette.CreateBrush(ColorPalette.DarkGray),
            userTextColor: ColorPalette.CreateBrush(ColorPalette.White),
            aiTextColor: ColorPalette.CreateBrush(ColorPalette.White),
            inputBackground: ColorPalette.CreateBrush(ColorPalette.VeryDarkGray),
            inputTextColor: ColorPalette.CreateBrush(ColorPalette.White),
            typingIndicatorBackground: ColorPalette.CreateBrush(ColorPalette.DarkGray),
            typingIndicatorDotColor: ColorPalette.CreateBrush(ColorPalette.White)
        );

        /// <summary>
        /// Predefined light mode color scheme with appropriate contrast for light backgrounds.
        /// User bubbles are blue, AI bubbles are light gray, with text colors adjusted for readability.
        /// </summary>
        public static readonly ColorScheme LightMode = new ColorScheme(
            userBubbleBackground: ColorPalette.CreateBrush(ColorPalette.Blue),
            aiBubbleBackground: ColorPalette.CreateBrush(ColorPalette.LightGray),
            userTextColor: ColorPalette.CreateBrush(ColorPalette.White),
            aiTextColor: ColorPalette.CreateBrush(ColorPalette.Black),
            inputBackground: ColorPalette.CreateBrush(ColorPalette.White),
            inputTextColor: ColorPalette.CreateBrush(ColorPalette.Black),
            typingIndicatorBackground: ColorPalette.CreateBrush(ColorPalette.LightGray),
            typingIndicatorDotColor: ColorPalette.CreateBrush(ColorPalette.MediumGray)
        );
    }
}