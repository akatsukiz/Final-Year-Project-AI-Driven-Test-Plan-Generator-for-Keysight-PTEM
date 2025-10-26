using OpenTap;
using OpenTap.Plugins.BasicSteps;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatboxPlugin.Service.Utility
{
    /// <summary>
    /// Provides utility methods for the chatbox plugin.
    /// </summary>
    public static class ChatboxUtilities
    {
        private static readonly TraceSource _log = Log.CreateSource("ChatboxUtilities");

        /// <summary>
        /// Gathers detailed context about available instruments using *IDN? queries in a structured format.
        /// </summary>
        /// <returns>A structured string containing instrument details already parsed into components</returns>
        public static string GetInstrumentContext()
        {
            var instruments = InstrumentSettings.Current;
            if (instruments == null || instruments.Count == 0)
                return string.Empty;

            var instrumentDetails = new List<string>();

            for (int i = 0; i < instruments.Count; i++)
            {
                var instrument = instruments[i];

                // Start with a structured format that includes the index
                StringBuilder instrumentInfo = new StringBuilder();
                instrumentInfo.AppendLine($"Instrument {i + 1}: {instrument.Name} ({instrument.GetType().Name})");

                // Handle SCPI instruments
                if (instrument is IScpiInstrument scpiInstrument)
                {
                    bool wasConnected = instrument.IsConnected;
                    try
                    {
                        // Connect to the instrument if not already connected
                        if (!wasConnected)
                        {
                            _log.Debug($"Connecting to instrument {instrument.Name}");
                            instrument.Open();
                        }

                        // Now send the IDN query
                        if (instrument.IsConnected)
                        {
                            string idnResponse = scpiInstrument.ScpiQuery("*IDN?");
                            if (!string.IsNullOrEmpty(idnResponse))
                            {
                                // Parse the IDN response into components
                                string[] idnParts = idnResponse.Trim().Split(',');

                                // Add each available component with proper labelling
                                if (idnParts.Length >= 1 && !string.IsNullOrEmpty(idnParts[0]))
                                    instrumentInfo.AppendLine($"  Manufacturer: {idnParts[0].Trim()}");

                                if (idnParts.Length >= 2 && !string.IsNullOrEmpty(idnParts[1]))
                                    instrumentInfo.AppendLine($"  Model: {idnParts[1].Trim()}");

                                if (idnParts.Length >= 3 && !string.IsNullOrEmpty(idnParts[2]))
                                    instrumentInfo.AppendLine($"  Serial Number: {idnParts[2].Trim()}");

                                if (idnParts.Length >= 4 && !string.IsNullOrEmpty(idnParts[3]))
                                    instrumentInfo.AppendLine($"  Version: {idnParts[3].Trim()}");
                            }
                            else
                            {
                                instrumentInfo.AppendLine($"  Info: Connected but no IDN response");
                            }
                        }
                        else
                        {
                            instrumentInfo.AppendLine($"  Info: Failed to connect");
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Debug($"Error querying instrument {instrument.Name}: {ex.Message}");
                        instrumentInfo.AppendLine($"  Error: {ex.Message}");
                    }
                    finally
                    {
                        // Close the connection if we opened it
                        if (instrument.IsConnected && !wasConnected)
                        {
                            try
                            {
                                instrument.Close();
                            }
                            catch (Exception ex)
                            {
                                _log.Debug($"Error closing instrument {instrument.Name}: {ex.Message}");
                            }
                        }
                    }
                }
                else
                {
                    // Not an SCPI instrument, just use basic info
                    instrumentInfo.AppendLine($"  Info: Non-SCPI instrument");
                }

                // Add a blank line between instruments for better readability
                instrumentInfo.AppendLine();

                // Add the instrument info to the list
                instrumentDetails.Add(instrumentInfo.ToString().TrimEnd());
            }

            return $"Available Instruments:\n\n{string.Join("\n\n", instrumentDetails)}";
        }

        /// <summary>
        /// Retrieves and formats all SCPI steps in the test plan, including child steps, with their exact positions.
        /// </summary>
        /// <param name="testPlan">The test plan to extract SCPI steps from.</param>
        /// <returns>A formatted string listing all SCPI steps with their hierarchical positions.</returns>
        public static string GetScpiStepsContext(TestPlan testPlan)
        {
            if (testPlan == null || testPlan.ChildTestSteps == null)
                return "No test plan or steps available.";

            StringBuilder scpiList = new StringBuilder("SCPI Steps in Test Plan:\n\n");
            int stepPosition = 0;

            for (int i = 0; i < testPlan.ChildTestSteps.Count; i++)
            {
                var step = testPlan.ChildTestSteps[i];
                stepPosition++;

                if (step is SCPIRegexStep scpiStep)
                {
                    scpiList.AppendLine($"Step {stepPosition}: SCPI {scpiStep.Action}: {scpiStep.Name}");
                    scpiList.AppendLine($"  Action: {scpiStep.Action}");
                    scpiList.AppendLine($"  Query: {scpiStep.Query}");
                    scpiList.AppendLine();
                }
                else if (step.ChildTestSteps != null && step.ChildTestSteps.Count > 0)
                {
                    int childPosition = 0;
                    foreach (var childStep in step.ChildTestSteps)
                    {
                        if (childStep is SCPIRegexStep childScpiStep)
                        {
                            childPosition++;
                            scpiList.AppendLine($"Step {stepPosition} Child {childPosition}: SCPI {childScpiStep.Action}: {childScpiStep.Name}");
                            scpiList.AppendLine($"  Action: {childScpiStep.Action}");
                            scpiList.AppendLine($"  Query: {childScpiStep.Query}");
                            scpiList.AppendLine();
                        }
                    }
                }
            }

            return scpiList.Length > "SCPI Steps in Test Plan:\n\n".Length
                ? scpiList.ToString().TrimEnd()
                : "No SCPI steps found in the test plan.";
        }

        /// <summary>
        /// Sanitizes SCPI commands by removing comments and extra whitespace.
        /// </summary>
        /// <param name="command">The SCPI command to sanitise.</param>
        /// <returns>The sanitised SCPI command.</returns>
        public static string SanitizeScpiCommand(string command)
        {
            if (string.IsNullOrEmpty(command))
                return string.Empty;
                
            // Remove any comments
            if (command.Contains("#"))
                command = command.Substring(0, command.IndexOf("#")).Trim();
            if (command.Contains("//"))
                command = command.Substring(0, command.IndexOf("//")).Trim();

            // Remove any extra whitespace
            return command.Trim();
        }
    }
}