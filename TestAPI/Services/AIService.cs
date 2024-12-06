namespace AIComunicateAPI.Services
{
    using Microsoft.SemanticKernel;
    using System.Text;
    using TestAPI.Models;

    public class AIService
    {
        private readonly TextGenerateKernel textGenerateKernel;
        private readonly ImageProcessorKernel imageProcessorKernel;

        public AIService(TextGenerateKernel kernel, ImageProcessorKernel imageProcessorKernel)
        {
            this.textGenerateKernel = kernel;
            this.imageProcessorKernel = imageProcessorKernel;
        }

        public async Task<string> GenerateTextAsync(string message)
        {
            const string collectionName = "ghc-news";
            var textGenerated = new StringBuilder();

            // Set up the prompt for Semantic Kernel
            var prompt = this.GetDermatologyPrompt();

            var arguments = new KernelArguments
            {
                { "input", message },
                { "collection", collectionName }
            };

            var response = this.textGenerateKernel.InvokePromptStreamingAsync(prompt, arguments);
            await foreach (var result in response)
            {
                textGenerated.Append(result.ToString());
            }

            return textGenerated.ToString();
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

        private string GetDermatologyPrompt()
        {
            return @"
                You are an AI model specialized in dermatological disease prediction based on image analysis and related information. 
                Use your knowledge of skin conditions, diseases, and visual features.
        
                Question: {{$input}}
        
                Answer: Provide a detailed and medically-informed response based on the memory content: {{Recall}}.
        
                If you cannot determine the condition confidently or the question is unrelated to dermatological diseases, respond with: 
                'I don't know! Please consult a qualified dermatologist for further assistance.'";
        }
    }
}
