using System.Windows.Media;

namespace ChatboxPlugin.Theme
{
    /// <summary>
    /// Defines a palette of reusable colors for the chatbox UI components.
    /// </summary>
    public static class ColorPalette
    {
        // Base colors for UI elements
        /// <summary>Primary blue color used for user message bubbles and buttons.</summary>
        public static readonly Color Blue = Color.FromRgb(0, 120, 215);
        
        /// <summary>Dark gray color used for AI message bubbles in dark mode.</summary>
        public static readonly Color DarkGray = Color.FromRgb(64, 64, 64);
        
        /// <summary>Light gray color used for AI message bubbles in light mode.</summary>
        public static readonly Color LightGray = Color.FromRgb(240, 240, 240);
        
        /// <summary>Very dark gray color used for backgrounds in dark mode.</summary>
        public static readonly Color VeryDarkGray = Color.FromRgb(30, 30, 30);
        
        /// <summary>Medium gray color used for secondary elements.</summary>
        public static readonly Color MediumGray = Color.FromRgb(100, 100, 100);
        
        /// <summary>White color used for text and backgrounds.</summary>
        public static readonly Color White = Colors.White;
        
        /// <summary>Black color used for text and borders.</summary>
        public static readonly Color Black = Colors.Black;

        /// <summary>
        /// Creates a frozen (immutable) SolidColorBrush from a color value.
        /// </summary>
        /// <param name="color">The color to create a brush from.</param>
        /// <returns>A frozen SolidColorBrush with the specified color.</returns>
        public static SolidColorBrush CreateBrush(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze(); // Make immutable for performance and thread safety
            return brush;
        }
    }
}