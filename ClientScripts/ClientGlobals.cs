using DarkRift.Client.Unity;
using System.Collections.Generic;

public static class ClientGlobals
{
    public const string ClientVersion = "0.0.1";
    public const float RenderDistance = 50;
    public const float PlayerSpeed = 6;
    public const float Gravity = -5;
    public const float JumpSpeed = 1.75f;
    public const float TickRate = 0.1f;

    public static UnityClient WorldServer = null;
    public static ulong SteamID;

    public static List<Character> Characters = new List<Character>();
    public static Character SelectedCharacter = null;

    public static bool InitialResolutionSetup = false;

    public static bool DisableCameraZoom = false;

}
