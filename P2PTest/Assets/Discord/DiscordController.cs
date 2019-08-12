using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Discord;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Threading;

public class DiscordController : MonoBehaviour
{
    public static Discord.Discord discord;
    public static Lobby lobby;

    public static List<DiscordObj> objs = new List<DiscordObj>();

    private static LobbyManager lobbyManager;
    private static Discord.User user;

    // Start is called before the first frame update
    void Start()
    {
#if UNITY_EDITOR
        System.Environment.SetEnvironmentVariable("DISCORD_INSTANCE_ID", "0");
#else
        System.Environment.SetEnvironmentVariable("DISCORD_INSTANCE_ID", "1");
#endif
        discord = new Discord.Discord(553650964276576318, (ulong)CreateFlags.Default);
        //d.RegisterCommand("chess") // TODO: installation.
        discord.SetLogHook(LogLevel.Debug, (level, message) =>
        {
            message = $"DISCORD: {message}";
            switch (level)
            {
                case LogLevel.Debug:
                case LogLevel.Info:
                    Debug.Log(message);
                    break;
                case LogLevel.Warn:
                    log(message);
                    break;
                default:
                    Debug.LogError(message);
                    break;
            }
        });
        var u = discord.GetUserManager();
        u.OnCurrentUserUpdate += () =>
        {
            var usr = u.GetCurrentUser();
            var prem = u.GetCurrentUserPremiumType();
            user = usr;
            log($"User: {usr.Username}, {usr.Id} -- {prem}");
            foreach(DiscordObj o in objs)
            {
                o.Discord = discord;
                o.Ready(user);
            }
        };

        var activityManager = discord.GetActivityManager();

        activityManager.UpdateActivity(new Activity()
        {
            State = "Hub",
            Details = "Searching for lobbies"
        }, x =>
        {
            Debug.Log(x);
        });

        activityManager.OnActivityJoin += ActivityManager_OnActivityJoin;
        activityManager.OnActivityInvite += ActivityManager_OnActivityInvite;
        activityManager.OnActivityJoinRequest += ActivityManager_OnActivityJoinRequest;
        /*string file = "lobby.txt";
#if !UNITY_EDITOR
        var txn = lobbyManager.GetLobbyCreateTransaction();
        txn.SetCapacity(2);
        txn.SetType(LobbyType.Public);

        lobbyManager.CreateLobby(txn, (Result result, ref Lobby _lobby) =>
        {
            lobby = _lobby;
            System.IO.File.WriteAllText(file, $"{lobby.Id}:{lobby.Secret}");
            log($"Created {lobby.Id}, secret {lobby.Secret} owned by {lobby.OwnerId}");
            inLobby();
        });

#else

        string text = System.IO.File.ReadAllText(file);
        lobbyManager.ConnectLobbyWithActivitySecret(text, (Result res, ref Lobby _lobby) =>
        {
            lobby = _lobby;
            log($"Joined {lobby.Id} owned by {lobby.OwnerId}");
            inLobby();
            foreach (DiscordObj o in objs)
                o.JoinLobby(lobby);
        });
#endif*/
    }

    public static void log(string message)
    {
        Debug.LogWarning(message);
        string fileName = "log_";
        try
        {
            fileName += user.Id.ToString();
        }
        catch { fileName = "log"; }
        System.IO.File.AppendAllText(fileName + ".txt", message + "\r\n");
    }

    public void LobbySend(string message)
    {
        log("Sending " + message);
        lobbyManager.SendLobbyMessage(lobby.Id, message, x =>
        {
            Debug.Log($"{x} on {message}");
        });
    }

    class getUserValue
    {
        public AutoResetEvent Event;
        public Result Result = Result.NotFetched;
        public User User;
        public getUserValue()
        {
            Event = new AutoResetEvent(false);
        }
    }

    static Dictionary<long, getUserValue> getUserEvents = new Dictionary<long, getUserValue>();
    public static User GetUser(long id)
    {
        getUserValue val;
        if (getUserEvents.TryGetValue(id, out val))
        {
        } else
        {
            val = new getUserValue();
            getUserEvents[id] = val;
            var l = discord.GetUserManager();
            l.GetUser(id, (Result res, ref User usr) =>
            {
                log($"Fetched {id}, result: {res}, name: {usr.Username ?? "<n/a"}");
                if(getUserEvents.TryGetValue(usr.Id, out var thing))
                {
                    thing.Result = res;
                    thing.User = usr;
                    thing.Event.Set();
                }
            });
        }
        val.Event.WaitOne(5000);
        return val.User;
    }

    /// <summary>
    /// Fires when a user asks to join the current user's game.
    /// </summary>
    /// <param name="user">	the user asking to join</param>
    private void ActivityManager_OnActivityJoinRequest(ref User user)
    {
        var d = discord.GetActivityManager();
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Fires when the user receives a join or spectate invite.
    /// </summary>
    /// <param name="type">whether this invite is to join or spectate</param>
    /// <param name="user">the user sending the invite</param>
    /// <param name="activity">	the inviting user's current activity</param>
    private void ActivityManager_OnActivityInvite(ActivityActionType type, ref User user, ref Activity activity)
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Fires when a user accepts a game chat invite or receives confirmation from Asking to Join.
    /// </summary>
    /// <param name="secret">the secret to join the user's game</param>
    private void ActivityManager_OnActivityJoin(string secret)
    {
        
        throw new System.NotImplementedException();
    }

    // Update is called once per frame
    void Update()
    {
        try
        {
            discord.RunCallbacks();
        } catch (System.NullReferenceException)
        {
            Debug.LogError("Discord has closed! we must too.");
            Application.Quit(1);
        }
    }

    private void OnApplicationQuit()
    {
        try
        {
            var a = discord.GetActivityManager();
            a.ClearActivity(null);
        } catch { }
    }
}
