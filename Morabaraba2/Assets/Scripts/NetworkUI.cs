using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Lobby screen. Three ways to play:
///   1. Host — creates a private room, shows a code to share
///   2. Join by code — Player 2 types the code to join a specific room
///   3. Quick Join — automatically finds any open room (meets the
///      "just connect and play" requirement from the project brief)
///
/// INSPECTOR SETUP:
///   - Attach to a GameObject in LobbyScene
///   - Assign all serialized fields
///   - PhotonServerSettings must have your App ID set
/// </summary>
public class NetworkUI : MonoBehaviourPunCallbacks
{
    [Header("UI Elements")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button quickJoinButton;          // Auto-finds any open game
    [SerializeField] private TMP_InputField roomCodeInput;    // For joining by code
    [SerializeField] private TMP_InputField playerNameInput;  // Player name entry
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI roomCodeDisplay; // Shows code to host

    private void Start()
    {
        statusText.text = "Connecting to server...";
        SetButtonsInteractable(false);

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();

        hostButton.onClick.AddListener(HostGame);
        joinButton.onClick.AddListener(JoinByCode);
        quickJoinButton.onClick.AddListener(QuickJoin);
    }

    public override void OnConnectedToMaster()
    {
        statusText.text = "Connected. Enter your name to play.";
        SetButtonsInteractable(true);
        PhotonNetwork.JoinLobby();
    }

    // ── Host ──────────────────────────────────────────────────────────────
    private void HostGame()
    {
        if (!SetName()) return;

        string roomCode = Random.Range(1000, 9999).ToString();

        RoomOptions options = new RoomOptions
        {
            MaxPlayers = 2,
            IsVisible = true,   // Visible so Quick Join can find it
            IsOpen = true
        };

        statusText.text = "Creating room...";
        SetButtonsInteractable(false);

        PhotonNetwork.CreateRoom(roomCode, options);
    }

    public override void OnCreatedRoom()
    {
        string code = PhotonNetwork.CurrentRoom.Name;
        roomCodeDisplay.text = "Room Code: " + code;
        statusText.text = "Waiting for opponent...\nShare code: " + code;
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        statusText.text = "Failed to create room: " + message;
        SetButtonsInteractable(true);
    }

    // ── Join by code ──────────────────────────────────────────────────────
    private void JoinByCode()
    {
        string code = roomCodeInput.text.Trim();
        if (string.IsNullOrEmpty(code))
        {
            statusText.text = "Please enter a room code.";
            return;
        }
        if (!SetName()) return;

        statusText.text = "Joining room " + code + "...";
        SetButtonsInteractable(false);
        PhotonNetwork.JoinRoom(code);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        statusText.text = "Could not join. Check the code and try again.";
        SetButtonsInteractable(true);
    }

    // ── Quick Join (auto-matchmaking) ─────────────────────────────────────
    // Finds any available open room automatically.
    // If no room exists, creates one and waits for an opponent.
    // This satisfies the requirement for players to just "log in and play".
    private void QuickJoin()
    {
        if (!SetName()) return;

        statusText.text = "Looking for a game...";
        SetButtonsInteractable(false);

        // Try to join a random open room
        PhotonNetwork.JoinRandomRoom();
    }

    // Called when JoinRandomRoom finds no available rooms
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        statusText.text = "No open games found. Creating one for you...\nWaiting for opponent.";

        // No room available — create one and wait
        string roomCode = Random.Range(1000, 9999).ToString();
        RoomOptions options = new RoomOptions
        {
            MaxPlayers = 2,
            IsVisible = true,
            IsOpen = true
        };
        PhotonNetwork.CreateRoom(roomCode, options);
    }

    // ── Both players connected ────────────────────────────────────────────
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2 && PhotonNetwork.IsMasterClient)
        {
            statusText.text = "Opponent found! Starting game...";
            PhotonNetwork.LoadLevel("GameScene");
        }
    }

    public override void OnJoinedRoom()
    {
        // Player 2 joined — save their number
        PlayerPrefs.SetInt("MyPlayerNumber", 2);
        PlayerPrefs.SetString("P2", PhotonNetwork.NickName);
        statusText.text = "Joined! Waiting for game to start...";

        // If room is already full when we join (edge case), game starts via host
    }

    // ── Disconnect handling ───────────────────────────────────────────────
    public override void OnDisconnected(DisconnectCause cause)
    {
        statusText.text = "Disconnected. Reconnecting...";
        SetButtonsInteractable(false);
        PhotonNetwork.ConnectUsingSettings();
    }

    // ── Helpers ───────────────────────────────────────────────────────────
    private bool SetName()
    {
        string name = playerNameInput.text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            statusText.text = "Please enter your name first.";
            return false;
        }
        PhotonNetwork.NickName = name;
        PlayerPrefs.SetString("MyName", name);

        // Host is always Player 1
        if (PhotonNetwork.IsMasterClient || !PhotonNetwork.InRoom)
        {
            PlayerPrefs.SetString("P1", name);
            PlayerPrefs.SetInt("MyPlayerNumber", 1);
        }
        return true;
    }

    private void SetButtonsInteractable(bool value)
    {
        hostButton.interactable = value;
        joinButton.interactable = value;
        quickJoinButton.interactable = value;
    }
}