namespace GayDetectorBot.WebApi;

public class TelegramCommandException : Exception
{
    public TelegramCommandException(string message, Exception? innerException = null)
        : base(message, innerException)
    { }
}