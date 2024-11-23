using System.Collections.Generic;
using System.Threading.Tasks;
using GayDetectorBot.Models;

namespace GayDetectorBot.Data.Repos
{
    public class CommandRepository
    {
        private readonly DataContext _context;

        public CommandRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CustomCommand>> RetrieveAllCommandsByGuildId(ulong guildId)
        {
            await using var conn = _context.CreateConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = SqlReader.Load("Command$RetrieveByGuildId");

            cmd.Parameters.AddWithValue("$GuildId", guildId);

            List<CustomCommand> commands = new List<CustomCommand>();

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    var commandId = reader.GetInt32(0);
                    var cGuildId = reader.GetDecimal(1);
                    var userAddedId = reader.GetDecimal(2);
                    var prefix = reader.GetString(3);
                    var content = reader.GetString(4);

                    commands.Add(new CustomCommand
                    {
                        CommandId = commandId,
                        GuildId = (ulong)cGuildId,
                        UserAddedId = (ulong)userAddedId,
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
                    var cGuildId = reader.GetDecimal(1);
                    var userAddedId = reader.GetDecimal(2);
                    var prefix = reader.GetString(3);
                    var content = reader.GetString(4);

                    commands.Add(new CustomCommand
                    {
                        CommandId = commandId,
                        GuildId = (ulong)cGuildId,
                        UserAddedId = (ulong)userAddedId,
                        CommandPrefix = prefix,
                        CommandContent = content
                    });
                }
            }

            return commands;
        }

        public async Task AddCommand(ulong guildId, ulong userId, string prefix, string content)
        {
            await using var conn = _context.CreateConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = SqlReader.Load("Command$Add");
            cmd.Parameters.AddWithValue("$GuildId", guildId);
            cmd.Parameters.AddWithValue("$UserId", userId);
            cmd.Parameters.AddWithValue("$Prefix", prefix);
            cmd.Parameters.AddWithValue("$Content", content);

            await using (var reader = await cmd.ExecuteReaderAsync())
            {

            }
        }

        public async Task DeleteCommand(string prefix, ulong guildId)
        {
            await using var conn = _context.CreateConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = SqlReader.Load("Command$Delete");
            cmd.Parameters.AddWithValue("$GuildId", guildId);
            cmd.Parameters.AddWithValue("$Prefix", prefix);

            await using (var reader = await cmd.ExecuteReaderAsync())
            {

            }
        }

        public async Task<bool> CommandExists(string prefix, ulong guildId)
        {
            await using var conn = _context.CreateConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = SqlReader.Load("Command$Exists");
            cmd.Parameters.AddWithValue("$GuildId", guildId);
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