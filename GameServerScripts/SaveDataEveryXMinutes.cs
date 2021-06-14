using DarkRift;
using DarkRift.Client.Unity;
using DarkRift.Server;
using DarkRift.Server.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveDataEveryXMinutes : MonoBehaviour
{
    public UnityClient LoginServer;

    [HideInInspector]
    public bool SavingPlayerData;

    public int SaveEveryXMinutes;

    void Start()
    {
        SavingPlayerData = true;
        StartCoroutine("SaveDataEveryX");

    }

    IEnumerator SaveDataEveryX()
    {
        yield return new WaitForSecondsRealtime(SaveEveryXMinutes * 60);

        while (SavingPlayerData)
        {
            Debug.Log("Sending Player Data to Login Server.");
            using (DarkRiftWriter Writer = DarkRiftWriter.Create())
            {
                Writer.Write(LoginServerConnecter.EncryptedSessionID);

                foreach (Player Player in PlayerManager.IClientPlayers.Values)
                {
                    Writer.Write(new SaveDataFromWorldServer(Player.SteamID, Player.CharacterSlot, Player.Appearance, Player.LastGroundedPosition.x, Player.LastGroundedPosition.y, Player.LastGroundedPosition.z, Player.RotationY, Player.Name, Player.TotalLevel));

                    using (Message PlayerUpdate = Message.Create(PacketTags.SavePlayerData, Writer))
                    {
                        LoginServer.SendMessage(PlayerUpdate, SendMode.Reliable);
                    }
                }
            }
            Debug.Log("All player data has been sent.");

            yield return new WaitForSecondsRealtime(SaveEveryXMinutes * 60);
        }
    }

}
