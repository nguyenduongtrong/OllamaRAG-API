namespace AIComunicateAPI.Services
{
    using Azure.Core;
    using HtmlAgilityPack;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.SemanticKernel;
    using Microsoft.SemanticKernel.Embeddings;
    using Microsoft.SemanticKernel.Memory;
    using Microsoft.SemanticKernel.Plugins.Memory;
    using System;
    using System.Text;

    public class AIService
    {
        private readonly Kernel kernel;

        public AIService(Kernel kernel)
        {
            this.kernel = kernel;
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

            var response = this.kernel.InvokePromptStreamingAsync(prompt, arguments);
            await foreach (var result in response)
            {
                textGenerated.Append(result.ToString());
            }

            return textGenerated.ToString();
        }
    }
}
