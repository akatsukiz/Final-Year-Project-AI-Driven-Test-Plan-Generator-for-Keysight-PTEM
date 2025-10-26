# Final-Year-Project-AI-Driven-Test-Plan-Generator-for-Keysight-PTEM

An industrial project from Keysight Technologies. Developed an AI chatbox plugin for PTEM that allow test engineers to generate test plan through natural language query

![Dockable Panel Plugin for PTEM](https://github.com/akatsukiz/Final-Year-Project-AI-Driven-Test-Plan-Generator-for-Keysight-PTEM/blob/main/Screenshots/1.jpg)
Allow user to send query to AI using chatbox interface for generating test plan.

![AI-Generated Test Plan](https://github.com/akatsukiz/Final-Year-Project-AI-Driven-Test-Plan-Generator-for-Keysight-PTEM/blob/main/Screenshots/2.jpg)
AI responsed the test plan according to user's requirement with each test step detailed with step type, parameters and explanations. The test plan is also created automatically that is seamlessly integrated with PTEM and able to be executed directly.

![Auto Generated CSV File](https://github.com/akatsukiz/Final-Year-Project-AI-Driven-Test-Plan-Generator-for-Keysight-PTEM/blob/main/Screenshots/3.jpg)
CSV file is automatically generated after the test plan is executed by the user, it will detail each SCPI test step with its duration taken, action type, the query, and the responses gathered from instruments with its exact timestamp.

![Test Plan Optimization](https://github.com/akatsukiz/Final-Year-Project-AI-Driven-Test-Plan-Generator-for-Keysight-PTEM/blob/main/Screenshots/4.jpg)
It supports test plan optimization by feeding the context on current opened test plan along with user's prompt to make changes to current test plan.

![Optimized Test Plan](https://github.com/akatsukiz/Final-Year-Project-AI-Driven-Test-Plan-Generator-for-Keysight-PTEM/blob/main/Screenshots/5.jpg)
The previous basic test plan has been optimized and added with time guard test step to ensure the test plan executed within a given time frame and avoiding hanging behavior.

![Regex Pattern Verdict](https://github.com/akatsukiz/Final-Year-Project-AI-Driven-Test-Plan-Generator-for-Keysight-PTEM/blob/main/Screenshots/6.jpg)
It supports generating test plan with Regex Pattern verdict to fail or pass a test plan with a given condition.

![Regex Pattern Verdict Included in the Generated Test Plan](https://github.com/akatsukiz/Final-Year-Project-AI-Driven-Test-Plan-Generator-for-Keysight-PTEM/blob/main/Screenshots/7.jpg)
The Regex Pattern Verdict will ensure the test plan to be set failed if the response from instrument is lower than 3V.

![Test Plan Passed](https://github.com/akatsukiz/Final-Year-Project-AI-Driven-Test-Plan-Generator-for-Keysight-PTEM/blob/main/Screenshots/8.jpg)
The test plan result shows pass as the response retrieved is more than 3V.

![Multiple Instruments Supported](https://github.com/akatsukiz/Final-Year-Project-AI-Driven-Test-Plan-Generator-for-Keysight-PTEM/blob/main/Screenshots/9.jpg)
It supports generating test plan with multiple instruments by getting the context of connected instruments and user's requirements.

![Supports Data Logging](https://github.com/akatsukiz/Final-Year-Project-AI-Driven-Test-Plan-Generator-for-Keysight-PTEM/blob/main/Screenshots/10.jpg)
It supports data logging by logging the measurements with a given frequency within a time frame into CSV file.

![Data Logging Test Plan](https://github.com/akatsukiz/Final-Year-Project-AI-Driven-Test-Plan-Generator-for-Keysight-PTEM/blob/main/Screenshots/11.jpg)
The test plan is generated automatically, and user just have to click on the execute button and wait for the CSV output file.

![Data Logging Test Plan Execution](https://github.com/akatsukiz/Final-Year-Project-AI-Driven-Test-Plan-Generator-for-Keysight-PTEM/blob/main/Screenshots/12.jpg)
The test step is configured to log the reading data every 4.5 seconds for a total of 5 times.

![CSV Output of the Data Logging Test Plan](https://github.com/akatsukiz/Final-Year-Project-AI-Driven-Test-Plan-Generator-for-Keysight-PTEM/blob/main/Screenshots/13.jpg)
The data is logged into a CSV file with the details of each test steps to allow user to perform data analysis.

![Light/Dark Theme Supported](https://github.com/akatsukiz/Final-Year-Project-AI-Driven-Test-Plan-Generator-for-Keysight-PTEM/blob/main/Screenshots/14.jpg)
It supports both dark and light theme for the AI-Chatbox.
