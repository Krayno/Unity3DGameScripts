using DarkRift.Server;

public class Player
{
    public IClient Client { get; set; }
    public ulong SteamID { get; set; }
    public IClient ConnectedToWorld { get; set; }
    public string CurrentCharacterName { get; set; }
    public string CurrentCharacterGuild { get; set; }
    public short CurrentCharacterTotalLevel { get; set; }

    public Player(IClient client, ulong steamID, IClient connectedToWorld)
    {
        Client = client;
        SteamID = steamID;
        ConnectedToWorld = connectedToWorld;
    }
}