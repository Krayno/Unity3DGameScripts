using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using DarkRift.Server;
using DarkRift.Server.Unity;

public class Heartbeat : MonoBehaviour
{
    private XmlUnityServer XmlServer;
    private DarkRiftServer Server;

    public AccurateClock AccurateClock; //Event Based Timer.

    private float Counter;

    private void Start()
    {
        XmlServer = GetComponentInParent<XmlUnityServer>();
        Server = XmlServer.Server;
        Counter = 0;

        AccurateClock.Tick.Ticked += EveryTick;
    }

    private void EveryTick(object sender, EventTimerTickedArgs e) //Send Heartbeat every 5 seconds.
    {
        Counter += 1;
        if (Counter >= 50)
        {
            if (Server.ClientManager.GetAllClients().Length > 0)
            {
                using (Message HeartBeat = Message.CreateEmpty(PacketTags.Heartbeat))
                {
                    foreach (IClient Client in Server.ClientManager.GetAllClients())
                    {
                        Client.SendMessage(HeartBeat, SendMode.Reliable);
                    }
                }
            }
            Counter = 0;
        }
    }
}
