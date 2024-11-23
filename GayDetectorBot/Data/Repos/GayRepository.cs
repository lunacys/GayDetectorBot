using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GayDetectorBot.Models;

namespace GayDetectorBot.Data.Repos
{
    public class GayRepository
    {
        private readonly DataContext _context;

        public GayRepository(DataContext context)
        {
            _context = context;
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

                    var gId = (ulong)reader.GetDecimal(3);
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
    }
}