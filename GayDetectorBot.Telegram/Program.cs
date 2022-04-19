using GayDetectorBot.Telegram.Data;
using GayDetectorBot.Telegram.Data.Repos;
using GayDetectorBot.Telegram.MessageHandling;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GayDetectorBot.Telegram
{
    public class AppConfig
    {
        public string Token { get; set; }
        public string DbConnectionString { get; set; }
    }

    class Program
    {
        public static async Task Main(string[] args) => await new Program().Initialize();

        private AppConfig _appConfig;
        private TelegramBotClient _telegramClient;
        private MessageHandler _messageHandler;
        private DataContext _dataContext;
        private CommandRepository _commandRepository;
        private GayRepository _gayRepository;
        private ChatRepository _chatRepository;
        private ParticipantRepository _participantRepository;

        private async Task Initialize()
        {
            using (var sr = new StreamReader("appconfig.json"))
            {
                var text = await sr.ReadToEndAsync();
                _appConfig = JsonConvert.DeserializeObject<AppConfig>(text);
            }

            if (_appConfig == null)
                throw new Exception("Could not load appconfig.json");

            _dataContext = new DataContext(_appConfig.DbConnectionString, "TgData.db");

            _dataContext.Initialize(false);

            _commandRepository = new CommandRepository(_dataContext);
            _gayRepository = new GayRepository(_dataContext);
            _chatRepository = new ChatRepository(_dataContext);
            _participantRepository = new ParticipantRepository(_dataContext);

            _messageHandler = new MessageHandler(_commandRepository, _gayRepository, _chatRepository, _participantRepository);
            
            await InitTelegram();

            await Task.Delay(-1);
        }

        private async Task InitTelegram()
        {
            _telegramClient = new TelegramBotClient(_appConfig.Token);

            //await client2.SendTextMessageAsync(chatId, "гоша лох");

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }
            };

            _telegramClient.StartReceiving(UpdateHandler, ErrorHandler, receiverOptions);

            var me = await _telegramClient.GetMeAsync();
            Console.WriteLine($"TELEGRAM: {me.Id} {me.FirstName}");
        }

        private Task ErrorHandler(ITelegramBotClient client, Exception exception, CancellationToken token)
        {
            var errorMessage = exception switch
            {
                ApiRequestException api => $"Telegram API Error:\n{api.ErrorCode}\n{api.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(errorMessage);

            return Task.CompletedTask;
        }

        private async Task UpdateHandler(ITelegramBotClient client, Update update, CancellationToken token)
        {
            if (update.Type != UpdateType.Message)
                return;

            if (update.Message!.Type != MessageType.Text)
                return;

            var chatId = update.Message.Chat.Id;
            var text = update.Message.Text;
            var from = update.Message.From;

            Console.WriteLine($"Received a '{text}' message in chat {chatId} from {from?.FirstName} {from?.LastName} @{from?.Username}");

            await _messageHandler.Message(update.Message, client);
        }
    }
}