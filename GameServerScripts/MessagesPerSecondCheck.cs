using DarkRift.Server;
using DarkRift.Server.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessagesPerSecondCheck : MonoBehaviour
{
    public int AllowedMessagesPerSecond;

    [HideInInspector]
    public bool CheckMessagesPerSecond;

    private XmlUnityServer XmlServer;
    private DarkRiftServer Server;

    private void Start()
    {
        XmlServer = GetComponentInParent<XmlUnityServer>();
        Server = XmlServer.Server;

        CheckMessagesPerSecond = true;
        StartCoroutine("CheckEveryFive");
    }

    IEnumerator CheckEveryFive()
    {
        while (CheckMessagesPerSecond)
        {
            IClient[] AllClients = Server.ClientManager.GetAllClients();
            foreach (IClient Client in AllClients)
            {
                TimeSpan TimeDelta = DateTime.Now.Subtract(Client.ConnectionTime);
                double ConnectionLength = Math.Round(TimeDelta.TotalSeconds, 2);
                if (TimeDelta.TotalSeconds > 1)
                {
                    if (Math.Round((1 / (ConnectionLength / Client.MessagesReceived)), 2) >= AllowedMessagesPerSecond)
                    {
                        Client.Strike($"Sent more than {AllowedMessagesPerSecond} Messages to the login server a second.");
                    }
                }
            }

            yield return new WaitForSecondsRealtime(5);
        }
    }
}

