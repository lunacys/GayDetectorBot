using Telegram.Bot.Types;

namespace GayDetectorBot.WebApi.Tg;

public struct ChatCommand
{
    public string Prefix { get; set; }
    public string? Parameters { get; set; }

    public ChatCommand(string prefix, string? parameters)
    {
        Prefix = prefix;
        Parameters = parameters;
    }
}

public class CommandParser
{
    public ChatCommand? Parse(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return null;
        if (!text.StartsWith('!'))
            return null;

        var lower = text.ToLower().Trim();

        // Take the first word which will be the command, it must start with ! and end with a whitespace
        var whitespaceIndex = lower.IndexOfAny([' ', '\n']);
        var command = lower;
        if (whitespaceIndex >= 0)
            command = lower.Substring(0, whitespaceIndex);
        // Remove the ! symbol from the beginning
        command = command.Remove(0, 1);
        string? data;

        var i = text.IndexOfAny([' ', '\n']);
        if (i > 0)
            data = text.Substring(i + 1);
        else
            data = null;

        return new ChatCommand(command, data);
    }
}