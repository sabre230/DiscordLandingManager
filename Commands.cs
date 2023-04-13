﻿using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PostBot;

public class Commands
{
    public async Task Echo(DiscordSocketClient client, ulong guild, ulong textChannel, string message)
    {
        if (message != "")
        {
            await client.GetGuild(guild).GetTextChannel(textChannel).SendMessageAsync(message);
        }
        return;
    }

    public async Task SendMessage(DiscordSocketClient client, ulong guild, ulong textChannel, string message)
    {
        if (message != "")
        {
            await client.GetGuild(guild).GetTextChannel(textChannel).SendMessageAsync(message);
        }
        return;
    }

    public async Task BulkDelete(DiscordSocketClient client, ulong guild, ulong textChannel, int amount)
    {
        var channel = await client.GetChannelAsync(textChannel) as ITextChannel;
        var messages = await channel.GetMessagesAsync(amount).FlattenAsync();
        await channel.DeleteMessagesAsync(messages);
    }
}
