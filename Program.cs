using Discord;
using Discord.Net;
using Discord.WebSocket;
using System.Reflection;
using Discord.Commands;
using Discord.Utils;
using System.ComponentModel;

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

        //foreach (var msg in GetMessages())
        //{

        //}
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