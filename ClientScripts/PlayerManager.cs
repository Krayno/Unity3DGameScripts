using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public Dictionary<ushort, Player> Players;

    void Awake()
    {
        Players = new Dictionary<ushort, Player>();
    }

    private void OnDisable()
    {
        //Disconnected, so reset everything.
        Players.Clear();
    }
}
