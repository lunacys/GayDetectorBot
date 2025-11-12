using GayDetectorBot.WebApi.Models.Tg;
using GayDetectorBot.WebApi.Models.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace GayDetectorBot.WebApi.Data;

public class GayDetectorBotContext : DbContext
{
    public DbSet<TgUser> TgUsers { get; private set; } = null!;
    public DbSet<User> Users { get; private set; } = null!;
    public DbSet<CustomCommand> CustomCommands { get; private set; } = null!;
    public DbSet<Gay> Gays { get; private set; } = null!;
    public DbSet<Participant> Participants { get; private set; } = null!;
    public DbSet<Chat> Chats { get; private set; } = null!;
    public DbSet<SchedulerContext> Schedules { get; private set; } = null!;
    public DbSet<SavedFile> SavedFiles { get; private set; } = null!;
    public DbSet<Points> Points { get; private set; } = null!;
    public DbSet<TgUserChatLink> TgUserChatLinks { get; private set; } = null!;

    public GayDetectorBotContext(DbContextOptions<GayDetectorBotContext> options)
        : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TgUser>().ToTable("TgUser");
        modelBuilder.Entity<User>().ToTable("User");
        modelBuilder.Entity<CustomCommand>().ToTable("CustomCommand");
        modelBuilder.Entity<Gay>().ToTable("Gay");
        modelBuilder.Entity<Participant>().ToTable("Participant");
        modelBuilder.Entity<Chat>().ToTable("Chat");
        modelBuilder.Entity<SchedulerContext>().ToTable("Schedules");
        modelBuilder.Entity<SavedFile>().ToTable("SavedFile");
        modelBuilder.Entity<Points>().ToTable("Points");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.ConfigureWarnings(builder =>
        {
            builder.Ignore(RelationalEventId.PendingModelChangesWarning);
        });
    }
}