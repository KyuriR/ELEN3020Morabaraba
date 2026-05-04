using Mirror;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Lobby UI for Mirror networking.
/// Player 1 clicks Host - their IP address is shown.
/// Player 2 enters that IP and clicks Join.
///
/// INSPECTOR SETUP:
///   - Attach to a GameObject in LobbyScene
///   - Assign all fields in the Inspector
///   - Make sure MorabarabaNetworkManager exists in the scene
/// </summary>
public class NetworkUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private TMP_InputField ipAddressInput;   // Player 2 enters host IP here
    [SerializeField] private TMP_InputField playerNameInput;  // Both players enter name
    [SerializeField] private TextMeshProUGUI statusText;      // Connection status
    [SerializeField] private TextMeshProUGUI ipDisplay;       // Shows host's IP to Player 1

    private void Start()
    {
        hostButton.onClick.AddListener(HostGame);
        joinButton.onClick.AddListener(JoinGame);

        // Show this machine's IP so Player 1 can share it
        if (ipDisplay != null)
            ipDisplay.text = "Your IP: " + GetLocalIP();

        statusText.text = "Enter your name, then host or join.";
    }

    private void HostGame()
    {
        string name = playerNameInput.text.Trim();
        if (string.IsNullOrEmpty(name)) name = "Player 1";

        PlayerPrefs.SetString("P1", name);
        PlayerPrefs.SetString("MyName", name);
        PlayerPrefs.SetInt("MyPlayerNumber", 1);

        statusText.text = "Hosting... Share your IP with Player 2.";
        hostButton.interactable = false;
        joinButton.interactable = false;

        // Start as host (server + client combined)
        NetworkManager.singleton.StartHost();
    }

    private void JoinGame()
    {
        string ip = ipAddressInput.text.Trim();
        if (string.IsNullOrEmpty(ip))
        {
            statusText.text = "Please enter the host's IP address.";
            return;
        }

        string name = playerNameInput.text.Trim();
        if (string.IsNullOrEmpty(name)) name = "Player 2";

        PlayerPrefs.SetString("P2", name);
        PlayerPrefs.SetString("MyName", name);
        PlayerPrefs.SetInt("MyPlayerNumber", 2);

        statusText.text = "Connecting to " + ip + "...";
        hostButton.interactable = false;
        joinButton.interactable = false;

        // Set the IP and connect
        NetworkManager.singleton.networkAddress = ip;
        NetworkManager.singleton.StartClient();
    }

    private string GetLocalIP()
    {
        try
        {
            string hostName = System.Net.Dns.GetHostName();
            System.Net.IPAddress[] addresses = System.Net.Dns.GetHostAddresses(hostName);
            foreach (System.Net.IPAddress addr in addresses)
            {
                if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    return addr.ToString();
            }
        }
        catch { }
        return "Unknown";
    }
}
