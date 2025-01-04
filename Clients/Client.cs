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

    protected void Log(MessageInfo info)
    {
        if (!Logging) return;
        try
        {
            string timestamp = DateTime.Now.ToString("HH-mm-ss-ffffff");
            string sanitizedSender = info.Sender.Replace(":", "_").Replace("/", "_");
            string fileName = $"{timestamp}_{sanitizedSender}.json";
            string fullPath = Path.Combine(FilePath, fileName);

            string json = JsonConvert.SerializeObject(info);
            Console.WriteLine(json);
            File.WriteAllText(fullPath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to log message: {ex.Message}");
        }
    }
    public abstract Task Start();
}