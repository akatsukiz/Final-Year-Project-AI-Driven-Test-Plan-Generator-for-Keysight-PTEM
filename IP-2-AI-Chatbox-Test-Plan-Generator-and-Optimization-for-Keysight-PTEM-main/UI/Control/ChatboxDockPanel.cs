using Keysight.OpenTap.Wpf;
using OpenTap;
using System.Windows;

namespace ChatboxPlugin.UI.Control
{
    /// <summary>
    /// A dockable panel that hosts the ChatboxControl within the OpenTAP GUI.
    /// Provides integration with the OpenTAP environment to make the chatbox accessible.
    /// </summary>
    [Display("AI Chatbox")]
    public class ChatboxDockPanel : ITapDockPanel
    {
        /// <summary>
        /// Gets the desired initial width of the dockable panel.
        /// </summary>
        public double? DesiredWidth => 400;

        /// <summary>
        /// Gets the desired initial height of the dockable panel.
        /// </summary>
        public double? DesiredHeight => 300;

        /// <summary>
        /// Creates and returns the ChatboxControl as the dockable element.
        /// This method is called by OpenTAP when the panel is first loaded.
        /// </summary>
        /// <param name="context">The dock context provided by OpenTAP, which contains access to the current test plan.</param>
        /// <returns>The FrameworkElement to be docked in the OpenTAP GUI.</returns>
        public FrameworkElement CreateElement(ITapDockContext context)
        {
            return new ChatboxControl(context);
        }
    }
}