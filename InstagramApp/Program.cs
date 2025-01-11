// See https://aka.ms/new-console-template for more information

using VoiceNoteTranscription;

namespace Instagram;

public static class Program
{
    private static InstagramClient _client;
    private static string filepath = "storage/Instagram";
    private static bool logging;

    private static string username ="";
    private static string password;

    private static async Task Main(string[] args)
    {
        #if RELEASE
            Console.WriteLine("in release config");
        #elif DEBUG
            Console.WriteLine("in debug config");
        #else
            Console.WriteLine("in other config")
        #endif
        for (int i=0; i < args.Length; i++)
        {
            if (args[i] == "--filepath" && i+1 < args.Length)
            {
                filepath = args[i+1];
            }
            else if (args[i] == "--disable-logging")
            {
                logging = false;
            }

            if (args[i] == "--username" && i+1 < args.Length)
            {
                if (!args.Contains("--password"))
                {
                    Console.WriteLine("you need to also specify a password to log in");
                    return;
                }
                username = args[i+1];
            }

            if (args[i] == "--password" && i + 1 < args.Length)
            {
                if (!args.Contains("--username"))
                {
                    Console.WriteLine("you need to also specify a username to log in");
                    return;
                }
                password = args[i+1];
            }
        }
        _client = new InstagramClient(filepath, logging);
        if (username != "") _client.Login(username, password);
        await _client.Start();
    }
}