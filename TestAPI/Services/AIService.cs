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
            var prompt = @"
            Question: {{$input}}
            Answer the question using the memory content: {{Recall}}
            If you don't know an answer, say 'I don't know!'";

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
    }
}
