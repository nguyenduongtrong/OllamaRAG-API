#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0003
#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0011
#pragma warning disable SKEXP0050
#pragma warning disable SKEXP0052

using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Memory;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using AIComunicateAPI.Services;
using TestAPI.Models;

public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Register necessary services for Semantic Kernel
        builder.Services.AddSingleton(serviceProvider =>
        {
            var kernelBuilder = Kernel.CreateBuilder();
            kernelBuilder.AddOpenAIChatCompletion(
                modelId: "phi3.5",
                endpoint: new Uri("http://localhost:11434"),
                apiKey: "apikey"
            );
            kernelBuilder.AddLocalTextEmbeddingGeneration();
            var kernel = kernelBuilder.Build();

            // Get the embeddings generator service
            var embeddingGenerator = kernel.Services.GetRequiredService<ITextEmbeddingGenerationService>();
            var memory = new SemanticTextMemory(new VolatileMemoryStore(), embeddingGenerator);

            const string collectionName = "ghc-news";

            // Add articles to memory
            var articleList = new List<string>
            {
                "https://www.linkedin.com/posts/fsoft-ghc_telehealth-telehealth-digitalhealth-activity-7267371560036896768-YIdq",
                "https://www.linkedin.com/posts/fsoft-ghc_techday2024-digitalhealthcare-telehealth-activity-7265257966428184577-b9sl",
                "https://www.linkedin.com/posts/fsoft-ghc_ai-ai-data-activity-7264110087042920449--uqE"
            };

            var htmlWeb = new HtmlWeb();
            foreach (var article in articleList)
            {
                var htmlDoc = htmlWeb.Load(article);
                var node = htmlDoc.DocumentNode.Descendants(0)
                    .FirstOrDefault(n => n.HasClass("attributed-text-segment-list__content"));
                if (node != null)
                {
                    memory.SaveInformationAsync(collectionName, node.InnerText, Guid.NewGuid().ToString()).Wait();
                }
            }

            // Import the text memory plugin into the Kernel.
            kernel.ImportPluginFromObject(new TextMemoryPlugin(memory), "memory");

            return new TextGenerateKernel(kernel);
        });

        // Register necessary services for Semantic Kernel
        builder.Services.AddSingleton(serviceProvider =>
        {
            var kernelBuilder = Kernel.CreateBuilder();
            kernelBuilder.AddOpenAIChatCompletion(
                modelId: "llava",
                endpoint: new Uri("http://localhost:11434"),
                apiKey: "apikey"
            );
            kernelBuilder.AddLocalTextEmbeddingGeneration();
            var kernel = kernelBuilder.Build();

            return new ImageProcessorKernel(kernel);
        });

        builder.Services.AddSingleton<AIService>();

        builder.Services.AddSwaggerGen();
        // Add controllers
        builder.Services.AddControllers();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapControllers();

        app.Run();
    }
}