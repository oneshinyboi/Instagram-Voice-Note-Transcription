using VoiceNoteTranscription.Clients;

namespace VoiceNoteTranscription;

public class InstagramClientApp
{
    private static InstagramClient _client;
    
    private static async Task Main()
    {
        _client = new InstagramClient(
            "/home/diamond/Projects/Instagram-Voice-Note-Transcription/audio/Instagram", true);
        await _client.Start();
    }
}