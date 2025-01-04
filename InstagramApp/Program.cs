// See https://aka.ms/new-console-template for more information

using VoiceNoteTranscription;

namespace Instagram;

public static class Program
{
    private static InstagramClient _client = new (
        System.Environment.GetEnvironmentVariable("INSTAGRAM_PATH")!, true);

    private static async Task Main()
    {
        await _client.Start();
    }
}