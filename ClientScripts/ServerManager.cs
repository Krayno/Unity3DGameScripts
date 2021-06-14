using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift.Client.Unity;
using UnityEngine.SceneManagement;
using System;
using DarkRift.Client;
using UnityEngine.UI;

public class ServerManager : MonoBehaviour
{
    public static ServerManager Instance { get; private set; }
    public Dictionary<string, UnityClient> Clients;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(gameObject);

        //Initialise List of Clients.
        Clients = new Dictionary<string, UnityClient>();

        //Populate the Client List and subscribe to LoginServer disconnections.
        foreach (Transform ChildTransform in transform)
        {
            if (ChildTransform != transform)
            {
                Clients.Add(ChildTransform.name, ChildTransform.GetComponent<UnityClient>());
                if (ChildTransform.name == "LoginServer")
                {
                    ChildTransform.GetComponent<UnityClient>().Disconnected += OnLoginServerDisconnection;
                }
            }
        }

        //Subscribe to Scene Change Events.
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnLoginServerDisconnection(object sender, DisconnectedEventArgs e)
    {
        SceneManager.LoadScene("Login");

        //Disconnect from all servers except the login server and return to login screen.
        foreach (UnityClient Client in Clients.Values)
        {
            if (Client != Clients["LoginServer"])
            {
                if (Client.ConnectionState == DarkRift.ConnectionState.Connected)
                {
                    Client.Disconnect();
                }
            }
        }
        //Remove all characters and reset World Server.
        ClientGlobals.WorldServer = null;
        ClientGlobals.Characters.Clear();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode arg1)
    {
        if (scene.name == "GameWorld") //Disable all WorldServer scripts except the ClientGlobals WorldServer.
        {
            foreach (UnityClient WorldServer in Clients.Values)
            {
                if (WorldServer.gameObject.name != "LoginServer")
                {
                    foreach (Transform WorldServerScript in WorldServer.transform)
                    {
                        if (WorldServerScript.gameObject != WorldServer.gameObject) //Don't disable the WorldServer itself.
                        {
                            WorldServerScript.gameObject.SetActive(false);
                        }
                    }
                }
            }

            foreach (Transform WorldServerScript in ClientGlobals.WorldServer.transform) //Enable Selected WorldServer scripts.
            {
                WorldServerScript.gameObject.SetActive(true);
            }

            //Subscribe to ClientGlobalsWorldServer disconnection events.
            ClientGlobals.WorldServer.Disconnected += OnCurrentWorldServerDisconnection;
        }
        if (scene.name == "CharacterSelection") //Disable all WorldServerScripts.
        {
            foreach (UnityClient WorldServer in Clients.Values)
            {
                if (WorldServer.gameObject.name != "LoginServer")
                {
                    foreach (Transform WorldServerScript in WorldServer.transform)
                    {
                        if (WorldServerScript.gameObject != WorldServer.gameObject) //Don't disable the WorldServer itself.
                        {
                            WorldServerScript.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }
    }

    private void OnCurrentWorldServerDisconnection(object sender, DisconnectedEventArgs e)
    {
        //Switch back to character selection, unsubscribe from disconnection events.
        ClientGlobals.WorldServer.Disconnected -= OnCurrentWorldServerDisconnection;
        ClientGlobals.WorldServer = null;

        SceneManager.LoadSceneAsync("CharacterSelection");
    }
}
