using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using System.IO;

public class PlayerWithinRenderDistance : IDarkRiftSerializable
{
    public ushort ClientID { get; set; }
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float PositionZ { get; set; }
    public float RotationY { get; set; }

    public PlayerWithinRenderDistance(ushort clientID, float positionX, float positionY, float positionZ, float rotationY)
    {
        ClientID = clientID;
        PositionX = positionX;
        PositionY = positionY;
        PositionZ = positionZ;
        RotationY = rotationY;
    }

    public PlayerWithinRenderDistance()
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
    }
}
