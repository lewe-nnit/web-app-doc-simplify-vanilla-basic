using Azure;
using Azure.AI.OpenAI;
using Azure.AI.OpenAI.Chat;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using System.ClientModel;
using System.Text.Json;
using System.Threading.Tasks;

namespace NNIT.Veeva.Documents
{
    public class DocumentSearchSettings
    {
        public required string ChatModelDeploymentName { get; set; }
        public required string ChatModelApiKey { get; set; }
        public required string ChatModelApiEndpoint { get; set; }
        public required string ChatModelSystemMessage { get; set; }
    }

    public class DocumentSearch
    {
        private readonly ILogger<DocumentSearch> _logger;
        private readonly ChatClient _chatClient;
        private readonly string _chatModelSystemMessage;

        public DocumentSearch(ILogger<DocumentSearch> logger, IOptions<DocumentSearchSettings> settings)
        {
            var opts = settings.Value;
            _logger = logger;
            AzureOpenAIClient azureClient = new(
                new Uri(opts.ChatModelApiEndpoint),
                new AzureKeyCredential(opts.ChatModelApiKey));
            _chatClient = azureClient.GetChatClient(opts.ChatModelDeploymentName);

            _chatModelSystemMessage = opts.ChatModelSystemMessage;

            _logger.LogInformation("CONSTRUCTOR: documents_search instance created with chat endpoint: " + opts.ChatModelApiEndpoint);
        }

        [Function("documents_search")]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            _logger.LogInformation("START: documents_search function received a request.");

            // Read user input from request body
            string requestBody;
            using (var reader = new StreamReader(req.Body))
            {
                requestBody = await reader.ReadToEndAsync();
            }

            if (string.IsNullOrWhiteSpace(requestBody))
                return new BadRequestObjectResult("Empty request body.");

            var data = JsonSerializer.Deserialize<JsonElement>(requestBody);

            if (!data.TryGetProperty("userChatMessage", out JsonElement messageElement))
                return new BadRequestObjectResult("Missing required property: userChatMessage");

            string userChatMessage = messageElement.GetString() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(userChatMessage))
                return new BadRequestObjectResult("userChatMessage cannot be empty.");

            // Call the OpenAI chat completion API
            var completion = await _chatClient.CompleteChatAsync(new ChatMessage[]
            {
                new SystemChatMessage(_chatModelSystemMessage),
                new UserChatMessage(userChatMessage),
            });

            _logger.LogInformation("END: documents_search function completed a request.");
            return new OkObjectResult(completion);
        }
    }
}