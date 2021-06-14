public static class PacketTags
{
    //Game Server Tags.

    public static readonly ushort SpawnPlayer = 0; //Client sends appearance, Server sends 6 floats.
    public static readonly ushort DespawnPlayer = 1; //Server sends 1 float to clients and SteamID Loginserver.

    public static readonly ushort PlayerPosition = 2; //Client sends 7 floats, Server sends 8 floats.
    public static readonly ushort PlayerEnteredRenderDistance = 14; //Client sends 7 floats, Server sends 8 floats and a string.

    public static readonly ushort Heartbeat = 3; //Server sends empty. (Client uses as a proxy for Pinging).

    public static readonly ushort PlayerCount = 4; //Client requests server player count. Server sends short.

    public static readonly ushort LoginChatServerAuthentication = 8; //Client sends string, encrypted string, server sends bool.

    public static readonly ushort ClientToWorldServerAuthentication = 9; //CLient sends ulong, byte. Server sends ulong to LoginServer, LoginServer responds bool to WorldServer, WorldServer responds bool to Client.

    public static readonly ushort SavePlayerData = 12; //Game Server sends ulong, byte and 4 floats to Login Server.

    public static readonly ushort ChatMessage = 13; //Client sends a byte, string to game server.

    //LoginServer Tags.

    public static readonly ushort RequestLogin = 5; //Client sends LoginRequest. Server responds with bool, if valid, sends characters in the format of byte, string, ushort, byte, int
    public static readonly ushort RequestCreateCharacter = 6; //Client sends string, int. Server responds with a bool.
    public static readonly ushort RequestDeleteCharacter = 7; //Client sends string, server responds with bool.

    public static readonly ushort LoginChatServerVerifiedWorldServerRequest = 10; //LoginServer responds SteamID, bool to WorldServer.

    public static readonly ushort ShutdownServer = 11; //LoginServer sends a shutdown message to a World Server or ALL World Servers.
}

