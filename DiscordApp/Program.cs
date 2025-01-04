// See https://aka.ms/new-console-template for more information

using VoiceNoteTranscription;

namespace Discord;

public static class Program
{
    private static DiscordClient _client = new (
        "storage/Discord", true);

    private static async Task Main()
    {
        await _client.Start();
    }
}