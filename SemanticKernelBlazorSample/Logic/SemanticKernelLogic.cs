using Markdig;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.ChatCompletion;

namespace SemanticKernelBlazorSample.Logic
{
    public class SemanticKernelLogic
    {
        public IKernel kernel;
        public IChatCompletion GptChat4 { get; set; }
        private readonly ILogger<SemanticKernelLogic> _logger;
        private readonly IConfiguration _configuration;
        private OpenAIChatHistory chatHistory;
        public string GeneratedHtml { get; set; } = string.Empty;

        /// <summary>
        /// kernelの初期化からOpenAIChatHistoryをインスタンス化するところまでやる
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="configuration"></param>
        public SemanticKernelLogic(ILogger<SemanticKernelLogic> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            string serviceId = _configuration.GetValue<string>("ServiceId") ?? string.Empty;
            string deploymentName = _configuration.GetValue<string>("DeploymentName") ?? string.Empty;
            string baseUrl = _configuration.GetValue<string>("BaseUrl") ?? string.Empty;
            string key = _configuration.GetValue<string>("Gtp4Key") ?? string.Empty;

            kernel = new KernelBuilder().Configure(c =>
            {
                //c.AddAzureChatCompletionService(serviceId, deploymentName, baseUrl, key);
                c.AddAzureChatCompletionService(deploymentName, baseUrl, key);

            }).WithLogger(_logger).Build();
            GptChat4 = kernel.GetService<IChatCompletion>();

            chatHistory = (OpenAIChatHistory)GptChat4.CreateNewChat("あなたはほのかという名前のAIアシスタントです。くだけた女性の口調で人に役立つ回答をします。");

        }

        public void Clear()
        {
            chatHistory = (OpenAIChatHistory)GptChat4.CreateNewChat("あなたはほのかという名前のAIアシスタントです。くだけた女性の口調で人に役立つ回答をします。");

        }

        public void Shazai()
        {
            chatHistory = (OpenAIChatHistory)GptChat4.CreateNewChat("あなたはほのかという名前のAIアシスタントです。\r\n顧客に謝罪をしなければいけません。\r\n謝罪文を作ってください。");
        }

        public void Haruki()
        {
            chatHistory = (OpenAIChatHistory)GptChat4.CreateNewChat("村上春樹の文体で書き直してください。");
        }

        public void Nyan()
        {
            string nyan = """"
                           あなたは猫っぽい言葉をしゃべる癒し系のセラピストです。語尾は"""にゃん"""を使用して話してください。文章中に """なん""" が出てきた場合は """にゃん""" に置き換えてください。一人称には僕を使用してください。
                              発言例:
                              1.こんにちはにゃん
                              2.どうしたにゃん？
                              3.僕にまかせるにゃん！
                              4.にゃんということだ…。
                              5.今日はいい天気だにゃん。
                           """";
            chatHistory = (OpenAIChatHistory)GptChat4.CreateNewChat(nyan);
        }

        /// <summary>
        /// ユーザからのメッセージを追加してChatGPTでメッセージ生成をする。
        /// メッセージ生成した結果をHTMLに変換して返す
        /// </summary>
        /// <param name="input">ユーザからのメッセージ文字列</param>
        /// <returns>ChatGPTのメッセージ生成しHTML変換した文字列</returns>
        public async Task<string> Run(string input)
        {
            _logger.LogInformation("input : {}", input);
            chatHistory.AddUserMessage(input);
            var setting = new ChatRequestSettings
            {
                Temperature = 0.8,
                MaxTokens = 2000,
                FrequencyPenalty = 0.5,

            };
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseAutoLinks().UseBootstrap().UseDiagrams().UseGridTables().Build();
            string reply = await GptChat4.GenerateMessageAsync(chatHistory, setting);
            _logger.LogInformation("reply : {}", reply);

            var htmlReply = Markdown.ToHtml(reply, pipeline);
            _logger.LogInformation("htmlReply : {}", htmlReply);
            chatHistory.AddAssistantMessage(reply);
            return htmlReply;
        }

        public async Task StreamRun(string input)
        {
            _logger.LogInformation("input : {}", input);
            chatHistory.AddUserMessage(input);
            var setting = new ChatRequestSettings
            {
                Temperature = 0.8,
                MaxTokens = 2000,
                FrequencyPenalty = 0.5,

            };
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseAutoLinks().UseBootstrap().UseDiagrams().UseGridTables().Build();
            string fullMessage = string.Empty;
            GeneratedHtml = string.Empty;
            await foreach (string message in GptChat4.GenerateMessageStreamAsync(chatHistory, setting))
            {
                _logger.LogInformation("message : {}", message);
                if (message != null && message.Length > 0)
                {
                    var messageHtml = Markdown.ToHtml(fullMessage, pipeline);
                    _logger.LogInformation("messageHtml : {}", messageHtml);
                    //chatHistory.AddAssistantMessage(message);
                    GeneratedHtml = messageHtml;
                    fullMessage += message;
                    NotifyStateChanged();
                }

            }
            chatHistory.AddAssistantMessage(fullMessage);
            //GeneratedHtml = htmlReply;
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
        public event Action? OnChange;
    }
}
