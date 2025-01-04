using Newtonsoft.Json;

namespace VoiceNoteTranscription.Clients;

public abstract class Client
{
    protected bool DoNextVoiceMessage = true;
    protected string FilePath;
    protected bool Logging;

    protected Client(string filepath, bool logging)
    {
        FilePath = filepath;
        Logging = logging;
    }

    protected void Log(MessageInfo info, string filename)
    {
        if (!Logging) return;
        try
        {
            string fullPath = Path.Combine(FilePath, filename);
            string json = JsonConvert.SerializeObject(info);

            using (StreamWriter writer = new StreamWriter(fullPath, append: true))
            {
                writer.WriteLine(json);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to log message: {ex.Message}");
        }
    }
    public abstract Task Start();
}