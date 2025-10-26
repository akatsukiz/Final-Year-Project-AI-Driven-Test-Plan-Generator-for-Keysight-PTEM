using Microsoft.VisualStudio.TestTools.UnitTesting;
using ChatboxPlugin.UI.Processor;

namespace WhiteBoxTesting
{
    [TestClass]
    public class ResponseProcessorTests
    {
        private ResponseProcessor? _responseProcessor;

        [TestInitialize]
        public void TestInitialize()
        {
            _responseProcessor = new ResponseProcessor();
        }

        [TestMethod]
        public void ExtractJsonFromResponse_WithCodeBlock_ReturnsJsonFromCodeBlock()
        {
            // Arrange
            string aiResponse = @"Here's a test plan for measuring voltage:

                                ```json
                                {
                                  ""Steps"": [
                                    {
                                      ""StepOrder"": 1,
                                      ""StepType"": ""SCPI"",
                                      ""Parameters"": {
                                        ""Action"": ""Query"",
                                        ""Query"": ""MEAS:VOLT:DC?"",
                                        ""Instrument"": ""DMM""
                                      }
                                    }
                                  ],
                                  ""Explanation"": [
                                    ""This is a simple voltage measurement test plan.""
                                  ]
                                }
                                ```

                                You can load this into PTEM to execute the test.";

            // Act
            string extractedJson = _responseProcessor.ExtractJsonFromResponse(aiResponse);

            // Assert
            Assert.IsNotNull(extractedJson);
            Assert.IsTrue(extractedJson.Contains("\"StepType\": \"SCPI\""));
            Assert.IsTrue(extractedJson.Contains("\"Query\": \"MEAS:VOLT:DC?\""));
        }

        [TestMethod]
        public void ExtractJsonFromResponse_WithoutCodeBlock_ExtractsByKeys()
        {
            // Arrange
            string aiResponse = @"Here's a test plan for measuring voltage:

                                {
                                  ""Steps"": [
                                    {
                                      ""StepOrder"": 1,
                                      ""StepType"": ""SCPI"",
                                      ""Parameters"": {
                                        ""Action"": ""Query"",
                                        ""Query"": ""MEAS:VOLT:DC?"",
                                        ""Instrument"": ""DMM""
                                      }
                                    }
                                  ],
                                  ""Explanation"": [
                                    ""This is a simple voltage measurement test plan.""
                                  ]
                                }

                                You can load this into PTEM to execute the test.";

            // Act
            string extractedJson = _responseProcessor.ExtractJsonFromResponse(aiResponse);

            // Assert
            Assert.IsNotNull(extractedJson);
            Assert.IsTrue(extractedJson.Contains("\"StepType\": \"SCPI\""));
            Assert.IsTrue(extractedJson.Contains("\"Query\": \"MEAS:VOLT:DC?\""));
        }

        [TestMethod]
        public void ExtractJsonFromResponse_WithMultipleJsonBlocks_ExtractsCorrectOne()
        {
            // Arrange
            string aiResponse = @"First, let me show you a simple JSON structure:

                                {
                                  ""example"": ""This is just an example"",
                                  ""value"": 123
                                }

                                Now here's the actual test plan:

                                {
                                  ""Steps"": [
                                    {
                                      ""StepOrder"": 1,
                                      ""StepType"": ""SCPI"",
                                      ""Parameters"": {
                                        ""Action"": ""Query"",
                                        ""Query"": ""MEAS:VOLT:DC?"",
                                        ""Instrument"": ""DMM""
                                      }
                                    }
                                  ],
                                  ""Explanation"": [
                                    ""This is a simple voltage measurement test plan.""
                                  ]
                                }";

            // Act
            string extractedJson = _responseProcessor.ExtractJsonFromResponse(aiResponse);

            // Assert
            Assert.IsNotNull(extractedJson);
            Assert.IsTrue(extractedJson.Contains("\"Steps\""));
            Assert.IsTrue(extractedJson.Contains("\"Explanation\""));
            Assert.IsFalse(extractedJson.Contains("\"example\""));
        }

        [TestMethod]
        public void ParseAiResponseJson_WithValidJson_ReturnsTestPlanJsonObject()
        {
            // Arrange
            string jsonStr = @"{
                ""Steps"": [
                    {
                        ""StepOrder"": 1,
                        ""StepType"": ""SCPI"",
                        ""Parameters"": {
                            ""Action"": ""Query"",
                            ""Query"": ""MEAS:VOLT:DC?"",
                            ""Instrument"": ""DMM"" 
                        }
                    },
                    {
                        ""StepOrder"": 2,
                        ""StepType"": ""Delay"",
                        ""Parameters"": {
                            ""DelaySecs"": 1.0
                        }
                    }
                ],
                ""Explanation"": [
                    ""This test plan measures DC voltage after a 1-second delay.""
                ]
            }";

            // Act
            var testPlanJson = _responseProcessor.ParseAiResponseJson(jsonStr);

            // Assert
            Assert.IsNotNull(testPlanJson);
            Assert.AreEqual(2, testPlanJson.Steps.Count);
            
            Assert.AreEqual(1, testPlanJson.Steps[0].StepOrder);
            Assert.AreEqual("SCPI", testPlanJson.Steps[0].StepType);
            Assert.AreEqual("Query", testPlanJson.Steps[0].Parameters["Action"]);
            Assert.AreEqual("MEAS:VOLT:DC?", testPlanJson.Steps[0].Parameters["Query"]);
            
            Assert.AreEqual(2, testPlanJson.Steps[1].StepOrder);
            Assert.AreEqual("Delay", testPlanJson.Steps[1].StepType);
            Assert.AreEqual(1.0, (double)testPlanJson.Steps[1].Parameters["DelaySecs"]);
            
            Assert.AreEqual(1, testPlanJson.Explanation.Count);
            Assert.AreEqual("This test plan measures DC voltage after a 1-second delay.", testPlanJson.Explanation[0]);
        }

        [TestMethod]
        public void ParseAiResponseJson_WithTimeGuardStep_HandlesChildStepsCorrectly()
        {
            // Arrange
            string jsonStr = @"{
                ""Steps"": [
                    {
                        ""StepOrder"": 1,
                        ""StepType"": ""TimeGuard"",
                        ""Parameters"": {
                            ""Timeout"": 10.0,
                            ""StopOnTimeout"": true,
                            ""TimeoutVerdict"": ""Error""
                        },
                        ""ChildSteps"": [
                            {
                                ""StepOrder"": 1,
                                ""StepType"": ""SCPI"",
                                ""Parameters"": {
                                    ""Action"": ""Command"",
                                    ""Query"": ""OUTP ON"",
                                    ""Instrument"": ""Power Supply""
                                }
                            },
                            {
                                ""StepOrder"": 2,
                                ""StepType"": ""SCPI"",
                                ""Parameters"": {
                                    ""Action"": ""Query"",
                                    ""Query"": ""MEAS:VOLT:DC?"",
                                    ""Instrument"": ""DMM""
                                }
                            }
                        ]
                    }
                ],
                ""Explanation"": [
                    ""This test plan turns on a power supply and measures voltage with a DMM.""
                ]
            }";

            // Act
            var testPlanJson = _responseProcessor.ParseAiResponseJson(jsonStr);

            // Assert
            Assert.IsNotNull(testPlanJson);
            Assert.AreEqual(1, testPlanJson.Steps.Count);
            Assert.AreEqual("TimeGuard", testPlanJson.Steps[0].StepType);
            
            Assert.IsNotNull(testPlanJson.Steps[0].ChildSteps);
            Assert.AreEqual(2, testPlanJson.Steps[0].ChildSteps.Count);
            
            var childStep1 = testPlanJson.Steps[0].ChildSteps[0];
            Assert.AreEqual(1, childStep1.StepOrder);
            Assert.AreEqual("SCPI", childStep1.StepType);
            Assert.AreEqual("Command", childStep1.Parameters["Action"]);
            Assert.AreEqual("OUTP ON", childStep1.Parameters["Query"]);
            Assert.AreEqual("Power Supply", childStep1.Parameters["Instrument"]);
            
            var childStep2 = testPlanJson.Steps[0].ChildSteps[1];
            Assert.AreEqual(2, childStep2.StepOrder);
            Assert.AreEqual("SCPI", childStep2.StepType);
            Assert.AreEqual("Query", childStep2.Parameters["Action"]);
            Assert.AreEqual("MEAS:VOLT:DC?", childStep2.Parameters["Query"]);
            Assert.AreEqual("DMM", childStep2.Parameters["Instrument"]);
        }

        [TestMethod]
        public void ParseAiResponseJson_WithMissingSteps_ReturnsNull()
        {
            // Arrange
            string jsonStr = @"{
                ""Explanation"": [
                    ""This test plan measures DC voltage.""
                ]
            }";

            // Act
            var testPlanJson = _responseProcessor.ParseAiResponseJson(jsonStr);

            // Assert
            Assert.IsNull(testPlanJson);
        }

        [TestMethod]
        public void ParseAiResponseJson_WithMissingExplanation_CreatesDefaultExplanations()
        {
            // Arrange
            string jsonStr = @"{
                ""Steps"": [
                    {
                        ""StepOrder"": 1,
                        ""StepType"": ""SCPI"",
                        ""Parameters"": {
                            ""Action"": ""Query"",
                            ""Query"": ""MEAS:VOLT:DC?"",
                            ""Instrument"": ""DMM""
                        }
                    }
                ]
            }";

            // Act
            var testPlanJson = _responseProcessor.ParseAiResponseJson(jsonStr);

            // Assert
            Assert.IsNotNull(testPlanJson);
            Assert.IsNotNull(testPlanJson.Explanation);
            Assert.IsTrue(testPlanJson.Explanation.Count > 0);
            // Default explanations typically include the step count
            Assert.IsTrue(testPlanJson.Explanation[0].Contains("1"));
        }
    }
} 