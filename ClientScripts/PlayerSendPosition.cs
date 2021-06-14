using UnityEngine;
using DarkRift;
using DarkRift.Client.Unity;
using System.Collections.Generic;
using System;
using System.Collections;

public class PlayerSendPosition : MonoBehaviour
{
    public PlayerManager PlayerManager;
    public AccurateClock AccurateClock; //Timer on a seperate thread.

    private UnityClient Client; //The client that connects to the server.
    private Transform Player;

    private PlayerPosition PlayerData;

    private void Awake()
    {
        PlayerData = null;

        Client = transform.parent.GetComponent<UnityClient>();
    }

    private void Start()
    {
        AccurateClock.Tick.Ticked += EveryTick;
    }

    private void OnEnable()
    {
        StartCoroutine("WaitForPlayerSpawned");
    }

    IEnumerator WaitForPlayerSpawned()
    {
        while (!PlayerManager.Players.ContainsKey(Client.ID))
        {
            yield return null;
        }

        Player = PlayerManager.Players[Client.ID].GameObject.transform;
        PlayerData = new PlayerPosition(Player.position.x, Player.position.y, Player.position.z, Player.rotation.eulerAngles.y);
    }

    void EveryTick(object sender, EventTimerTickedArgs e)
    {
        if (PlayerData != null)
        {
            if (PlayerData.PositionX == Player.position.x && PlayerData.PositionY == Player.position.y && PlayerData.PositionZ == Player.position.z && PlayerData.RotationY == Player.rotation.eulerAngles.y)
            {
                return;
            }

            PlayerData = new PlayerPosition(Player.position.x, Player.position.y, Player.position.z, Player.rotation.eulerAngles.y);
            SendPlayerPosition(PlayerData);
        } 
    }

    void SendPlayerPosition(PlayerPosition Update)
    {
        using (Message PlayerPosition = Message.Create(PacketTags.PlayerPosition, Update))
        {
            Client.SendMessage(PlayerPosition, SendMode.Reliable);
        }
    }

    private void OnDisable()
    {
        //Disconnected, so reset everything.
        PlayerData = null;
        Player = null;
    }
}
