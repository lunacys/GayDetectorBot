using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using GayDetectorBot.Models;

namespace GayDetectorBot.Data.Repos
{
    public class ParticipantRepository
    {
        private readonly DataContext _context;

        public ParticipantRepository(DataContext context)
        {
            _context = context;
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
                        GuildId = (ulong)cGuildId,
                        UserId = (ulong)userId,
                        StartedAt = DateTimeOffset.FromUnixTimeSeconds(startedAt).DateTime,
                        IsRemoved = isRemoved
                    });
                }
            }

            return result;
        }
    }
}