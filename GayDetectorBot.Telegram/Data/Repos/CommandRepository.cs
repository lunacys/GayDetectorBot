using GayDetectorBot.Telegram.Models;

namespace GayDetectorBot.Telegram.Data.Repos
{
    public class CommandRepository
    {
        private readonly DataContext _context;

        public CommandRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CustomCommand>> RetrieveAllCommandsByChatId(long chatId)
        {
            await using var conn = _context.CreateConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = SqlReader.Load("Command$RetrieveByChatId");

            cmd.Parameters.AddWithValue("$ChatId", chatId);

            List<CustomCommand> commands = new List<CustomCommand>();

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    var commandId = reader.GetInt32(0);
                    var chId = reader.GetInt64(1);
                    var userAddedName = reader.GetString(2);
                    var prefix = reader.GetString(3);
                    var content = reader.GetString(4);

                    commands.Add(new CustomCommand
                    {
                        CommandId = commandId,
                        ChatId = chId,
                        UserAddedName = userAddedName,
                        CommandPrefix = prefix,
                        CommandContent = content
                    });
                }
            }

            return commands;
        }

        public async Task<IEnumerable<CustomCommand>> RetrieveAllCommands()
        {
            await using var conn = _context.CreateConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = SqlReader.Load("Command$RetrieveAll");

            List<CustomCommand> commands = new List<CustomCommand>();

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    var commandId = reader.GetInt32(0);
                    var chId = reader.GetInt64(1);
                    var userAddedName = reader.GetString(2);
                    var prefix = reader.GetString(3);
                    var content = reader.GetString(4);

                    commands.Add(new CustomCommand
                    {
                        CommandId = commandId,
                        ChatId = chId,
                        UserAddedName = userAddedName,
                        CommandPrefix = prefix,
                        CommandContent = content
                    });
                }
            }

            return commands;
        }

        public async Task<CustomCommand> RetrieveByPrefix(string prefix, long chatId)
        {
            await using var conn = _context.CreateConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = SqlReader.Load("Command$RetrieveByPrefix");

            cmd.Parameters.AddWithValue("$ChatId", chatId);
            cmd.Parameters.AddWithValue("$Prefix", prefix);

            await using var reader = await cmd.ExecuteReaderAsync();

            var cmdId = reader.GetInt32(0);
            var chId = reader.GetInt64(1);
            var userAddedName = reader.GetString(2);
            var p = reader.GetString(3);
            var content = reader.GetString(4);

            return new CustomCommand
            {
                CommandId = cmdId,
                ChatId = chId,
                UserAddedName = userAddedName,
                CommandPrefix = p,
                CommandContent = content
            };
        }

        public async Task AddCommand(long chatId, string username, string prefix, string content)
        {
            await using var conn = _context.CreateConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = SqlReader.Load("Command$Add");
            cmd.Parameters.AddWithValue("$ChatId", chatId);
            cmd.Parameters.AddWithValue("$Username", username);
            cmd.Parameters.AddWithValue("$Prefix", prefix);
            cmd.Parameters.AddWithValue("$Content", content);

            await using (var reader = await cmd.ExecuteReaderAsync())
            {

            }
        }

        public async Task DeleteCommand(string prefix, long chatId)
        {
            await using var conn = _context.CreateConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = SqlReader.Load("Command$Delete");
            cmd.Parameters.AddWithValue("$ChatId", chatId);
            cmd.Parameters.AddWithValue("$Prefix", prefix);

            await using (var reader = await cmd.ExecuteReaderAsync())
            {

            }
        }

        public async Task<bool> CommandExists(string prefix, long chatId)
        {
            await using var conn = _context.CreateConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = SqlReader.Load("Command$Exists");
            cmd.Parameters.AddWithValue("$ChatId", chatId);
            cmd.Parameters.AddWithValue("$Prefix", prefix);

            int? commandId = null;

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    commandId = reader.GetInt32(0);
                }
            }

            return commandId.HasValue;
        }
    }
}