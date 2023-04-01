using Microsoft.AspNetCore.Mvc;
using OpenAI;
using OpenAI.Chat;

namespace NostrBot.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatAiController : ControllerBase
    {
        private readonly ILogger<ChatAiController> _logger;

        public ChatAiController(ILogger<ChatAiController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<string> Get(string message)
        {
            var api = new OpenAIClient(new OpenAIAuthentication("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", "org-JKDLHSXJ4YuYxvYp5qylR0Ty"));
            //var models = await api.ModelsEndpoint.GetModelsAsync();
            //foreach (var model in models)
            //{
            //    _logger.LogInformation("Available model: {model}", model.ToString());
            //}

            var chatPrompts = new List<ChatPrompt>
            {
                new("user", message)
            };

            var chatRequest = new ChatRequest(chatPrompts);
            var result = await api.ChatEndpoint.GetCompletionAsync(chatRequest);

            var response = string.Join(Environment.NewLine, result.Choices.Select(x => x.Message.Content));
            return response;
        }
    }
}