using GayDetectorBot.Telegram.Data;
using GayDetectorBot.Telegram.Data.Repos;
using GayDetectorBot.Telegram.MessageHandling;
using GayDetectorBot.Telegram.Services;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using File = System.IO.File;

namespace GayDetectorBot.Telegram
{
    public class AppConfig
    {
        public string Token { get; set; } = null!;
        public string DbConnectionString { get; set; } = null!;
    }

    class Program
    {
        public static async Task Main(string[] args) => await new Program().Initialize();

        private AppConfig? _appConfig;
        private TelegramBotClient _telegramClient = null!;
        private MessageHandler _messageHandler = null!;
        private DataContext _dataContext = null!;
        private CommandRepository _commandRepository = null!;
        private GayRepository _gayRepository = null!;
        private ChatRepository _chatRepository = null!;
        private ParticipantRepository _participantRepository = null!;

        private async Task Initialize()
        {
            var appConfigFile = "appconfig.json";

            var env = Environment.GetEnvironmentVariable("ENVIRONMENT");

            if (env != null)
            {
                if (env == "Production")
                    appConfigFile = "appconfig.prod.json";
                else if (env == "Dev")
                    appConfigFile = "appconfig.dev.json";
            }

            Console.WriteLine($"USING FILE {appConfigFile}");

            using (var sr = new StreamReader(appConfigFile))
            {
                var text = await sr.ReadToEndAsync();
                _appConfig = JsonConvert.DeserializeObject<AppConfig>(text);
            }

            if (_appConfig == null)
                throw new Exception($"Could not load {appConfigFile}");

            _dataContext = new DataContext(_appConfig.DbConnectionString, "TgData.db");

            _dataContext.Initialize();

            _commandRepository = new CommandRepository(_dataContext);
            _gayRepository = new GayRepository(_dataContext);
            _chatRepository = new ChatRepository(_dataContext);
            _participantRepository = new ParticipantRepository(_dataContext);

            await DumpToFile();

            _messageHandler = new MessageHandler(_commandRepository, _gayRepository, _chatRepository, _participantRepository);
            
            await InitTelegram();

            await Task.Delay(-1);
        }

        private async Task DumpToFile()
        {
            // Dumping all data to a file
            var commands = await _commandRepository.RetrieveAllCommands();
            var gays = await _gayRepository.RetrieveAllGays();
            var chats = await _chatRepository.RetrieveAll();
            var participants = await _participantRepository.RetrieveAll();

            var cmdStr = "";
            foreach (var command in commands)
            {
                cmdStr += $"{{{command.CommandId}|{command.ChatId}|{command.UserAddedName}|{command.CommandPrefix}|{command.CommandContent}}}\n";
            }
            await File.WriteAllTextAsync("DB_Commands.txt", cmdStr);

            var gayStr = "";
            foreach (var gay in gays)
            {
                gayStr += $"{{{gay.GayId}|{gay.DateTimestamp:yyyy-MM-dd HH:mm:ss.ms}+05|{gay.Participant.ParticipantId}}}\n";
            }
            await File.WriteAllTextAsync("DB_Gays.txt", gayStr);

            var chatStr = "";
            foreach (var chat in chats)
            {
                chatStr += $"{{{chat.ChatInternalId}|{chat.ChatId}|{chat.LastGayUsername}|{chat.LastChecked:yyyy-MM-dd HH:mm:ss.ms}+05}}\n";
            }
            await File.WriteAllTextAsync("DB_Chats.txt", chatStr);

            var partStr = "";
            foreach (var part in participants)
            {
                partStr += $"{{{part.ParticipantId}|{part.ChatId}|{part.Username}|{part.StartedAt:yyyy-MM-dd HH:mm:ss.ms}+05|{part.IsRemoved}|{part.FirstName}|{part.LastName}}}\n";
            }
            await File.WriteAllTextAsync("DB_Participants.txt", partStr);
        }

        private async Task InitTelegram()
        {
            if (_appConfig == null)
                return;

            _telegramClient = new TelegramBotClient(_appConfig.Token.Trim());

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }
            };

            _telegramClient.StartReceiving(UpdateHandler, ErrorHandler, receiverOptions);

            var me = await _telegramClient.GetMeAsync();
            Console.WriteLine($"BOT DATA: {me.Id} {me.FirstName} {me.LastName}");

            Scheduler.Initialize();
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
            if (update.Type == UpdateType.InlineQuery && update.InlineQuery != null)
            {
                await InlineQueryHandler(update.InlineQuery, token);
            }

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

        private async Task InlineQueryHandler(InlineQuery inlineQuery, CancellationToken token)
        {
            InlineQueryResult[] results =
            {
                new InlineQueryResultArticle("1", "a", new InputTextMessageContent("hello"))
            };

            await _telegramClient.AnswerInlineQueryAsync(inlineQuery.Id, results, 0, true, cancellationToken: token);
        }
    }
}