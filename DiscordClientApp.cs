using VoiceNoteTranscription.Clients;

namespace VoiceNoteTranscription;

public class DiscordClientApp
{
    private static DiscordClient _client;
    private static async Task Main()
    {
        _client = new DiscordClient(
            "/home/diamond/Projects/Instagram-Voice-Note-Transcription/audio/Instagram", true);
        await _client.Start();
    }
}