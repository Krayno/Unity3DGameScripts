using DarkRift;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlayerEnteredRenderDistance : IDarkRiftSerializable
{
    public ushort ClientID { get; set; }
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float PositionZ { get; set; }
    public float RotationY { get; set; }
    public long Appearance { get; set; }
    public string Name { get; set; }

    public PlayerEnteredRenderDistance(ushort clientID, float positionX, float positionY, float positionZ, float rotationY, long appearance, string name)
    {
        ClientID = clientID;
        PositionX = positionX;
        PositionY = positionY;
        PositionZ = positionZ;
        RotationY = rotationY;
        Appearance = appearance;
        Name = name;
    }

    public PlayerEnteredRenderDistance()
    {

    }

    public void Deserialize(DeserializeEvent e)
    {
        try
        {
            ClientID = e.Reader.ReadUInt16();
            PositionX = e.Reader.ReadSingle();
            PositionY = e.Reader.ReadSingle();
            PositionZ = e.Reader.ReadSingle();
            RotationY = e.Reader.ReadSingle();
            Appearance = e.Reader.ReadInt64();
            Name = e.Reader.ReadString();
        }
        catch (EndOfStreamException)
        {
            throw;
        }
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(ClientID);
        e.Writer.Write(PositionX);
        e.Writer.Write(PositionY);
        e.Writer.Write(PositionZ);
        e.Writer.Write(RotationY);
        e.Writer.Write(Appearance);
        e.Writer.Write(Name);
    }
}
