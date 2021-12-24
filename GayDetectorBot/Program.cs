using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using GayDetectorBot.Data;
using GayDetectorBot.Data.Repos;
using GayDetectorBot.MessageHandlers;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;

namespace GayDetectorBot
{
    public class AppConfig
    {
        public string Token { get; set; }
        public string DbConnectionString { get; set; }
    }

    public class Program
    {
        public static async Task Main(string[] args) => await new Program().MainAsync();

        private DiscordSocketClient _client;
        private AppConfig _appConfig;
        private MessageHandler _messageHandler;
        private DataContext _dataContext;


        private CommandRepository _commandRepository;
        private GayRepository _gayRepository;
        private GuildRepository _guildRepository;
        private ParticipantRepository _participantRepository;

        private async Task MainAsync()
        {
            using (var sr = new StreamReader("appconfig.json"))
            {
                var text = await sr.ReadToEndAsync();
                _appConfig = JsonConvert.DeserializeObject<AppConfig>(text);
            }

            _client = new DiscordSocketClient();

            _dataContext = new DataContext(_appConfig.DbConnectionString, "Data.db");

            _dataContext.Initialize(false);

            _commandRepository = new CommandRepository(_dataContext);
            _gayRepository = new GayRepository(_dataContext);
            _guildRepository = new GuildRepository(_dataContext);
            _participantRepository = new ParticipantRepository(_dataContext);

            //_userRepository.Initialize();

            _messageHandler = new MessageHandler(_commandRepository, _gayRepository, _guildRepository, _participantRepository);

            _client.Log += Log;

            var token = _appConfig.Token;

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await _client.SetActivityAsync(new Game("!помоги"));

            _client.MessageReceived += async message =>
            {
                if (message.Author.IsBot)
                    return;

                await _messageHandler.Message(message);
            };

            _client.Connected += async () =>
            {
                await Task.CompletedTask;
            };

            _client.LoggedIn += async () =>
            {
                await Task.CompletedTask;
            };

            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
