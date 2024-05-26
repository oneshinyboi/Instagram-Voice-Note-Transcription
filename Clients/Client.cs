namespace VoiceNoteTranscription.Clients;

public abstract class Client
{
    protected bool DoNextVoiceMessage;
    protected string FilePath;

    protected Client(string filepath)
    {
        FilePath = filepath;
    }

    public abstract Task Start();
}