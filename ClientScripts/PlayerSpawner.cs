using UnityEngine;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using System;
using UnityEngine.SceneManagement;
using System.IO;

public class PlayerSpawner : MonoBehaviour
{
    public PlayerManager PlayerManager;

    private UnityClient Client; //The client that connects to the server.
    public GameObject ControllablePrefab; //Player.
    public GameObject NetworkPrefab; //Other players.
    public ColoursScriptableObject Colours; //Colours

    Color[] ColoursArray;

    private void Awake()
    {
        Client = transform.parent.GetComponent<UnityClient>();
        Client.MessageReceived += OnMessageReceived;
    }

    private void OnEnable()
    {
        //Send Message to SelectedServer that Client is ready to spawn.

        using (Message Message = Message.CreateEmpty(PacketTags.SpawnPlayer))
        {
            ClientGlobals.WorldServer.SendMessage(Message, SendMode.Reliable);
        }

    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        using (Message Message = e.GetMessage())
        {
            if (Message.Tag == PacketTags.SpawnPlayer)
            {
                SpawnPlayer(Message);
            }
            if (Message.Tag == PacketTags.PlayerEnteredRenderDistance)
            {
                SpawnPlayerWithinRenderDistance(Message);
            }
        }
    }

    private void SpawnPlayer(Message Message)
    {
        GameObject NewPlayer;

        using (DarkRiftReader Reader = Message.GetReader())
        {
            while (Reader.Position < Reader.Length) //Loops through the data until it reaches the end.
            {
                PlayerEnteredRenderDistance PlayerData;

                try
                {
                    PlayerData = Reader.ReadSerializable<PlayerEnteredRenderDistance>();
                }
                catch (EndOfStreamException)
                {
                    Debug.Log($"{ClientGlobals.WorldServer.Address} sent an invalid 'PlayerEnteredRenderDistance' Packet.");
                    return;
                }

                if (!PlayerManager.Players.ContainsKey(PlayerData.ClientID)) //Check if the player has already been added.
                {
                    if (PlayerData.ClientID == Client.ID)
                    {
                        NewPlayer = Instantiate(ControllablePrefab, new Vector3(PlayerData.PositionX, PlayerData.PositionY, PlayerData.PositionZ), Quaternion.Euler(0, PlayerData.RotationY, 0)); //Spawn the player.
                    }
                    else
                    {
                        NewPlayer = Instantiate(NetworkPrefab, new Vector3(PlayerData.PositionX, PlayerData.PositionY, PlayerData.PositionZ), Quaternion.Euler(0, PlayerData.RotationY, 0)); //Spawn the new player.
                    }

                    //Retrieve the index of the colours and hairstyles from the appearance long.
                    int Gender = int.Parse(PlayerData.Appearance.ToString().Substring(0, 1));
                    int SkinColour = int.Parse(PlayerData.Appearance.ToString().Substring(1, 2));
                    int UnderwearColour = int.Parse(PlayerData.Appearance.ToString().Substring(3, 2));
                    int ScleraColour = int.Parse(PlayerData.Appearance.ToString().Substring(5, 2));
                    int LeftEyeColour = int.Parse(PlayerData.Appearance.ToString().Substring(7, 2));
                    int RightEyeColour = int.Parse(PlayerData.Appearance.ToString().Substring(9, 2));
                    int BraColour = Gender == 1 ? SkinColour : UnderwearColour; //If male, set bra to skin colour.
                    int MouthColour = int.Parse(PlayerData.Appearance.ToString().Substring(13, 2));
                    int EyebrowColour = int.Parse(PlayerData.Appearance.ToString().Substring(15, 2));
                    int HairStyle = int.Parse(PlayerData.Appearance.ToString().Substring(17, 2));

                    //Get the model.
                    SkinnedMeshRenderer Model = NewPlayer.GetComponent<NewCharacterReferences>().Model;

                    //Colour the model.
                    Model.materials[0].color = Colours.Colours[SkinColour];
                    Model.materials[1].color = Colours.Colours[UnderwearColour];
                    Model.materials[2].color = Colours.Colours[ScleraColour];
                    Model.materials[3].color = Colours.Colours[LeftEyeColour];
                    Model.materials[4].color = Colours.Colours[RightEyeColour];
                    Model.materials[5].color = Colours.Colours[BraColour];
                    Model.materials[6].color = Colours.Colours[MouthColour];
                    Model.materials[7].color = Colours.Colours[EyebrowColour];

                    //Set the hairstyle of the model and colour it.
                    NewPlayer.GetComponent<NewCharacterReferences>().Hairstyles[HairStyle].gameObject.SetActive(true);
                    NewPlayer.GetComponent<NewCharacterReferences>().Hairstyles[HairStyle].material.color = Colours.Colours[EyebrowColour];

                    //Set name.
                    NewPlayer.GetComponent<NewCharacterReferences>().Name.text = PlayerData.Name;

                    PlayerManager.Players.Add(PlayerData.ClientID, new Player(NewPlayer)); //Add the player to list of players.
                }
            }
        }
    }

    private void SpawnPlayerWithinRenderDistance(Message Message)
    {
        PlayerEnteredRenderDistance PlayerData;

        try
        {
            PlayerData = Message.Deserialize<PlayerEnteredRenderDistance>();
        }
        catch (EndOfStreamException)
        {
            Debug.Log($"{ClientGlobals.WorldServer.Address} sent an invalid 'PlayerEnteredRenderDistance' Packet.");
            return;
        }

        if (!PlayerManager.Players.ContainsKey(PlayerData.ClientID) && PlayerManager.Players.ContainsKey(Client.ID))
        {
            if (Vector3.Distance(new Vector3(PlayerData.PositionX, PlayerData.PositionY, PlayerData.PositionZ), PlayerManager.Players[Client.ID].GameObject.transform.position) <= ClientGlobals.RenderDistance)
            {
                GameObject NewPlayer = Instantiate(NetworkPrefab, new Vector3(PlayerData.PositionX, PlayerData.PositionY, PlayerData.PositionZ), Quaternion.Euler(0, PlayerData.RotationY, 0));

                //Retrieve the index of the colours and hairstyles from the appearance long.
                int Gender = int.Parse(PlayerData.Appearance.ToString().Substring(0, 1));
                int SkinColour = int.Parse(PlayerData.Appearance.ToString().Substring(1, 2));
                int UnderwearColour = int.Parse(PlayerData.Appearance.ToString().Substring(3, 2));
                int ScleraColour = int.Parse(PlayerData.Appearance.ToString().Substring(5, 2));
                int LeftEyeColour = int.Parse(PlayerData.Appearance.ToString().Substring(7, 2));
                int RightEyeColour = int.Parse(PlayerData.Appearance.ToString().Substring(9, 2));
                int BraColour = Gender == 0 ? SkinColour : UnderwearColour; //If male, set bra to skin colour.
                int MouthColour = int.Parse(PlayerData.Appearance.ToString().Substring(11, 2));
                int EyebrowColour = int.Parse(PlayerData.Appearance.ToString().Substring(13, 2));
                int HairStyle = int.Parse(PlayerData.Appearance.ToString().Substring(15, 2));

                //Get the model.
                SkinnedMeshRenderer Model = NewPlayer.GetComponent<NewCharacterReferences>().Model;

                //Colour the model.
                Model.materials[0].color = Colours.Colours[SkinColour];
                Model.materials[1].color = Colours.Colours[UnderwearColour];
                Model.materials[2].color = Colours.Colours[ScleraColour];
                Model.materials[3].color = Colours.Colours[LeftEyeColour];
                Model.materials[4].color = Colours.Colours[RightEyeColour];
                Model.materials[5].color = Colours.Colours[BraColour];
                Model.materials[6].color = Colours.Colours[MouthColour];
                Model.materials[7].color = Colours.Colours[EyebrowColour];

                //Set the hairstyle of the model and colour it.
                NewPlayer.GetComponent<NewCharacterReferences>().Hairstyles[HairStyle].gameObject.SetActive(true);
                NewPlayer.GetComponent<NewCharacterReferences>().Hairstyles[HairStyle].material.color = Colours.Colours[EyebrowColour];

                //Set name.
                NewPlayer.GetComponent<NewCharacterReferences>().Name.text = PlayerData.Name;

                //Add the player manager.
                PlayerManager.Players.Add(PlayerData.ClientID, new Player(NewPlayer));
            }
        }

    }
}
