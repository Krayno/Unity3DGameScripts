using DarkRift;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveDataFromWorldServer : IDarkRiftSerializable
{
    public ulong SteamID  { get; set; }
    public byte CharacterSlot { get; set; }
    public long Appearance { get; set; }
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float PositionZ { get; set; }
    public float RotationY { get; set; }
    public short TotalLevel { get; set; }

    public SaveDataFromWorldServer(ulong steamID, byte characterSlot, long appearance, double positionX, double positionY, double positionZ, double rotationY, string name, int totalLevel)
    {
        SteamID = steamID;
        CharacterSlot = characterSlot;
        Appearance = appearance;
        PositionX = (float)positionX;
        PositionY = (float)positionY;
        PositionZ = (float)positionZ;
        RotationY = (float)rotationY;
        TotalLevel = (short)totalLevel;
    }

    public SaveDataFromWorldServer()
    {
    }

    public void Deserialize(DeserializeEvent e)
    {
        SteamID = e.Reader.ReadUInt64();
        CharacterSlot = e.Reader.ReadByte();
        Appearance = e.Reader.ReadInt64();
        PositionX = e.Reader.ReadSingle();
        PositionY = e.Reader.ReadSingle();
        PositionZ = e.Reader.ReadSingle();
        RotationY = e.Reader.ReadSingle();
        TotalLevel = e.Reader.ReadInt16();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(SteamID);
        e.Writer.Write(CharacterSlot);
        e.Writer.Write(Appearance);
        e.Writer.Write(PositionX);
        e.Writer.Write(PositionY);
        e.Writer.Write(PositionZ);
        e.Writer.Write(RotationY);
        e.Writer.Write(TotalLevel);
    }
}
