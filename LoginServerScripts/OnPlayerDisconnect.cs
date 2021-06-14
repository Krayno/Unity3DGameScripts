using UnityEngine;
using DarkRift;
using DarkRift.Server;
using DarkRift.Server.Unity;
using System.Linq;
using System.Collections.Generic;
using System;

public class OnPlayerDisconnect : MonoBehaviour
{
    private XmlUnityServer XmlServer;
    private DarkRiftServer Server;
    private List<DisconnectUpdate> DisconnectUpdates;

    public AccurateClock AccurateClock; //Event Based Timer.

    private class DisconnectUpdate
    {
        public IClient Client { get; set; }

        public DisconnectUpdate(IClient client)
        {
            Client = client;
        }
    }

    private void Start()
    {
        XmlServer = GetComponentInParent<XmlUnityServer>();
        Server = XmlServer.Server;
        DisconnectUpdates = new List<DisconnectUpdate>();

        AccurateClock.Tick.Ticked += EveryTick;

        Server.ClientManager.ClientDisconnected += OnClientDisconnected;
    }

    private void OnClientDisconnected(object sender, ClientDisconnectedEventArgs e)
    {
        DisconnectUpdates.Add(new DisconnectUpdate(e.Client));
    }

    private void EveryTick(object sender, EventTimerTickedArgs e)
    {
        if (DisconnectUpdates.Count > 0)
        {
            foreach (DisconnectUpdate Disconnection in DisconnectUpdates)
            {
                foreach (Player Player in PlayerManager.SteamPlayers.Values)
                {
                    if (Player.Client == Disconnection.Client)
                    {
                        PlayerManager.SteamPlayers.Remove(Player.SteamID);
                        break;
                    }
                }
            }
            DisconnectUpdates.Clear();
        }
    }
}
