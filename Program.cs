using VoiceNoteTranscription.Clients;

namespace VoiceNoteTranscription;

public class Program
{
    private static Client _client;

    private static async Task Main()
    {
        _client = new DiscordClient(
            "/home/diamond/Documents/HPserver/linode-migration/Documents/VoiceNoteTranscription");
        await _client.Start();
    }
}