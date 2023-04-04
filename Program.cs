using Discord;
using Discord.Net;
using Discord.WebSocket;
using System.Reflection;
using Discord.Commands;
using Discord.Utils;
using System.ComponentModel;
using System.Threading.Channels;
using INIParser;
using System.Xml.Serialization;
using System.Threading.Tasks.Dataflow;

namespace PostBot;

public  class Program
{
    private DiscordSocketClient _client;
    public ulong Guild = 1080603347427545199;
    public ulong TextChannel = 1080606943204356177;
    private string token = "MTA5MTM4NjM3MjczNDA3MDgyNA.GapIK9.Usa3nzYiN9CmL6uakzDO6C6gp0MTiuSY_RdEoE";

    public static Task Main(string[] args) => new Program().MainAsync();

    public async Task MainAsync()
    {
        var _config = new DiscordSocketConfig { MessageCacheSize = 100 };
        _client = new DiscordSocketClient(_config);
        _client.Log += Log;

        await StartBot(); 
    }

    public async Task StartBot()
    {
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        _client.Ready += () =>
        {
            Console.WriteLine("Ready!");
            OnReady();
            return Task.CompletedTask;
        };

        // Block this task until the program is closed
        await Task.Delay(-1);
    }

    public async Task OnReady()
    {
        // WE ARE READY TO DO STUFF NOW

        await BulkDelete();
        Console.WriteLine($"BulkDelete done");
        await PostFromXML();
        Console.WriteLine($"PostFromXML done");
    }

    public async Task PostFromXML()
    {
        XmlSerializer serializer = new XmlSerializer(typeof(List<MessagesFromXML>), new XmlRootAttribute("Messages"));
        List<MessagesFromXML> messages;

        using (FileStream stream = new FileStream("C:\\Users\\sabre\\source\\repos\\sabre230\\PostBot\\announcement.xml", FileMode.Open))
        {
            messages = (List<MessagesFromXML>)serializer.Deserialize(stream);
            Console.WriteLine($"Loaded {messages.Count} messages from file.");
        }

        Console.WriteLine($"messages: {messages}");

        foreach (var message in messages)
        {
            Console.WriteLine($"Message Text: {message.Text}");
            await SendText(message.Text);

            Console.WriteLine($"Message File: {message.File}");
            await SendText(message.File + " REAL IMAGES COMING SOON:tm:");
        }
    }

    public async Task BulkDelete()
    {
        var textChannel = await _client.GetChannelAsync(TextChannel) as ITextChannel;
        var messages = await textChannel.GetMessagesAsync(250).FlattenAsync();
        await textChannel.DeleteMessagesAsync(messages);
    }

    public async Task SendText(string message)
    {
        await _client.GetGuild(Guild).GetTextChannel(TextChannel).SendMessageAsync(message);
    }

    public async Task SendFile(string filePath)
    {
        await _client.GetGuild(Guild).GetTextChannel(TextChannel).SendFileAsync(filePath, null);
    }

    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
}

public class MessagesFromXML
{
    public string File { get; set; }
    public string Text { get; set; }
}