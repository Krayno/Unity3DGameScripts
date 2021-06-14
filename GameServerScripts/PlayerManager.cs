using System.Collections.Generic;
using DarkRift.Server;

public static class PlayerManager
{
    public static Dictionary<IClient, Player> IClientPlayers = new Dictionary<IClient, Player>();
}
