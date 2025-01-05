// See https://aka.ms/new-console-template for more information

using VoiceNoteTranscription;

namespace Instagram;

public static class Program
{
    private static InstagramClient _client = new (
        "storage/Instagram", true);

    private static async Task Main()
    {
        #if RELEASE
            Console.WriteLine("in release config");
        #elif DEBUG
            Console.WriteLine("in debug config");
        #else
            Console.WriteLine("in other config")
        #endif
        await _client.Start();
    }
}