using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Discord;

public class LobbyController : DiscordObj
{
    public static LobbyController instance;
    public static Lobby CurrentLobby;
    public static bool Connected;
    public LobbyManager Manager { get; private set; }
    public override void JoinLobby(Lobby lobby)
    {
        CurrentLobby = lobby;
        Connected = true;
        Debug.LogWarning(lobby.Display());

        var num = Manager.MemberCount(lobby.Id);
        var secret = Manager.GetLobbyActivitySecret(lobby.Id);
        var ac = Discord.GetActivityManager();
        Activity activity = new Activity()
        {
            State = "Playing",
            Details = "Against someone",
            Secrets = new ActivitySecrets()
            {
                Join = secret
            },
            Party = new ActivityParty()
            {
                Id = lobby.Id.ToString(),
                Size = new PartySize()
                {
                    CurrentSize = num,
                    MaxSize = (int)lobby.Capacity
                }
            }
        };
        ac.UpdateActivity(activity, (r) =>
        {
            log($"Attempt activity set: {r}");
        });
    }

    public override void Ready(User user)
    {
        instance = this;
        Debug.LogWarning(user.Display(true));
        // Fetch lobbies.
        Manager = Discord.GetLobbyManager();

#if UNITY_EDITOR
        var cr = Manager.GetLobbyCreateTransaction();
        cr.SetCapacity(2);
        cr.SetType(LobbyType.Public);
        Manager.CreateLobby(cr, (Result res, ref Lobby lob) =>
        {
            if (res == Result.Ok)
            {
                JoinLobby(lob);
            }
            else
            {
                log($"Failed created: {res}");
            }
        });

//#else

        var q = Manager.GetSearchQuery();
        q.Distance(LobbySearchDistance.Local);
        q.Limit(5);
        q.Filter("slots", LobbySearchComparison.GreaterThan, LobbySearchCast.Number, "0"); // lobby with space left

        Manager.Search(q, (r) =>
        {
            if (r == Result.Ok)
            {
                var count = Manager.LobbyCount();
                log($"There are {count} lobbies online");
                for(int i = 0; i < count; i++)
                {
                    var lobby = Manager.GetLobbyId(count);
                    log($"At {i}: {lobby}");
                }
            } else {
                log($"Failed search: {r}");
            }
        });

#endif
    }

    void inLobby()
    {
        var n = Discord.GetNetworkManager();
        LobbyManager lob = Discord.GetLobbyManager();
        lob.OnMemberConnect += Lob_OnMemberConnect;
        lob.OnLobbyMessage += Lob_OnLobbyMessage;
        lob.OnNetworkMessage += Lob_OnNetworkMessage;
        lob.OnMemberDisconnect += Lob_OnMemberDisconnect;
        lob.OnMemberUpdate += Lob_OnMemberUpdate;
    }

    private void Lob_OnLobbyMessage(long lobbyId, long userId, byte[] data)
    {
        string text = System.Text.Encoding.UTF8.GetString(data);
        var user = GetUser(userId);
        log($"From {userId} ({user.Username}): {text}");
        foreach (DiscordObj o in DiscordController.objs)
        {
            o.OnMessage(user, text);
        }
    }

    private void Lob_OnMemberUpdate(long lobbyId, long userId)
    {
        log($"Update: {lobbyId}, user: {userId}");
    }

    private void Lob_OnMemberDisconnect(long lobbyId, long userId)
    {
        log($"Gone: {lobbyId}, old: {userId}");
    }

    private void Lob_OnMemberConnect(long lobbyId, long userId)
    {
        log($"Conn: {lobbyId}, new: {userId}");
    }

    private void Lob_OnNetworkMessage(long lobbyId, long userId, byte channelId, byte[] data)
    {
        string text = System.Text.Encoding.UTF8.GetString(data);
        log($"TEXT: {channelId} # {userId}: {text}");
    }

    public void TryJoinLobby(string secret)
    {
        if (Connected)
            return;
        Manager.ConnectLobbyWithActivitySecret(secret, (Result res, ref Lobby lob) =>
        {
            if(res == Result.Ok)
            {
                JoinLobby(lob);
            } else
            {
                log($"Failed to join {secret}, {res}");
            }
        });
    }
}
