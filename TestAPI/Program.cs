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

            const string collectionName = "ghc";

            var articleList = new List<string>
            {
                "https://docs.google.com/document/d/1DhvNyY2zoGTH4XQ8qJjZpX3pKFw7oi4o/edit?usp=sharing"
            };

            var htmlWeb = new HtmlWeb();
            foreach (var article in articleList)
            {
                // Convert to the export format URL
                var exportUrl = article.Replace("/edit", "/export?format=html");

                var htmlDoc = htmlWeb.Load(exportUrl); // Load the exported HTML content
                var node = htmlDoc.DocumentNode.Descendants()
                    .FirstOrDefault(n => n.Name == "body"); // You can adjust this depending on the content structure

                if (node != null)
                {
                    // Extract the text content from the body of the document
                    var documentContent = node.InnerText.Trim();

                    // Save to memory or database
                    memory.SaveInformationAsync(collectionName, documentContent, Guid.NewGuid().ToString()).Wait();
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

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()    // Allow any origin
                      .AllowAnyHeader()    // Allow any header
                      .AllowAnyMethod();   // Allow any HTTP method (GET, POST, etc.)
            });
        });

        builder.Services.AddSwaggerGen();
        // Add controllers
        builder.Services.AddControllers();

        var app = builder.Build();

        // Enable the "AllowAll" CORS policy globally
        app.UseCors("AllowAll");

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