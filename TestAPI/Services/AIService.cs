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
        public int count = 0;

        public AIService(TextGenerateKernel kernel, ImageProcessorKernel imageProcessorKernel)
        {
            this.textGenerateKernel = kernel;
            this.imageProcessorKernel = imageProcessorKernel;
        }

        public async Task<string> GenerateTextAsync(string message)
        {
            inputFromUser = message;
            var textGenerated = new StringBuilder();
            // Generate the prompt
            var prompt = $@"
            You are an AI model specialized in dermatological disease prediction based on text analysis and related information. 
            Use your knowledge of skin conditions, diseases, and visual features. Summary in 100 words
            Question: {inputFromUser}
            Answer the question using the memory content: {{{{Recall}}}}
            Is this information enough for dermatological disease prediction based on the description?
            If yes, return 'Answer: Yes; Details: [AI response]'. 
            If no, return 'Answer: No; Details: [List of further questions needed]'.
            For example : Answer: Yes; Details: You are shaking
            For example : Answer: No; Details: How long?";

            var arguments = new KernelArguments
            {
                { "input", inputFromUser },
                { "collection", "ghc" } // Adjust collection name as needed
            };

            // Call the AI and parse the response
            var responseFromAI = this.textGenerateKernel.InvokePromptStreamingAsync(prompt, arguments);
            await foreach (var result in responseFromAI)
            {
                textGenerated.Append(result.ToString());
                Console.WriteLine(result.ToString());
            }
            var dictionaryFromResponse = ParseStringToDictionary(textGenerated.ToString());
            string answer = dictionaryFromResponse["Answer"];
            var isSucess = dictionaryFromResponse.TryGetValue("Details", out var details);

            // Check AI's response
            if (answer.Equals("Yes", StringComparison.OrdinalIgnoreCase) && isSucess)
            {
                inputFromUser = string.Empty;
                return $"{string.Join(", ", details)}";
            }
            else if (answer.Equals("No", StringComparison.OrdinalIgnoreCase) && count < 5 && isSucess)
            {
                count++;
                inputFromUser = $"{string.Join(", ", message)}. {details}";
                return details;
            }
            else
            {
                inputFromUser = string.Empty;
                return $"I don't know!.";
            }

            return "I don't know!.";
        }

        static Dictionary<string, string> ParseStringToDictionary(string input)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();

            // Split the input string by commas to get individual key-value pairs
            string[] pairs = input.Split(';');

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
