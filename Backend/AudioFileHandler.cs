using CliWrap;
using CliWrap.Buffered;
using Google.Cloud.Speech.V1;
using NAudio.Wave;

namespace VoiceNoteTranscription;

public class AudioFileHandler
{
    private const int GoogleMaxFilesize = 10485760;
    private const int MyMaxFilesizeBeforeHeadder = 10484680;

    private readonly SpeechClient _client = SpeechClient.Create();

    private readonly RecognitionConfig _config;

    private readonly RecognitionConfig _defaultConfig = new()
    {
        Encoding = RecognitionConfig.Types.AudioEncoding.EncodingUnspecified,
        LanguageCode = LanguageCodes.English.UnitedStates,
        AudioChannelCount = 2,
        EnableSeparateRecognitionPerChannel = false,
        EnableAutomaticPunctuation = true,
        AlternativeLanguageCodes = { LanguageCodes.Spanish.Spain, LanguageCodes.French.France }
    };

    private readonly string _filePath;

    public AudioFileHandler(string absPath, RecognitionConfig? conf = null)
    {
        if (conf != null)
            _config = conf;
        else
            _config = _defaultConfig;
        _filePath = absPath;
    }

    private static byte[] GetWavHeadder(string filename)
    {
        using var f = File.OpenRead(filename);
        var val = new byte[77];
        f.Read(val, 0, 77);
        return val;
    }
    static double GetWavFileLengthInSeconds(string filePath)
    {
        using (var reader = new AudioFileReader(filePath))
        {
            return reader.TotalTime.TotalSeconds;
        }
    }
    private static byte[] GetAllowableLength(string filename, int offset, int length)
    {
        using var f = File.OpenRead(filename);
        f.Seek(offset, SeekOrigin.Begin);
        var val = new byte[length];
        f.Read(val, 0, length);
        return val;
    }

    private static byte[] Combine(byte[] first, byte[] second)
    {
        var bytes = new byte[first.Length + second.Length];
        Buffer.BlockCopy(first, 0, bytes, 0, first.Length);
        Buffer.BlockCopy(second, 0, bytes, first.Length, second.Length);
        return bytes;
    }

    private static void SaveBytesToFile(byte[] data, string filePath)
    {
        using var writer = new BinaryWriter(File.OpenWrite(filePath));
        writer.Write(data);
    }

    public async Task<MessageInfo> ProcessDownloadUrl(string url, string fileFormat)
    {
        Console.WriteLine($"attempting to download: {url}");
        var filename = DateTime.Now.ToString("HH:mm:ss:FFFFFF");
        string oldFilePath;
        string newFilePath;
        oldFilePath = $"{_filePath}/{filename}.{fileFormat}";
        newFilePath = $"{_filePath}/{filename}.wav";
        using (var client = new HttpClient())
        using (var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
        {
            response.EnsureSuccessStatusCode();

            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var fileStream = new FileStream(oldFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await stream.CopyToAsync(fileStream);
            }
        }
        await Cli
            .Wrap("ffmpeg")
            .WithArguments($"-i {oldFilePath} {newFilePath} -vn")
            .ExecuteBufferedAsync();

        Console.WriteLine("file saved");

        Console.WriteLine("starting audio stuff");
        var combinedMessages = "";
        var fileLengthBytes = new FileInfo(newFilePath).Length;
        var x = (int)Math.Ceiling((double)fileLengthBytes / MyMaxFilesizeBeforeHeadder);
        var byteHeadder = GetWavHeadder(newFilePath);
        byte[] currFile;
        var recognizeTasks = new Task<RecognizeResponse>[x];
        for (var i = 0; i < x; i++)
        {
            if (i != x - 1)
                currFile = GetAllowableLength(newFilePath, 77 + i * MyMaxFilesizeBeforeHeadder,
                    MyMaxFilesizeBeforeHeadder);
            else
                currFile = GetAllowableLength(newFilePath, 77 + i * MyMaxFilesizeBeforeHeadder,
                    (int)(fileLengthBytes - 77 - i * MyMaxFilesizeBeforeHeadder));

            SaveBytesToFile(Combine(byteHeadder, currFile), newFilePath.Replace(".wav", "") + $":{i}.wav");
            var audio = RecognitionAudio.FromFile(newFilePath.Replace(".wav", "") + $":{i}.wav");

            recognizeTasks[i] = _client.RecognizeAsync(_config, audio);
        }

        for (var i = 0; i < x; i++)
        {
            var response = await recognizeTasks[i];

            Console.WriteLine("deleting files");
            File.Delete(newFilePath.Replace(".wav", "") + $":{i}.wav");
            Console.WriteLine("deleted");
            var message = "";
            for (var y = 0; y < response.Results.Count; y++) message += response.Results[y].Alternatives[0].Transcript;
            combinedMessages += message + " ";
        }
        MessageInfo toReturn;

        if (combinedMessages != "")
        {
            toReturn = new MessageInfo()
            {
                Message = combinedMessages,
                Timestamp = DateTime.Now,
                Duration = (int)GetWavFileLengthInSeconds(newFilePath)
            };
        }
        else
        {
            toReturn = new MessageInfo()
            {
                Message = "I made a fucky wucky UwU. I don't understand what you said 🥺"
            }; 
        }
        
        File.Delete(oldFilePath);
        File.Delete(newFilePath);
        return toReturn;
    }
}