using Markdig;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.ChatCompletion;

namespace SemanticKernelBlazorSample.Logic
{
    public class SementicKernelLogic
    {
        public IKernel kernel;
        public IChatCompletion GtpChat4 { get; set; }
        private readonly ILogger<SementicKernelLogic> _logger;
        private readonly IConfiguration _configuration;
        private readonly OpenAIChatHistory chatHistory;
        public SementicKernelLogic(ILogger<SementicKernelLogic> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            string baseUrl = _configuration.GetValue<string>("BaseUrl") ?? string.Empty;
            string key = _configuration.GetValue<string>("Gtp4Key") ?? string.Empty;

            kernel = new KernelBuilder().Configure(c =>
            {
                c.AddAzureChatCompletionService("gpt-4", "Gtp-4", baseUrl, key);
                
            }).WithLogger(_logger).Build();
            GtpChat4 = kernel.GetService<IChatCompletion>();
            
            chatHistory = (OpenAIChatHistory)GtpChat4.CreateNewChat();
            
        }
        public async Task<string> Run(string input)
        {
            chatHistory.AddUserMessage(input);
            var setting = new ChatRequestSettings
            {
                Temperature = 0.8,
                MaxTokens = 2000,
                FrequencyPenalty = 0.5
            };
            string reply = await GtpChat4.GenerateMessageAsync(chatHistory,setting);
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseAutoLinks().UseBootstrap().UseDiagrams().UseGridTables().Build();
            var htmlReply = Markdown.ToHtml(reply, pipeline);
            chatHistory.AddAssistantMessage(reply);
            return htmlReply;
        }
    }
}
