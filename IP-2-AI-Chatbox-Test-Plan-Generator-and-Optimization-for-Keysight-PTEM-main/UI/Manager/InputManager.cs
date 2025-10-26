using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ChatboxPlugin.UI.Manager
{
    /// <summary>
    /// Manages the input text box for the chatbox, handling height adjustments and focus.
    /// </summary>
    public class InputManager
    {
        private readonly TextBox _queryInput;
        private const double MinHeight = 36.0;
        private const double MaxHeight = 100.0;

        /// <summary>
        /// Initializes a new instance of the InputManager class.
        /// </summary>
        /// <param name="queryInput">The text box to manage.</param>
        public InputManager(TextBox queryInput)
        {
            _queryInput = queryInput ?? throw new ArgumentNullException(nameof(queryInput));
        }

        /// <summary>
        /// Adjusts the height of the text box based on its content.
        /// </summary>
        public void AdjustTextBoxHeight()
        {
            // Ensure the text box is not null before proceeding
            // and that it is not in a collapsed state
            if (_queryInput == null)
                return;

            // Update the layout of the text box
            _queryInput.Dispatcher.BeginInvoke(new Action(() =>
            {
                _queryInput.UpdateLayout();

                // If the text box is empty, reset to minimum height
                if (string.IsNullOrEmpty(_queryInput.Text))
                {
                    _queryInput.Height = MinHeight;  
                    _queryInput.InvalidateMeasure(); // Ensure layout recalculates
                    _queryInput.UpdateLayout();      // Apply immediately
                }
                // If the text box is not empty, set the height to the desired height
                else
                {
                    _queryInput.Height = double.NaN; // Allow the text box to expand to its content 
                    _queryInput.UpdateLayout();      // Apply immediately
                    _queryInput.Measure(new Size(_queryInput.ActualWidth, double.PositiveInfinity)); // Measure the text box
                    double desiredHeight = _queryInput.DesiredSize.Height; // Get the desired height of the text box
                    double newHeight = Math.Max(MinHeight, Math.Min(desiredHeight, MaxHeight)); // Ensure the height is within the bounds
                    _queryInput.Height = newHeight;  // Set the new height
                }
            }), DispatcherPriority.Render); // Schedule the update for the UI thread
        }

        /// <summary>
        /// Clears the text box and sets focus to it.
        /// </summary>
        public void ClearAndFocus()
        {
            if (_queryInput == null)
                return;

            // Use Clear() which is more thorough than setting Text to empty string
            _queryInput.Clear();
            
            // Force height reset immediately
            _queryInput.Height = MinHeight;

            // Schedule further layout updates and focus with higher priority
            _queryInput.Dispatcher.BeginInvoke(new Action(() =>
            {
                // Ensure textbox is properly reset
                _queryInput.Clear();
                _queryInput.Text = string.Empty;
                _queryInput.InvalidateMeasure();
                _queryInput.UpdateLayout();
                _queryInput.CaretIndex = 0;
                _queryInput.Focus();
            }), DispatcherPriority.Render);
        }
        
        /// <summary>
        /// Performs a complete reset of the textbox after the typing indicator is removed.
        /// This ensures the textbox is properly reset after AI processing.
        /// </summary>
        public void ResetAfterTypingIndicator()
        {
            if (_queryInput == null)
                return;
            
            // Execute with a slight delay to ensure proper sequence after typing indicator removal
            _queryInput.Dispatcher.BeginInvoke(new Action(() =>
            {
                // Completely reset the textbox
                _queryInput.Clear();
                _queryInput.Text = string.Empty;
                _queryInput.Height = MinHeight;
                _queryInput.InvalidateMeasure();
                _queryInput.UpdateLayout();
                _queryInput.CaretIndex = 0;
                _queryInput.Focus();
            }), DispatcherPriority.ApplicationIdle);
        }
    }
}