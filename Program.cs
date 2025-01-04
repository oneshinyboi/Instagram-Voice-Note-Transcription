using VoiceNoteTranscription.Clients;

namespace VoiceNoteTranscription;

public class Program
{
    private static Client _client;

    private static async Task Main()
    {
        _client = new InstagramClient(
            "/home/diamond/Projects/Instagram-Voice-Note-Transcription/audio/Instagram", true);
        await _client.Start();
    }
}