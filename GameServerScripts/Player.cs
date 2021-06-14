using DarkRift.Server;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public IClient Client { get; set; }
    public ulong SteamID { get; set; }
    public byte CharacterSlot { get; set; }
    public Vector3 Position { get; set; }
    public float RotationY { get; set; }
    public long Appearance { get; set; }
    public Vector3 LastGroundedPosition { get; set; }
    public float FallingUpdates { get; set; }
    public float RisingUpdates { get; set; }
    public Dictionary<IClient, PlayerCurrent> NearbyPlayers { get; set; }
    public string Name { get; set; }
    public short TotalLevel { get; set; }

    public Player(IClient client, ulong steamID, byte characterSlot, Vector3 position, float rotationY, long appearance, string name, short totalLevel)
    {
        Client = client;
        SteamID = steamID;
        CharacterSlot = characterSlot;
        Position = position;
        RotationY = rotationY;
        Appearance = appearance;
        LastGroundedPosition = position;
        NearbyPlayers = new Dictionary<IClient, PlayerCurrent>();
        Name = name;
        TotalLevel = totalLevel;
    }

    public Player(IClient client, ulong steamID, byte characterSlot)
    {
        Client = client;
        SteamID = steamID;
        CharacterSlot = characterSlot;
    }
}
