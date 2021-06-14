using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using System.IO;

public class PlayerPosition : IDarkRiftSerializable
{
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float PositionZ { get; set; }
    public float RotationY { get; set; }

    public PlayerPosition(float positionX, float positionY, float positionZ, float rotationY)
    {
        PositionX = positionX;
        PositionY = positionY;
        PositionZ = positionZ;
        RotationY = rotationY;
    }

    public PlayerPosition()
    {

    }

    public void Deserialize(DeserializeEvent e)
    {
        try
        {
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
        e.Writer.Write(PositionX);
        e.Writer.Write(PositionY);
        e.Writer.Write(PositionZ);
        e.Writer.Write(RotationY);
    }
}
