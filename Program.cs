using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using System.Reflection;
using Discord.Commands;
using Discord.Utils;
using System.Xml.Linq;
using System;
using Newtonsoft.Json;
using System.Windows.Input;

//namespace PostBot;

public  class Program
{
    private DiscordSocketClient _client;
    private CommandService _commandService;
    private CommandHandler _commandHandler;

    public ulong Guild;
    public ulong TextChannel;
    private string token;

    public static Task Main(string[] args) => new Program().MainAsync();

    public async Task MainAsync()

    {
        // Ensure our config files are in place
        Console.WriteLine("Checking for config.xml and announcement.xml...");
        CheckForFiles();
        Console.WriteLine("Done!");

        // Get our settings before doing anything else!
        await GetConfigFromXML();

        // Create a message cache for reasons
        var _config = new DiscordSocketConfig { MessageCacheSize = 100 };

        // Create a new client instance (just one)
        _client = new DiscordSocketClient(_config);

        // Create a new command service and command handler
        _commandService = new(); // apparently new() is the new hotness? 
        _commandHandler = new(_client, _commandService);

        _client.Log += Log;

        await StartBot(); 
    }

    public async Task StartBot()
    {
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        // Load our modules/commands
        await _commandHandler.InstallCommandsAsync();
        _client.SlashCommandExecuted += SlashCommandHandler;
        //_client.InteractionCreated += HandleInteractionAsync;


        // Listen for when the client is ready
        _client.Ready += () =>
        {
            Console.WriteLine("Ready!");
            OnReady();
            return Task.CompletedTask;
        };

        //_client.MessageReceived += _commandHandler.HandleCommandAsync;

        // Block this task until the program is closed
        // Uncomment to keep the task open after finishing
        await Task.Delay(-1);
    }

    public async Task OnReady()
    {
        // WE ARE READY TO DO STUFF NOW

        // Once the bot is beefier, we will delegate these to mod commands
        //Console.WriteLine("Starting bulk delete...");
        //await BulkDelete(250);
        //Console.WriteLine("Bulk delete finished!");

        Console.WriteLine("Starting PostFromXML...");
        await PostFromXML();
        Console.WriteLine("PostFromXML done! You should be safe to close the window now.");

        await SlashCommands();
    }

    private async Task SlashCommands()
    {
        // Set up slash commands
        var serverCommand = new SlashCommandBuilder()
        .WithName("echo-server")
        .WithDescription("echoes a message");

        // Echo command (global)
        var echoCommand = new SlashCommandBuilder()
        .WithName("echo")
        .WithDescription("Echoes a message")
        .AddOption("string", ApplicationCommandOptionType.String, "The string you want to echo.", isRequired: true);


        // Rules command (global)
        var rulesCommand = new SlashCommandBuilder();
        rulesCommand.WithName("rules");
        rulesCommand.WithDescription("Quotes an abridges version of th rules");

        try
        {
            // With global commands we don't need the guild.
            await _client.CreateGlobalApplicationCommandAsync(echoCommand.Build());
            await _client.CreateGlobalApplicationCommandAsync(rulesCommand.Build());
            // Using the ready event is a simple implementation for the sake of the example. Suitable for testing and development.
            // For a production bot, it is recommended to only run the CreateGlobalApplicationCommandAsync() once for each command.
        }
        catch (ApplicationCommandException exception)
        {
            // If our command was invalid, we should catch an ApplicationCommandException. This exception contains the path of the error as well as the error message. You can serialize the Error field in the exception to get a visual of where your error is.
            var json = JsonConvert.SerializeObject(exception.Reason, Formatting.Indented);

            // You can send this error somewhere or just print it to the console, for this example we're just going to print it.
            Console.WriteLine(json);
        }
    }

    private async Task SlashCommandHandler(SocketSlashCommand command)
    {
        // Checking against the given command
        switch (command.ToString())
        {
            case "echo":
                // Execute somecommand
                break;

            case "rules":
                // Execute someothercommand
                break;
            
            default:
                Console.WriteLine($"{command} is an invalid command");
                break;
        }

        await command.RespondAsync($"So this happened: {command.Data.Name}");
    }

    public void CheckForFiles()
    {
        bool restartRequired = false;

        if (!File.Exists("config.xml"))
        {
            Console.WriteLine("No config.xml found! Creating a new one based on the template.");
            File.Copy("config-template.xml", "config.xml");

            restartRequired = true;
        }

        if (!File.Exists("announcement.xml"))
        {
            Console.WriteLine("No announcement.xml found! Creating a new one based on the template.");
            File.Copy("announcement-template.xml", "announcement.xml");

            restartRequired = true;
        }

        if (restartRequired)
        {
            Console.WriteLine("Further configuration is required.");
            Console.WriteLine("Please update config.xml and announcment.xml with your chosen information.");

            Console.WriteLine("Press any key to close this window...");
            Console.ReadKey();
            Environment.Exit(0);
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

public class CommandHandler
{
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;

    // Retrieve client and CommandService instance via ctor
    public CommandHandler(DiscordSocketClient client, CommandService commands)
    {
        _commands = commands;
        _client = client;
    }

    public async Task InstallCommandsAsync()
    {
        // Hook the MessageReceived event into our command handler
        _client.MessageReceived += HandleCommandAsync;

        // Here we discover all of the command modules in the entry 
        // assembly and load them. Starting from Discord.NET 2.0, a
        // service provider is required to be passed into the
        // module registration method to inject the 
        // required dependencies.
        //
        // If you do not use Dependency Injection, pass null.
        // See Dependency Injection guide for more information.
        await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(),
                                        services: null);
    }

    private async Task HandleCommandAsync(SocketMessage messageParam)
    {
        // Don't process the command if it was a system message
        var message = messageParam as SocketUserMessage;
        if (message == null) return;

        // Create a number to track where the prefix ends and the command begins
        int argPos = 0;

        // Determine if the message is a command based on the prefix and make sure no bots trigger commands
        if (!(message.HasCharPrefix('!', ref argPos) ||
            message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
            message.Author.IsBot)
            return;

        // Create a WebSocket-based command context based on the message
        var context = new SocketCommandContext(_client, message);

        // Execute the command with the command context we just
        // created, along with the service provider for precondition checks.
        await _commands.ExecuteAsync(
            context: context,
            argPos: argPos,
            services: null);
    }
}

// Create a module with no prefix
public class MyModule : ModuleBase<SocketCommandContext>
{
    [SlashCommand("hello", "Says hello to the user.")]
    public async Task HelloAsync()
    {
        await ReplyAsync("Hello, world!");
    }
}