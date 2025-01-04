using Discord;
using Discord.WebSocket;

namespace VoiceNoteTranscription;

public class DiscordClient : Client
{
    private static DiscordSocketClient _client;	
    public DiscordClient(string filepath, bool logging) : base(filepath, logging)
    {
    }

    public override async Task Start()
    {
        var config = new DiscordSocketConfig {GatewayIntents = GatewayIntents.MessageContent};
        _client = new DiscordSocketClient();
        await _client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DiscordToken"));
        await _client.StartAsync();

        _client.MessageReceived += ProcessVoiceNote;
        
        _client.Ready += () => 
        {
            Console.WriteLine("Bot is connected!");
            return Task.CompletedTask;
        };
        
        await Task.Delay(-1);
    }

    private static async Task ProcessVoiceNote(SocketMessage socketMessage)
    {
        if (socketMessage.Attachments.Count >= 0)
        {
            foreach (var attachment in socketMessage.Attachments)
            {
                Console.WriteLine(attachment.ContentType);
                if (attachment.ContentType == "audio")
                {
                    Console.WriteLine(attachment.Url);
                }
            }
        }
    }
}