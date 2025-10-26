using ChatboxPlugin.Theme;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ChatboxPlugin.UI.Control
{
    /// <summary>  
    /// Handles the UI setup for the chatbox control.  
    /// Creates and configures all UI elements including the conversation area,   
    /// input field, and send button.  
    /// </summary>  
    public class ChatboxUI
    {
        /// <summary>  
        /// Gets the main grid containing all chatbox UI elements.  
        /// This is the root container of the chatbox interface.  
        /// </summary>  
        public Grid MainGrid { get; }

        /// <summary>  
        /// Gets the panel where messages are displayed.  
        /// Messages are added to this panel as they are sent or received.  
        /// </summary>  
        public StackPanel MessagesPanel { get; private set; }

        /// <summary>  
        /// Gets the text box for user input.  
        /// Users type their queries in this text box.  
        /// </summary>  
        public TextBox QueryInput { get; private set; }

        /// <summary>  
        /// Gets the scrollable area containing the conversation.  
        /// This ScrollViewer wraps the MessagesPanel to allow scrolling.  
        /// </summary>  
        public ScrollViewer ConversationArea { get; }

        /// <summary>  
        /// Gets the button to send messages.  
        /// Clicking this button will send the contents of the QueryInput.  
        /// </summary>  
        public Button SendButton { get; private set;  }

        /// <summary>  
        /// Gets the border surrounding the input text box.  
        /// This border provides styling and visual separation for the input area.  
        /// </summary>  
        public Border TextboxBorder { get; private set;  }

        /// <summary>  
        /// Initializes a new instance of the ChatboxUI class.  
        /// Sets up the entire UI layout including the conversation area and input controls.  
        /// </summary>  
        /// <param name="themeManager">The theme manager to apply styles to UI elements.</param>  
        public ChatboxUI(ThemeManager themeManager)
        {
            // Create main grid with two rows: conversation area and input area  
            MainGrid = new Grid();
            MainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Create the conversation area (ScrollViewer with a StackPanel for messages)  
            ConversationArea = CreateConversationArea();
            MainGrid.Children.Add(ConversationArea);

            // Create the input grid with text box and send button  
            var inputGrid = CreateInputGrid();
            Grid.SetRow(inputGrid, 1);
            MainGrid.Children.Add(inputGrid);

            // Apply theme to input elements  
            themeManager.ApplyTheme(QueryInput);
            themeManager.ApplyTheme(TextboxBorder);
        }

        /// <summary>  
        /// Creates the conversation area with a scrollable message panel.  
        /// </summary>  
        /// <returns>A configured ScrollViewer containing the messages panel.</returns>  
        private ScrollViewer CreateConversationArea()
        {
            // Create the scrollable conversation area  
            var conversationScrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(5),
                Background = new SolidColorBrush(Colors.Transparent)
            };

            // Create the panel to hold message bubbles  
            MessagesPanel = new StackPanel();
            conversationScrollViewer.Content = MessagesPanel;
            Grid.SetRow(conversationScrollViewer, 0);

            return conversationScrollViewer;
        }

        /// <summary>  
        /// Creates the input grid containing the text box and send button.  
        /// </summary>  
        /// <returns>A configured Grid with the input controls.</returns>  
        private Grid CreateInputGrid()
        {
            // Create a grid with two columns for the text box and send button  
            var inputGrid = new Grid { Margin = new Thickness(5) };
            inputGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            inputGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Create and add the text box inside a border  
            TextboxBorder = CreateTextboxBorder();
            Grid.SetColumn(TextboxBorder, 0);
            inputGrid.Children.Add(TextboxBorder);

            // Create and add the send button  
            SendButton = CreateSendButton();
            Grid.SetColumn(SendButton, 1);
            inputGrid.Children.Add(SendButton);

            return inputGrid;
        }

        /// <summary>  
        /// Creates the border that contains the input text box.  
        /// </summary>  
        /// <returns>A configured Border containing the query input text box.</returns>  
        private Border CreateTextboxBorder()
        {
            // Create the border with rounded corners  
            var border = new Border
            {
                BorderBrush = new SolidColorBrush(Colors.Gray),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(5),
                Margin = new Thickness(0, 0, 5, 0)
            };

            // Create the input text box  
            QueryInput = new TextBox
            {
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Padding = new Thickness(8, 6, 8, 6),
                MinHeight = 36,
                MaxHeight = 100,
                BorderThickness = new Thickness(0)
            };

            // Add the text box to the border  
            border.Child = QueryInput;

            return border;
        }

        /// <summary>  
        /// Creates the send button with appropriate styling.  
        /// </summary>  
        /// <returns>A configured send Button.</returns>  
        private Button CreateSendButton()
        {
            return new Button
            {
                Content = "Send",
                Height = 36,
                Width = 80,
                Background = new SolidColorBrush(Color.FromRgb(0, 120, 215)),
                Foreground = new SolidColorBrush(Colors.White),
                BorderThickness = new Thickness(0),
                Padding = new Thickness(10, 5, 10, 5),
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Top
            };
        }
    }
}