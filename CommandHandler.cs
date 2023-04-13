using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PostBot;

//public class CommandHandler
//{
//    private readonly DiscordSocketClient _client;
//    private readonly CommandService _commands;

//    public CommandHandler(DiscordSocketClient client, CommandService commands)
//    {
//        _client = client;
//        _commands = commands;
//    }

//    public async Task InstallCommandsAsync()
//    {
//        _client.MessageReceived += HandleCommandAsync;

//        await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: null);
//    }

//    private async Task HandleCommandAsync(SocketMessage messageParam)
//    {
//        var message = messageParam as SocketUserMessage;

//        if (message == null) return;

//        int argPos = 0;

//        if (!(message.HasCharPrefix('!', ref argPos) ||
//                    message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
//                    message.Author.IsBot)
//            return;

//        var context = new SocketCommandContext(_client, message);

//        await _commands.ExecuteAsync(
//            context: context,
//            argPos: argPos,
//            services: null);
//    }
//}

//public class RulesCommand : ModuleBase<SocketCommandContext>
//{
//    [Command("echo")]
//    [Summary("Echoes a message")]
//    public Task async SayAsync(
//        [Remainder] [Summary("The text to echo")]
//        string echo)
//        => ReplyAsync(echo);
//}