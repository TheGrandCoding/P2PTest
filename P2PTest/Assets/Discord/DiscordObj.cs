using Discord;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DiscordObj : MonoBehaviour
{
    public Discord.Discord Discord { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        DiscordController.objs.Add(this);
    }

    public virtual void log(string message)
    {
        string path = this.GetType().Name;
        path = path.Replace("Controller", "");
        DiscordController.log($"{path}:// {message}");
    }

    public User GetUser(long id) { return DiscordController.GetUser(id); }

    /// <summary>
    /// Called when the user has connected to Discord.
    /// </summary>
    public abstract void Ready(User user);
    /// <summary>
    /// Called when the user joins a lobby
    /// </summary>
    public abstract void JoinLobby(Lobby lobby);
    /// <summary>
    /// Called when the user sends a message to the lobby
    /// </summary>
    public virtual void OnMessage(User from, string message) { }
}
