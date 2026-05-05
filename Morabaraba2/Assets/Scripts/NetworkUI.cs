using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Simplified Lobby scene UI.
///
/// INSPECTOR SETUP — assign these fields:
///   - playerNameInput    TMP_InputField
///   - quickJoinButton    Button
///   - hostButton         Button
///   - joinButton         Button
///   - localPlayButton    Button
///   - roomCodeInput      TMP_InputField
///   - roomCodeDisplay    TextMeshProUGUI
///   - statusText         TextMeshProUGUI
/// </summary>
public class NetworkUI : MonoBehaviourPunCallbacks
{
    [Header("UI Elements")]
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private Button quickJoinButton;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button localPlayButton;
    [SerializeField] private TMP_InputField roomCodeInput;
    [SerializeField] private TextMeshProUGUI roomCodeDisplay;
    [SerializeField] private TextMeshProUGUI statusText;

    private void Start()
    {
        // Pre-fill name from login
        string savedName = PlayerPrefs.GetString("LoggedInUsername", "");
        if (!string.IsNullOrEmpty(savedName) && playerNameInput != null)
            playerNameInput.text = savedName;

        SetButtonsInteractable(false);
        SetStatus("Connecting to server...", true);

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();

        quickJoinButton.onClick.AddListener(QuickJoin);
        hostButton.onClick.AddListener(HostGame);
        joinButton.onClick.AddListener(JoinByCode);
        if (localPlayButton != null) localPlayButton.onClick.AddListener(PlayLocal);
    }

    // ── Photon Callbacks ───────────────────────────────────────────────────
    public override void OnConnectedToMaster()
    {
        SetStatus("Connected. Ready to play!", true);
        SetButtonsInteractable(true);
        PhotonNetwork.JoinLobby();
    }

    public override void OnCreatedRoom()
    {
        string code = PhotonNetwork.CurrentRoom.Name;
        if (roomCodeDisplay != null)
            roomCodeDisplay.text =  code;
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
        SetStatus("No open games found. Creating one...\nWaiting for opponent.", true);
        string roomCode = Random.Range(1000, 9999).ToString();
        RoomOptions options = new RoomOptions { MaxPlayers = 2, IsVisible = true, IsOpen = true };
        PhotonNetwork.CreateRoom(roomCode, options);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
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
        RoomOptions options = new RoomOptions { MaxPlayers = 2, IsVisible = true, IsOpen = true };
        SetStatus("Creating room...", true);
        SetButtonsInteractable(false);
        PhotonNetwork.CreateRoom(roomCode, options);
    }

    private void JoinByCode()
    {
        string code = roomCodeInput != null ? roomCodeInput.text.Trim() : "";
        if (string.IsNullOrEmpty(code)) { SetStatus("Please enter a room code.", false); return; }
        if (!SetName()) return;
        SetStatus("Joining room " + code + "...", true);
        SetButtonsInteractable(false);
        PhotonNetwork.JoinRoom(code);
    }

    private void PlayLocal()
    {
        string name = playerNameInput != null ? playerNameInput.text.Trim() : "Player 1";
        if (string.IsNullOrEmpty(name)) name = "Player 1";
        PlayerPrefs.SetString("P1", name);
        PlayerPrefs.SetString("P2", "Player 2");
        PlayerPrefs.SetInt("MyPlayerNumber", 0);
        SceneManager.LoadScene("GameScene");
    }

    // ── Helpers ────────────────────────────────────────────────────────────
    private bool SetName()
    {
        string name = playerNameInput != null ? playerNameInput.text.Trim() : "";
        if (string.IsNullOrEmpty(name)) { SetStatus("Please enter your name.", false); return false; }
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

    private void SetButtonsInteractable(bool value)
    {
        if (quickJoinButton != null) quickJoinButton.interactable = value;
        if (hostButton != null) hostButton.interactable = value;
        if (joinButton != null) joinButton.interactable = value;
        if (localPlayButton != null) localPlayButton.interactable = value;
    }
}