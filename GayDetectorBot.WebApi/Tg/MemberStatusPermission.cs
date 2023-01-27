namespace GayDetectorBot.WebApi.Tg;

[Flags]
public enum MemberStatusPermission
{
    None = 0,
    Creator = 1,
    Administrator = 2,
    Member = 4,
    Left = 8,
    Kicked = 16,
    Restricted = 32,
    All = Creator | Administrator | Member | Left | Kicked | Restricted
}