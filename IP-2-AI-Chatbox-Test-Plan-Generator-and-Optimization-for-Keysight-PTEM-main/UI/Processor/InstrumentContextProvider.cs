using System;
using System.IO;
using ChatboxPlugin.Service.Utility;

namespace ChatboxPlugin.UI.Processor
{
    /// <summary>
    /// Provides instrument context information and handles extraction of manufacturer and model details.
    /// </summary>
    public class InstrumentContextProvider
    {
        /// <summary>
        /// Extracts manufacturer and model information from the instrument context.
        /// </summary>
        /// <param name="instrumentContext">The raw instrument context string.</param>
        /// <returns>A tuple containing (manufacturer, model).</returns>
        public Tuple<string, string> ExtractInstrumentInfo(string instrumentContext)
        {
            string manufacturer = "Unknown";
            string model = "Unknown";

            // Parse the instrument context to extract manufacturer and model
            using (StringReader reader = new StringReader(instrumentContext))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains("Manufacturer:"))
                    {
                        manufacturer = line.Substring(line.IndexOf("Manufacturer:") + "Manufacturer:".Length).Trim();
                    }
                    else if (line.Contains("Model:"))
                    {
                        model = line.Substring(line.IndexOf("Model:") + "Model:".Length).Trim();
                        // Once we have both manufacturer and model, we can break out of the loop
                        if (manufacturer != "Unknown")
                            break;
                    }
                }
            }

            return new Tuple<string, string>(manufacturer, model);
        }

        /// <summary>
        /// Gets the current instrument context, or returns null if no instruments are defined.
        /// </summary>
        /// <returns>The instrument context string, or null if no instruments are defined.</returns>
        public string GetInstrumentContext()
        {
            string instrumentContext = ChatboxUtilities.GetInstrumentContext();
            
            if (string.IsNullOrEmpty(instrumentContext))
                return null;
            
            return instrumentContext;
        }
    }
} 