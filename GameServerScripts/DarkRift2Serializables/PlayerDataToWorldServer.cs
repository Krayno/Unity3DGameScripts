using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;


public class PlayerDataToWorldServer : IDarkRiftSerializable
{
    public ulong SteamID { get; set; }
    public long Appearance { get; set; }
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float PositionZ { get; set; }
    public float RotationY { get; set; }
    public string Name { get; set; }
    public short TotalLevel { get; set; }

    public PlayerDataToWorldServer(ulong steamID, long appearance, double positionX, double positionY, double positionZ, double rotationY, string name, int totalLevel)
    {
        SteamID = steamID;
        Appearance = appearance;
        PositionX = (float)positionX;
        PositionY = (float)positionY;
        PositionZ = (float)positionZ;
        RotationY = (float)rotationY;
        Name = name;
        TotalLevel = (short)totalLevel;
    }

    public PlayerDataToWorldServer()
    {

    }

    public void Deserialize(DeserializeEvent e)
    {
        SteamID = e.Reader.ReadUInt64();
        Appearance = e.Reader.ReadInt64();
        PositionX = e.Reader.ReadSingle();
        PositionY = e.Reader.ReadSingle();
        PositionZ = e.Reader.ReadSingle();
        RotationY = e.Reader.ReadSingle();
        Name = e.Reader.ReadString();
        TotalLevel = e.Reader.ReadInt16();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(SteamID);
        e.Writer.Write(Appearance);
        e.Writer.Write(PositionX);
        e.Writer.Write(PositionY);
        e.Writer.Write(PositionZ);
        e.Writer.Write(RotationY);
        e.Writer.Write(Name);
        e.Writer.Write(TotalLevel);
    }
}
