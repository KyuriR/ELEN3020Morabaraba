using Photon.Pun;
using UnityEngine;
using TMPro;

public class TurnUIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI turnText;
    [SerializeField] private TextMeshProUGUI cowCountText;

    private string player1Name;
    private string player2Name;

    private void Start()
    {
        player1Name = PlayerPrefs.GetString("P1", "Player 1");
        player2Name = PlayerPrefs.GetString("P2", "Player 2");

        if (PhotonNetwork.IsConnected && PhotonNetwork.PlayerList.Length >= 1)
        {
            player1Name = PhotonNetwork.PlayerList[0].NickName;
            if (PhotonNetwork.PlayerList.Length >= 2)
                player2Name = PhotonNetwork.PlayerList[1].NickName;
        }
    }

    void Update()
    {
        if (GameManager.Instance == null) return;
        UpdateTurnText();
        UpdateCowCountText();
    }

    private void UpdateTurnText()
    {
        int current = GameManager.Instance.GetCurrentPlayer();
        string name = current == 1 ? player1Name : player2Name;

        if (GameManager.Instance.IsRemovalPhase())
        {
            int mill = GameManager.Instance.GetPlayerWhoFormedMill();
            string millName = mill == 1 ? player1Name : player2Name;
            turnText.text = millName + " formed a mill!\nRemove an opponent's cow.";
        }
        else
        {
            turnText.text = name + "'s Turn";
        }
        // Colour is set in the Inspector on the TMP component — never changed here
    }

    private void UpdateCowCountText()
    {
        if (cowCountText == null) return;

        int p1 = GameManager.Instance.GetPlayerOneTotalCows();
        int p2 = GameManager.Instance.GetPlayerTwoTotalCows();

        cowCountText.text = player1Name + ": " + p1 + " cows\n"
                          + player2Name + ": " + p2 + " cows";
    }
}