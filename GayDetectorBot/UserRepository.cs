using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord;
using GayDetectorBot.Models;
using Microsoft.Data.Sqlite;

namespace GayDetectorBot
{
    public class UserRepository
    {
        private readonly DataContext _context;

        public static readonly string SchemaFilename = "Schema";

        public UserRepository(DataContext context)
        {
            _context = context;
        }

        public void Initialize(bool forceDrop = false)
        {
            Console.WriteLine("  [DB]: Initializing User Repository");

            if (forceDrop && File.Exists(_context.DbName))
                File.Delete(_context.DbName);

            if (!File.Exists(_context.DbName))
            {
                Console.WriteLine("  [DB]: Database doesn't exist, initializing...");
                
                using var conn = _context.CreateConnection();
                conn.Open();

                var cmd = conn.CreateCommand();
                cmd.CommandText = SqlReader.Load(SchemaFilename);

                using (var reader = cmd.ExecuteReader())
                { }

                Console.WriteLine("  [DB]: Database initialization done");
            }
            else
            {
                Console.WriteLine("  [DB]: Database already exists, skipping initialization");
            }
        }

        public async Task<bool> IsStartedForUser(ulong userId, ulong guildId)
        {
            await using var conn = _context.CreateConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = SqlReader.Load("Participant$Retrieve");
            cmd.Parameters.AddWithValue("$GuildId", guildId);
            cmd.Parameters.AddWithValue("$UserId", userId);

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

        public async Task AddUser(IUser user, ulong guildId)
        {
            await using var conn = _context.CreateConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = SqlReader.Load("Participant$Add");
            cmd.Parameters.AddWithValue("$GuildId", guildId);
            cmd.Parameters.AddWithValue("$UserId", user.Id);
            cmd.Parameters.AddWithValue("$StartedAt", DateTimeOffset.Now.ToUnixTimeSeconds());

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                { }
            }
        }

        public async Task RemoveUser(ulong guildId, ulong userId)
        {
            await using var conn = _context.CreateConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = SqlReader.Load("Participant$Remove");
            cmd.Parameters.AddWithValue("$GuildId", guildId);
            cmd.Parameters.AddWithValue("$UserId", userId);

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                { }
            }
        }

        public async Task<bool> GuildExists(ulong guildId)
        {
            await using var conn = _context.CreateConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = SqlReader.Load("Guild$GetLastChecked");
            cmd.Parameters.AddWithValue("$GuildId", guildId);

            long? lastChecked = null;

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    lastChecked = reader.GetInt64(0);
                }
            }

            return lastChecked.HasValue;
        }


        public async Task<DateTimeOffset?> GuildLastChecked(ulong guildId)
        {
            await using var conn = _context.CreateConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = SqlReader.Load("Guild$GetLastChecked");
            cmd.Parameters.AddWithValue("$GuildId", guildId);

            DateTimeOffset? lastChecked = null;

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    var i = reader.GetInt64(0);

                    lastChecked = DateTimeOffset.FromUnixTimeSeconds(i).ToLocalTime();
                }
            }

            return lastChecked;
        }

        public async Task<ulong?> GetLastGay(ulong guildId)
        {
            await using var conn = _context.CreateConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = SqlReader.Load("Guild$GetLastGay");
            cmd.Parameters.AddWithValue("$GuildId", guildId);

            ulong? lastGay = null;

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    if (reader.IsDBNull(0))
                    {
                        lastGay = null;
                    }
                    else
                    {
                        var i = reader.GetDecimal(0);

                        lastGay = (ulong)i;
                    }
                }
            }

            return lastGay;
        }

        public async Task GuildUpdate(ulong guildId, ulong userId)
        {
            await using var conn = _context.CreateConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = SqlReader.Load("Guild$Update");
            cmd.Parameters.AddWithValue("$GuildId", guildId);
            cmd.Parameters.AddWithValue("$LastChecked", DateTimeOffset.Now.ToUnixTimeSeconds());
            cmd.Parameters.AddWithValue("$LastGay", userId);


            /*
             * 	LastChecked = $LastChecked,
	LastGayUserId = $LastGay
WHERE GuildId = $GuildId
             */

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                { }
            }
        }

        public async Task GuildAdd(ulong guildId, DateTimeOffset? dateTime, ulong lastGayUserId)
        {
            await using var conn = _context.CreateConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = SqlReader.Load("Guild$Add");
            cmd.Parameters.AddWithValue("$GuildId", guildId);
            cmd.Parameters.AddWithValue("$LastChecked", dateTime?.ToUnixTimeSeconds());
            cmd.Parameters.AddWithValue("$LastGayUserId", lastGayUserId);

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                { }
            }
        }

        public async Task<IEnumerable<Participant>> RetrieveParticipants(ulong guildId)
        {
            await using var conn = _context.CreateConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = SqlReader.Load("Participant$RetrieveForGuild");

            cmd.Parameters.AddWithValue("$GuildId", guildId);

            var result = new List<Participant>();

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    var partId = reader.GetInt32(0);
                    var cGuildId = reader.GetDecimal(1);
                    var userId = reader.GetDecimal(2);
                    var startedAt = reader.GetInt64(3);
                    bool isRemoved = false;
                    if (!reader.IsDBNull(4))
                        isRemoved = reader.GetBoolean(4);

                    result.Add(new Participant()
                    {
                        ParticipantId = partId,
                        GuildId = (ulong) cGuildId,
                        UserId = (ulong) userId,
                        StartedAt = DateTimeOffset.FromUnixTimeSeconds(startedAt).DateTime,
                        IsRemoved = isRemoved
                    });
                }
            }

            return result;
        }

        public async Task AddGay(Participant participant)
        {
            await using var conn = _context.CreateConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = SqlReader.Load("Gays$Add");

            cmd.Parameters.AddWithValue("$DateTimestamp", DateTimeOffset.Now.ToUnixTimeSeconds());
            cmd.Parameters.AddWithValue("$ParticipantId", participant.ParticipantId);

            await using (var reader = await cmd.ExecuteReaderAsync())
            {

            }
        }

        public async Task<IEnumerable<Gay>> RetrieveGays(ulong guildId)
        {
            await using var conn = _context.CreateConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = SqlReader.Load("Gays$RetrieveForGuild");

            cmd.Parameters.AddWithValue("$GuildId", guildId);

            var list = new List<Gay>();

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    var gayId = reader.GetInt32(0);
                    var ts = reader.GetInt64(1);
                    var partId = reader.GetInt32(2);

                    var gId = (ulong) reader.GetDecimal(3);
                    var uId = (ulong)reader.GetDecimal(4);
                    var startedAt = reader.GetInt64(5);
                    bool isRemoved = false;
                    if (!reader.IsDBNull(6))
                        isRemoved = reader.GetBoolean(6);

                    var part = new Participant
                    {
                        ParticipantId = partId,
                        GuildId = gId,
                        UserId = uId,
                        StartedAt = DateTimeOffset.FromUnixTimeSeconds(ts).DateTime,
                        IsRemoved = isRemoved
                    };

                    var gay = new Gay
                    {
                        GayId = gayId,
                        DateTimestamp = DateTimeOffset.FromUnixTimeSeconds(startedAt).DateTime,
                        Participant = part
                    };

                    list.Add(gay);
                }
            }

            return list;
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
                        GuildId = (ulong) cGuildId,
                        UserAddedId = (ulong) userAddedId,
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