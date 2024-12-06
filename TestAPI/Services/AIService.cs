namespace AIComunicateAPI.Services
{
    using Microsoft.SemanticKernel;
    using System.Text;
    using TestAPI.Models;

    public class AIService
    {
        private readonly TextGenerateKernel textGenerateKernel;
        private readonly ImageProcessorKernel imageProcessorKernel;
        public string inputFromUser;

        public AIService(TextGenerateKernel kernel, ImageProcessorKernel imageProcessorKernel)
        {
            this.textGenerateKernel = kernel;
            this.imageProcessorKernel = imageProcessorKernel;
        }

        public async Task<string> GenerateTextAsync(string message)
        {
            inputFromUser = message;
            const string collectionName = "ghc-news";
            var textGenerated = new StringBuilder();
            do
            {
                // Generate the prompt
                var prompt = $@"
                You are an AI model specialized in dermatological disease prediction based on text analysis and related information. 
                Use your knowledge of skin conditions, diseases, and visual features.
                Question: {inputFromUser}
                Is this information enough for dermatological disease prediction based on the description?
                If yes, return 'Answer: Yes, Details: [AI response]'. 
                If no, return 'Answer: No, Details: [List of further questions needed]'.
                For example : Answer: Yes, Details: You are shaking
                For example : Answer: No, Details: How long?";

                var arguments = new Dictionary<string, string>
                {
                    { "input", inputFromUser },
                    { "collection", "ghc-images" } // Adjust collection name as needed
                };

                // Call the AI and parse the response
                var responseFromAI = this.imageProcessorKernel.InvokePromptStreamingAsync(prompt, arguments);
                await foreach (var result in responseFromAI)
                {
                    textGenerated.Append(result.ToString());
                }
                var dictionaryFromResponse = ParseStringToDictionary(textGenerated.ToString());
                string answer = dictionaryFromResponse["Answer"];
                string details = dictionaryFromResponse["Details"];

                // Check AI's response
                if (answer.Equals("Yes", StringComparison.OrdinalIgnoreCase))
                {
                    IsBreak = true;
                    inputFromUser = string.Empty;
                    return $"{string.Join(", ", details)}";
                }
                else if (answer.Equals("No", StringComparison.OrdinalIgnoreCase) && count < MaxAttempts)
                {
                    count++;
                    inputFromUser = $"{string.Join(", ", message)}. {details}";
                }
                else
                {
                    inputFromUser = string.Empty;
                    return $"I don't know!. \nAll Questions Asked: {string.Join(", ", details)}";
                }

            } while (!IsBreak);

            return "Unexpected exit from loop.";
        }

        public async Task<string> ImageProcessorAsync(string message)
        {
            const string collectionName = "ghc-images";
            var textGenerated = new StringBuilder();

            // Set up the prompt for Semantic Kernel
            var prompt = @"
            Question: {{$input}}
            If you don't know an answer, say 'I don't know!'";

            var arguments = new KernelArguments
            {
                { "input", message },
                { "collection", collectionName }
            };

            var response = this.imageProcessorKernel.InvokePromptStreamingAsync(prompt, arguments);
            await foreach (var result in response)
            {
                textGenerated.Append(result.ToString());
            }

            return textGenerated.ToString();
        }

        static Dictionary<string, string> ParseStringToDictionary(string input)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();

            // Split the input string by commas to get individual key-value pairs
            string[] pairs = input.Split(',');

            foreach (string pair in pairs)
            {
                // Trim spaces and split by the first colon to separate key and value
                string[] keyValue = pair.Trim().Split(':');

                if (keyValue.Length == 2)
                {
                    // Add the key-value pair to the dictionary
                    dictionary[keyValue[0].Trim()] = keyValue[1].Trim();
                }
            }

            return dictionary;
        }

    }
}
