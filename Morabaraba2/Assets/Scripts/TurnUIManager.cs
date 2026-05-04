using UnityEngine;
using TMPro;

/// <summary>
/// Updates turn and cow count UI every frame.
///
/// INSPECTOR SETUP:
///   - Attach to a UI Canvas GameObject in GameScene
///   - Assign turnText (required) and cowCountText (optional)
/// </summary>
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
        string name = (current == 1) ? player1Name : player2Name;

        if (GameManager.Instance.IsRemovalPhase())
        {
            int mill = GameManager.Instance.GetPlayerWhoFormedMill();
            string millName = (mill == 1) ? player1Name : player2Name;
            turnText.text = millName + " formed a mill!\nRemove an opponent's cow.";
            turnText.color = Color.yellow;
        }
        else
        {
            turnText.text = name + "'s Turn";
            turnText.color = (current == 1) ? Color.red : Color.blue;
        }
    }

    private void UpdateCowCountText()
    {
        if (cowCountText == null) return;
        var nodes = GameManager.Instance.GetOccupiedNodes();
        int p1 = 0, p2 = 0;
        foreach (var kvp in nodes)
        {
            if (kvp.Value == 1) p1++;
            else p2++;
        }
        cowCountText.text = player1Name + ": " + p1 + " cows\n"
                          + player2Name + ": " + p2 + " cows";
    }
}
