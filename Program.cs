using Discord;
using Discord.Net;
using Discord.WebSocket;
using System.Reflection;
using Discord.Commands;
using Discord.Utils;

using System.Xml.Linq;


namespace PostBot;

public  class Program
{
    private DiscordSocketClient _client;

    public ulong Guild;
    public ulong TextChannel;
    private string token;

    public static Task Main(string[] args) => new Program().MainAsync();

    public async Task MainAsync()

    {
        // Get our settings before doing anything else!
        await GetConfigFromXML();

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
        // Uncomment to keep the task open after finishing
        await Task.Delay(-1);
    }

    public async Task OnReady()
    {
        // WE ARE READY TO DO STUFF NOW
        Console.WriteLine("Checking for config.xml and announcement.xml...");
        await CheckForFiles();

        Console.WriteLine("Starting bulk delete...");
        await BulkDelete(250);
        Console.WriteLine("Bulk delete finished!");
        Console.WriteLine("Starting PostFromXML...");
        await PostFromXML();
        Console.WriteLine("PostFromXML done! You should be safe to close the window now.");
    }

    public async Task CheckForFiles()
    {
        bool restartRequired = false;

        if (!File.Exists(@"config.xml"))
        {
            Console.WriteLine("No config.xml found! Creating a new one based on the template.");
            File.Copy(@"config-template.xml", @"config.xml");

            restartRequired = true;
        }

        if (!File.Exists(@"announcement.xml"))
        {
            Console.WriteLine("No announcement.xml found! Creating a new one based on the template.");
            File.Copy(@"announcement-template.xml", @"announcement.xml");

            restartRequired = true;
        }

        if (restartRequired)
        {
            Console.WriteLine("Further configuration is required.");
            Console.WriteLine("Please update config.xml and announcment.xml with your chosen information.");
        }
    }

    public async Task GetConfigFromXML()
    {
        if (!File.Exists(Path.Combine("config.xml")))
        {
            Console.WriteLine($"File not found at: {Path.Combine("config.xml")}");
            return;
        }

        XDocument xmlConfigFile = XDocument.Load("config.xml");

        try
        {
            // Get the values of BotToken, GuildID, and TextChannelID
            token = xmlConfigFile.Element("Settings").Element("BotToken").Value;
            Guild = ulong.Parse(xmlConfigFile.Element("Settings").Element("GuildID").Value);
            TextChannel = ulong.Parse(xmlConfigFile.Element("Settings").Element("TextChannelID").Value);

            // Use the values as needed
            if (token!= null || token != "")
            {
                Console.WriteLine($"Bot Token: -->{token}<--");
            }
            Console.WriteLine($"Guild ID: {Guild}");
            Console.WriteLine($"Text Channel ID: {TextChannel}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public async Task PostFromXML()
    {
        if (!File.Exists(Path.Combine("announcement.xml")))
        {
            Console.WriteLine($"announcement.xml not found.");
            return;
        }

        XDocument xmlAnnouncementFile = XDocument.Load("announcement.xml");
        List<MessagesFromXML> messages = xmlAnnouncementFile.Root.Elements("Message").Select(x => new MessagesFromXML { Text = x.Element("Text").Value, File = x.Element("File").Value }).ToList();

        try
        {
            Console.WriteLine($"Loaded {messages.Count} messages from file.");

            foreach (var message in messages)
            {
                if (message.Text != "")
                {
                    Console.WriteLine($"Message Text: {message.Text}");
                    await SendText(message.Text);
                }
                
                if (message.File != "" && File.Exists(message.File))
                {
                    Console.WriteLine($"Message File: {message.File}");
                    await SendFile(message.File);
                }
            }
        }
        catch(Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public async Task BulkDelete(int amount)
    {
        var textChannel = await _client.GetChannelAsync(TextChannel) as ITextChannel;
        var messages = await textChannel.GetMessagesAsync(amount).FlattenAsync();
        await textChannel.DeleteMessagesAsync(messages);
    }

    public async Task SendText(string message)
    {
        if (message != "")
        {
            await _client.GetGuild(Guild).GetTextChannel(TextChannel).SendMessageAsync(message);
        }
    }

    public async Task SendFile(string filePath)
    {
        if (filePath != "")
        {
            await _client.GetGuild(Guild).GetTextChannel(TextChannel).SendFileAsync(filePath, null);
        }        
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

public class ConfigFromXML
{ 
    public string BotToken { get; set; }
    public string GuildID { get; set; }
    public string TextChannelID { get; set; }
}
