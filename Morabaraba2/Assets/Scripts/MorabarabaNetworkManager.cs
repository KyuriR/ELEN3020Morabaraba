using Mirror;
using UnityEngine;

/// <summary>
/// Extends Mirror's NetworkManager to handle player connection events.
/// 
/// INSPECTOR SETUP:
///   - Create an empty GameObject called "NetworkManager" in your LobbyScene
///   - Add this script as a component
///   - Also add the Transport component (right-click → Mirror → KcpTransport)
///   - Assign the Transport to the Transport field on this component
///   - Set "Offline Scene" to LobbyScene
///   - Set "Online Scene" to GameScene
/// </summary>
public class MorabarabaNetworkManager : NetworkManager
{
    // Called on the server when a client connects
    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);

        // Only allow 2 players
        if (numPlayers >= 2)
        {
            conn.Disconnect();
            return;
        }

        Debug.Log("Client connected. Total players: " + (numPlayers + 1));
    }

    // Called on the server when the second player joins
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
        Debug.Log("Player added. Total: " + numPlayers);
    }

    // Called on server when a client disconnects
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
        Debug.Log("Client disconnected.");
    }

    // Called on the client when connected to server
    public override void OnClientConnect()
    {
        base.OnClientConnect();
        Debug.Log("Connected to server.");
    }

    // Called on the client when disconnected
    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
        Debug.Log("Disconnected from server.");
    }
}
