namespace GayDetectorBot.WebApi.Data.Repositories;

public interface IPointsRepository
{
    
}

public class PointsRepository : IPointsRepository
{
    private readonly GayDetectorBotContext _context;

    public PointsRepository(GayDetectorBotContext context)
    {
        _context = context;
    }
}