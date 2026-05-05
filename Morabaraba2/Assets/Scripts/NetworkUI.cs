using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Lobby scene — connects to Photon and lets players host, join by code,
/// or quick-join (auto-matchmaking).
///
/// SCENE HIERARCHY SETUP:
///
/// Canvas
///   BackgroundImage              (Image - your background sprite)
///   HeaderPanel
///     TitleText                  (TextMeshProUGUI - "MORABARABA")
///     WelcomeText                (TextMeshProUGUI - "Welcome, [username]!")
///     LogoutButton               (Button - "Logout")
///
///   ConnectionPanel              (shown while connecting)
///     ConnectingText             (TextMeshProUGUI - "Connecting to server...")
///     SpinnerImage               (Image - optional loading spinner)
///
///   LobbyPanel                   (shown once connected)
///     PlayerNameInput            (TMP_InputField - pre-filled with username)
///
///     QuickJoinSection
///       QuickJoinTitle           (TextMeshProUGUI - "Quick Play")
///       QuickJoinDescription     (TextMeshProUGUI - "Find an opponent automatically")
///       QuickJoinButton          (Button - "Find Game")
///
///     Divider                    (Image - horizontal line)
///
///     HostSection
///       HostTitle                (TextMeshProUGUI - "Create Private Game")
///       HostButton               (Button - "Host Game")
///       RoomCodeDisplay          (TextMeshProUGUI - shows code after hosting)
///
///     JoinSection
///       JoinTitle                (TextMeshProUGUI - "Join Private Game")
///       RoomCodeInput            (TMP_InputField - "Enter room code")
///       JoinButton               (Button - "Join Game")
///
///     StatusText                 (TextMeshProUGUI - connection messages)
///
///     LocalPlayButton            (Button - "Play Local (Hot-Seat)")
/// </summary>
public class NetworkUI : MonoBehaviourPunCallbacks
{
    [Header("Panels")]
    [SerializeField] private GameObject connectionPanel;
    [SerializeField] private GameObject lobbyPanel;

    [Header("Header")]
    [SerializeField] private TextMeshProUGUI welcomeText;
    [SerializeField] private Button logoutButton;

    [Header("Lobby UI")]
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private Button quickJoinButton;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button localPlayButton;
    [SerializeField] private TMP_InputField roomCodeInput;
    [SerializeField] private TextMeshProUGUI roomCodeDisplay;
    [SerializeField] private TextMeshProUGUI statusText;

    private string loggedInUsername;

    // ── Lifecycle ──────────────────────────────────────────────────────────
    private void Start()
    {
        // Get logged in username
        loggedInUsername = PlayerPrefs.GetString("LoggedInUsername", "Player");

        // Show welcome message
        if (welcomeText != null)
            welcomeText.text = "Welcome, " + loggedInUsername + "!";

        // Pre-fill name input
        if (playerNameInput != null)
            playerNameInput.text = loggedInUsername;

        // Show connecting panel while we connect
        ShowConnecting(true);

        // Wire buttons
        quickJoinButton.onClick.AddListener(QuickJoin);
        hostButton.onClick.AddListener(HostGame);
        joinButton.onClick.AddListener(JoinByCode);
        localPlayButton.onClick.AddListener(PlayLocal);
        if (logoutButton != null) logoutButton.onClick.AddListener(Logout);

        // Connect to Photon
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();

        SetButtonsInteractable(false);
    }

    // ── Photon Callbacks ───────────────────────────────────────────────────
    public override void OnConnectedToMaster()
    {
        ShowConnecting(false);
        SetStatus("Connected. Ready to play!", true);
        SetButtonsInteractable(true);
        PhotonNetwork.JoinLobby();
    }

    public override void OnCreatedRoom()
    {
        string code = PhotonNetwork.CurrentRoom.Name;
        if (roomCodeDisplay != null)
            roomCodeDisplay.text = "Room Code: " + code + "\nShare this with your opponent.";
        SetStatus("Waiting for opponent...", true);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        SetStatus("Failed to create room. Try again.", false);
        SetButtonsInteractable(true);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2 && PhotonNetwork.IsMasterClient)
        {
            SetStatus("Opponent found! Starting game...", true);
            PhotonNetwork.LoadLevel("GameScene");
        }
    }

    public override void OnJoinedRoom()
    {
        PlayerPrefs.SetInt("MyPlayerNumber", 2);
        SetStatus("Joined! Starting game...", true);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        SetStatus("Could not join. Check the room code and try again.", false);
        SetButtonsInteractable(true);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        SetStatus("No open games found. Creating one for you...\nWaiting for opponent.", true);
        string roomCode = Random.Range(1000, 9999).ToString();
        RoomOptions options = new RoomOptions { MaxPlayers = 2, IsVisible = true, IsOpen = true };
        PhotonNetwork.CreateRoom(roomCode, options);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        ShowConnecting(true);
        SetButtonsInteractable(false);
        SetStatus("Disconnected. Reconnecting...", false);
        PhotonNetwork.ConnectUsingSettings();
    }

    // ── Button Actions ─────────────────────────────────────────────────────
    private void QuickJoin()
    {
        if (!SetName()) return;
        SetStatus("Looking for a game...", true);
        SetButtonsInteractable(false);
        PhotonNetwork.JoinRandomRoom();
    }

    private void HostGame()
    {
        if (!SetName()) return;

        string roomCode = Random.Range(1000, 9999).ToString();
        RoomOptions options = new RoomOptions
        {
            MaxPlayers = 2,
            IsVisible = true,
            IsOpen = true
        };

        SetStatus("Creating room...", true);
        SetButtonsInteractable(false);
        PhotonNetwork.CreateRoom(roomCode, options);
    }

    private void JoinByCode()
    {
        string code = roomCodeInput.text.Trim();
        if (string.IsNullOrEmpty(code))
        {
            SetStatus("Please enter a room code.", false);
            return;
        }
        if (!SetName()) return;

        SetStatus("Joining room " + code + "...", true);
        SetButtonsInteractable(false);
        PhotonNetwork.JoinRoom(code);
    }

    private void PlayLocal()
    {
        string name = playerNameInput != null ? playerNameInput.text.Trim() : loggedInUsername;
        if (string.IsNullOrEmpty(name)) name = "Player 1";

        PlayerPrefs.SetString("P1", name);
        PlayerPrefs.SetString("P2", "Player 2");
        PlayerPrefs.SetInt("MyPlayerNumber", 0);

        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }

    private void Logout()
    {
        PlayerPrefs.DeleteKey("LoggedInEmail");
        PlayerPrefs.DeleteKey("LoggedInUsername");
        if (PhotonNetwork.IsConnected) PhotonNetwork.Disconnect();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    // ── Helpers ───────────────────────────────────────────────────────────
    private bool SetName()
    {
        string name = playerNameInput != null ? playerNameInput.text.Trim() : loggedInUsername;
        if (string.IsNullOrEmpty(name))
        {
            SetStatus("Please enter your name.", false);
            return false;
        }
        PhotonNetwork.NickName = name;
        PlayerPrefs.SetString("P1", name);
        PlayerPrefs.SetInt("MyPlayerNumber", 1);
        return true;
    }

    private void SetStatus(string message, bool good)
    {
        if (statusText == null) return;
        statusText.text = message;
        statusText.color = good ? Color.green : Color.red;
    }

    private void ShowConnecting(bool connecting)
    {
        if (connectionPanel != null) connectionPanel.SetActive(connecting);
        if (lobbyPanel != null) lobbyPanel.SetActive(!connecting);
    }

    private void SetButtonsInteractable(bool value)
    {
        if (quickJoinButton != null) quickJoinButton.interactable = value;
        if (hostButton != null) hostButton.interactable = value;
        if (joinButton != null) joinButton.interactable = value;
        if (localPlayButton != null) localPlayButton.interactable = value;
    }
}