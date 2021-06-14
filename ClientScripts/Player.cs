using UnityEngine;
using System.Collections.Generic;

public class Player
{
    public GameObject GameObject { get; set; }
    public Vector3 PreviousDestinationPosition { get; set; }
    public Vector3 DestinationPosition { get; set; }

    public Player(GameObject gameObject)
    {
        GameObject = gameObject;
        PreviousDestinationPosition = GameObject.transform.position;
        DestinationPosition = GameObject.transform.position;
    }
}
