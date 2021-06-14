using UnityEngine;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using System.Collections.Generic;
using System;
using System.IO;
using System.Collections;
using System.Linq;

public class NetworkPlayerUpdatePosition : MonoBehaviour
{
    public PlayerManager PlayerManager;

    private List<NetworkPlayerInterpolation> Interpolations;
    private float PlayerSpeed;
    private UnityClient Client; //The client that connects to the server.

    private class NetworkPlayerInterpolation
    {
        public ushort ID { get; set; }
        public Transform NetworkPlayer { get; set; }
        public Vector3 DestinationPosition { get; set; }
        public Vector3 DestinationRotation { get; set; }
        public float NetworkPlayerSpeed { get; set; }
        public float InterpolationCompletionTime { get; set; }
        public Animator Animator;

        public NetworkPlayerInterpolation( ushort ID, Transform networkPlayer, Vector3 destinationPosition, Vector3 destinationRotation, float speed)
        {
            this.ID = ID;
            NetworkPlayer = networkPlayer;
            DestinationPosition = destinationPosition;
            DestinationRotation = destinationRotation;
            NetworkPlayerSpeed = speed;
            InterpolationCompletionTime = -1;
            Animator = networkPlayer.gameObject.GetComponent<NewCharacterReferences>().Animator;
            
        }
    }

    void Awake()
    {
        Interpolations = new List<NetworkPlayerInterpolation>();
        PlayerSpeed = ClientGlobals.PlayerSpeed;

        Client = transform.parent.GetComponent<UnityClient>();
        Client.MessageReceived += OnMessageReceived;
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        using (Message Message = e.GetMessage())
        {
            if (Message.Tag == PacketTags.PlayerPosition)
            {
                UpdateNetworkPlayerPositions(Message);
            }
        }
    }

    private void UpdateNetworkPlayerPositions(Message Message)
    {
        PlayerWithinRenderDistance PlayerData;

        try
        {
            PlayerData = Message.Deserialize<PlayerWithinRenderDistance>();
        }
        catch (EndOfStreamException)
        {
            Debug.Log($"{ClientGlobals.WorldServer.Address} sent an invalid 'PlayerPosition' Packet.");
            return;
        }

        if (PlayerManager.Players.ContainsKey(PlayerData.ClientID)) //Check if the player is spawned on the client.
        {
            Player Player = PlayerManager.Players[PlayerData.ClientID];

            bool Present = Interpolations.Exists(x => Player.GameObject.transform == x.NetworkPlayer);
            float NetworkPlayerSpeed = PlayerSpeed - (PlayerSpeed - (Vector3.Distance(Player.GameObject.transform.position, new Vector3(PlayerData.PositionX, PlayerData.PositionY, PlayerData.PositionZ)) * 10));

            if (Present)
            {
                //Remove the player if their latest position update is greater then the render distance.
                if (Vector3.Distance(PlayerManager.Players[Client.ID].GameObject.transform.position, new Vector3(PlayerData.PositionX, PlayerData.PositionY, PlayerData.PositionZ)) > ClientGlobals.RenderDistance)
                {
                    Destroy(Player.GameObject);
                    PlayerManager.Players.Remove(PlayerData.ClientID);
                    return;
                }

                NetworkPlayerInterpolation Interpolation = Interpolations.Find(x => Player.GameObject.transform == x.NetworkPlayer);

                Interpolation.DestinationPosition = new Vector3(PlayerData.PositionX, PlayerData.PositionY, PlayerData.PositionZ);
                Interpolation.DestinationRotation = new Vector3(0, PlayerData.RotationY, 0);
                Interpolation.NetworkPlayerSpeed = NetworkPlayerSpeed;

                if (Interpolation.DestinationPosition != Interpolation.NetworkPlayer.position)
                {
                    Interpolation.InterpolationCompletionTime = -1;
                }

            }
            else
            {
                Interpolations.Add(new NetworkPlayerInterpolation(PlayerData.ClientID, Player.GameObject.transform,
                                   new Vector3(PlayerData.PositionX, PlayerData.PositionY, PlayerData.PositionZ), new Vector3(0, PlayerData.RotationY, 0), NetworkPlayerSpeed));
            }
        }

    }

    private void Update()
    {
        if (Interpolations.Count > 0)
        {
            foreach (NetworkPlayerInterpolation Interpolation in Interpolations)
            {
                if (Interpolation.NetworkPlayer)
                {
                    float RotateSpeed = Quaternion.Angle(Interpolation.NetworkPlayer.rotation, Quaternion.Euler(Interpolation.DestinationRotation));

                    //Animate player when moving.
                    if (Interpolation.NetworkPlayer.position != Interpolation.DestinationPosition && !Interpolation.Animator.GetBool("isRunning"))
                    {
                        Debug.Log(Interpolation.Animator.GetBool("isRunning"));
                        Interpolation.Animator.SetBool("isRunning", true);
                    }

                    Interpolation.NetworkPlayer.gameObject.transform.rotation = Quaternion.RotateTowards(Interpolation.NetworkPlayer.rotation,
                                                                                Quaternion.Euler(Interpolation.DestinationRotation),
                                                                                (RotateSpeed / 90) * 1000 * Time.deltaTime);

                    Interpolation.NetworkPlayer.gameObject.transform.position = Vector3.MoveTowards(Interpolation.NetworkPlayer.position,
                                                                                Interpolation.DestinationPosition,
                                                                                Interpolation.NetworkPlayerSpeed * Time.deltaTime);

                    if (Vector3.Distance(Interpolation.NetworkPlayer.position, Interpolation.DestinationPosition) < Interpolation.NetworkPlayerSpeed * Time.deltaTime)
                    {
                        Interpolation.NetworkPlayer.position = Interpolation.DestinationPosition;
                    }

                    if (Quaternion.Angle(Interpolation.NetworkPlayer.rotation, Quaternion.Euler(Interpolation.DestinationRotation)) < (RotateSpeed / 90) * 1000 * Time.deltaTime)
                    {
                        Interpolation.NetworkPlayer.rotation = Quaternion.Euler(Interpolation.DestinationRotation);
                    }

                    if (Interpolation.NetworkPlayer.position == Interpolation.DestinationPosition &&
                        Interpolation.NetworkPlayer.rotation == Quaternion.Euler(Interpolation.DestinationRotation) &&
                        Interpolation.InterpolationCompletionTime == -1)
                    {
                        Interpolation.InterpolationCompletionTime = Time.time;
                    }
                }
            }

            List<NetworkPlayerInterpolation> KeepList = new List<NetworkPlayerInterpolation>();
            foreach (NetworkPlayerInterpolation x in Interpolations)
            {
                if (x.InterpolationCompletionTime == -1 || Time.time < 0.1f + x.InterpolationCompletionTime)
                {
                    KeepList.Add(x);
                }
                else
                {
                    x.Animator.SetBool("isRunning", false);
                }
            }
            Interpolations = KeepList;
        }
    }

    private void OnDisable()
    {
        //Disconnected, so reset everything.
        Interpolations = new List<NetworkPlayerInterpolation>();
    }
}
