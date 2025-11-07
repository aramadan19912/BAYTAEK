using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;

namespace HomeService.Infrastructure.AI.Services;

public class SemanticKernelService
{
    private readonly Kernel _kernel;

    public SemanticKernelService(IConfiguration configuration)
    {
        var builder = Kernel.CreateBuilder();

        // Configure OpenAI or Azure OpenAI
        var useAzure = configuration.GetValue<bool>("SemanticKernel:UseAzureOpenAI");

        if (useAzure)
        {
            var endpoint = configuration["SemanticKernel:AzureOpenAI:Endpoint"]
                ?? throw new InvalidOperationException("Azure OpenAI endpoint not configured");
            var apiKey = configuration["SemanticKernel:AzureOpenAI:ApiKey"]
                ?? throw new InvalidOperationException("Azure OpenAI API key not configured");
            var deploymentName = configuration["SemanticKernel:AzureOpenAI:DeploymentName"]
                ?? throw new InvalidOperationException("Azure OpenAI deployment name not configured");

            builder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);
        }
        else
        {
            var apiKey = configuration["SemanticKernel:OpenAI:ApiKey"]
                ?? throw new InvalidOperationException("OpenAI API key not configured");
            var modelId = configuration["SemanticKernel:OpenAI:ModelId"] ?? "gpt-4";

            builder.AddOpenAIChatCompletion(modelId, apiKey);
        }

        _kernel = builder.Build();
    }

    public Kernel GetKernel() => _kernel;
}
