using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCurrent
{
    public Vector3 Position { get; set; }
    public float RotationY { get; set; }

    public PlayerCurrent(Vector3 position, float rotationY)
    {
        Position = position;
        RotationY = rotationY;
    }
}
