using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class Extensions
{
    public static string Display(this User user, bool includeId = false)
    {
        return $"{(includeId ? $"{user.Id}- " : "")}{user.Username}#{user.Discriminator}";
    }

    public static string Display(this Lobby lobby)
    {
        return $"{lobby.Id}:{lobby.Secret}{(lobby.Locked ? " L" : "")} {lobby.Secret} {lobby.Type}";
    }
}
