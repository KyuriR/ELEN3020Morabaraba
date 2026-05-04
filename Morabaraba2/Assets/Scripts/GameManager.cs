using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    // ── Game State ─────────────────────────────────────────────────────────
    [SyncVar] public int currentPlayer = 1;

    private Dictionary<int, int> occupiedNodes = new Dictionary<int, int>();
    private Dictionary<int, GameObject> cowGameObjects = new Dictionary<int, GameObject>();
    private List<int> currentRemovableCows = new List<int>();

    private MillDetector millDetector;
    private CowRemovalClickHandler removalHandler;

    [SyncVar] private bool isRemovalPhase = false;
    [SyncVar] private int playerWhoFormedMill = 0;
    private List<MillDetector.Mill> currentMills = new List<MillDetector.Mill>();

    private int playerOneTotalCows = 12;
    private int playerTwoTotalCows = 12;

    private bool player1Flying = false;
    private bool player2Flying = false;

    private int playerOnePlacedCows = 0;
    private int playerTwoPlacedCows = 0;

    private bool gameOver = false;

    public enum GamePhase { Placement, Movement }
    [SyncVar] private GamePhase currentPhase = GamePhase.Placement;

    // ── UI ─────────────────────────────────────────────────────────────────
    [Header("Win Screen UI")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TextMeshProUGUI winText;
    [SerializeField] private Button rematchButton;
    [SerializeField] private Button quitButton;

    [Header("Game UI")]
    [SerializeField] private TextMeshProUGUI phaseText;
    [SerializeField] private TextMeshProUGUI removalIndicatorText;

    // ── Local player number ────────────────────────────────────────────────
    public int MyPlayerNumber
    {
        get
        {
            // Local hot-seat mode — no Mirror running at all
            if (!NetworkClient.active && !NetworkServer.active)
                return currentPlayer; // whoever's turn it is can drag
            // Online mode — host = Player 1, client = Player 2
            return NetworkServer.active ? 1 : 2;
        }
    }

    // ── Unity Lifecycle ────────────────────────────────────────────────────
    private void Awake()
    {
        Instance = this;
    }

    // Start handles LOCAL mode initialisation (Mirror not running)
    private void Start()
    {
        millDetector = GetComponent<MillDetector>();
        if (millDetector == null) millDetector = gameObject.AddComponent<MillDetector>();

        removalHandler = GetComponent<CowRemovalClickHandler>();
        if (removalHandler == null) removalHandler = gameObject.AddComponent<CowRemovalClickHandler>();

        if (winPanel != null) winPanel.SetActive(false);
        if (removalIndicatorText != null) removalIndicatorText.gameObject.SetActive(false);

        UpdatePhaseUI();

        if (rematchButton != null) rematchButton.onClick.AddListener(OnRematch);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuit);
    }

    // OnStartServer handles SERVER initialisation when Mirror IS running
    public override void OnStartServer()
    {
        millDetector = GetComponent<MillDetector>();
        if (millDetector == null) millDetector = gameObject.AddComponent<MillDetector>();

        removalHandler = GetComponent<CowRemovalClickHandler>();
        if (removalHandler == null) removalHandler = gameObject.AddComponent<CowRemovalClickHandler>();
    }

    // OnStartClient handles CLIENT initialisation when Mirror IS running
    public override void OnStartClient()
    {
        if (winPanel != null) winPanel.SetActive(false);
        if (removalIndicatorText != null) removalIndicatorText.gameObject.SetActive(false);

        UpdatePhaseUI();

        if (rematchButton != null) rematchButton.onClick.AddListener(OnRematch);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuit);
    }

    // ── Testability ────────────────────────────────────────────────────────
    public void SetPlayerOneCows(int value) { playerOneTotalCows = value; }
    public void SetPlayerTwoCows(int value) { playerTwoTotalCows = value; }

    // ══════════════════════════════════════════════════════════════════════
    // PLACEMENT
    // ══════════════════════════════════════════════════════════════════════
    public void RegisterPlacement(int player, int nodeID, GameObject cowObject)
    {
        if (gameOver) return;
        if (cowObject != null) cowGameObjects[nodeID] = cowObject;

        // Local mode — run logic directly
        if (!NetworkClient.active && !NetworkServer.active)
        {
            ApplyPlacementLogic(player, nodeID);
            return;
        }
        CmdPlacement(player, nodeID);
    }

    [Command(requiresAuthority = false)]
    private void CmdPlacement(int player, int nodeID)
    {
        ApplyPlacementLogic(player, nodeID);
        RpcPlacement(player, nodeID);
    }

    [ClientRpc]
    private void RpcPlacement(int player, int nodeID)
    {
        occupiedNodes[nodeID] = player;
        if (player == 1) playerOnePlacedCows++;
        else playerTwoPlacedCows++;

        // Find and store the cow GO on this client
        BoardNode node = GetNodeByID(nodeID);
        if (node != null)
        {
            Cow[] allCows = FindObjectsByType<Cow>(FindObjectsSortMode.None);
            foreach (Cow c in allCows)
            {
                if (c.playerNumber == player &&
                    Vector3.Distance(c.transform.position, node.transform.position) < 0.3f)
                {
                    cowGameObjects[nodeID] = c.gameObject;
                    break;
                }
            }
        }
        UpdatePhaseUI();
    }

    private void ApplyPlacementLogic(int player, int nodeID)
    {
        // Safety check — ensure millDetector is initialised
        if (millDetector == null)
        {
            millDetector = GetComponent<MillDetector>();
            if (millDetector == null) millDetector = gameObject.AddComponent<MillDetector>();
        }

        occupiedNodes[nodeID] = player;

        if (player == 1) playerOnePlacedCows++;
        else playerTwoPlacedCows++;

        List<MillDetector.Mill> formedMills =
            millDetector.CheckMillOnPlacement(nodeID, player, occupiedNodes);

        if (formedMills.Count > 0 && !isRemovalPhase)
        {
            isRemovalPhase = true;
            playerWhoFormedMill = player;
            currentMills = formedMills;

            int opponent = (player == 1) ? 2 : 1;
            currentRemovableCows = millDetector.GetRemovableOpponentCows(opponent, occupiedNodes);

            if (!NetworkClient.active && !NetworkServer.active)
                StartRemovalPhaseLocal(player);
            else
                RpcStartRemovalPhase(player, currentRemovableCows.ToArray());
        }
        else if (!isRemovalPhase)
        {
            CheckPlacementPhaseComplete();
            currentPlayer = (currentPlayer == 1) ? 2 : 1;
            UpdatePhaseUI();
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // MOVEMENT
    // ══════════════════════════════════════════════════════════════════════
    public void RegisterMovement(int player, int fromNodeID, int toNodeID, GameObject cowObject)
    {
        if (gameOver) return;

        if (!NetworkClient.active && !NetworkServer.active)
        {
            ApplyMovementLogic(player, fromNodeID, toNodeID);
            return;
        }
        CmdMovement(player, fromNodeID, toNodeID);
    }

    [Command(requiresAuthority = false)]
    private void CmdMovement(int player, int fromNodeID, int toNodeID)
    {
        ApplyMovementLogic(player, fromNodeID, toNodeID);
        RpcMovement(player, fromNodeID, toNodeID);
    }

    [ClientRpc]
    private void RpcMovement(int player, int fromNodeID, int toNodeID)
    {
        BoardNode toNode = GetNodeByID(toNodeID);
        BoardNode fromNode = GetNodeByID(fromNodeID);

        if (fromNode != null) fromNode.isOccupied = false;
        if (toNode != null) toNode.isOccupied = true;

        if (cowGameObjects.ContainsKey(fromNodeID))
        {
            GameObject cowGO = cowGameObjects[fromNodeID];
            if (cowGO != null && toNode != null)
                cowGO.transform.position = toNode.transform.position;
            cowGameObjects.Remove(fromNodeID);
            cowGameObjects[toNodeID] = cowGO;
        }

        if (occupiedNodes.ContainsKey(fromNodeID)) occupiedNodes.Remove(fromNodeID);
        occupiedNodes[toNodeID] = player;
        UpdatePhaseUI();
    }

    private void ApplyMovementLogic(int player, int fromNodeID, int toNodeID)
    {
        if (occupiedNodes.ContainsKey(fromNodeID)) occupiedNodes.Remove(fromNodeID);
        occupiedNodes[toNodeID] = player;

        List<MillDetector.Mill> formedMills =
            millDetector.CheckMillOnPlacement(toNodeID, player, occupiedNodes);

        if (formedMills.Count > 0 && !isRemovalPhase)
        {
            isRemovalPhase = true;
            playerWhoFormedMill = player;
            currentMills = formedMills;

            int opponent = (player == 1) ? 2 : 1;
            currentRemovableCows = millDetector.GetRemovableOpponentCows(opponent, occupiedNodes);

            if (!NetworkClient.active && !NetworkServer.active)
                StartRemovalPhaseLocal(player);
            else
                RpcStartRemovalPhase(player, currentRemovableCows.ToArray());
        }
        else if (!isRemovalPhase)
        {
            currentPlayer = (currentPlayer == 1) ? 2 : 1;
            UpdatePhaseUI();
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // REMOVAL PHASE
    // ══════════════════════════════════════════════════════════════════════
    private void CheckPlacementPhaseComplete()
    {
        if (playerOnePlacedCows >= 12 && playerTwoPlacedCows >= 12)
        {
            currentPhase = GamePhase.Movement;
            UpdatePhaseUI();
            Debug.Log("Movement phase begins!");
        }
    }

    // Local mode removal start
    private void StartRemovalPhaseLocal(int player)
    {
        if (currentRemovableCows.Count == 0)
        {
            isRemovalPhase = false;
            currentPlayer = (currentPlayer == 1) ? 2 : 1;
            UpdatePhaseUI();
            return;
        }

        HighlightRemovableCows(currentRemovableCows, true);
        UpdateRemovalUI(true);

        if (removalHandler != null)
            removalHandler.ActivateRemovalMode(currentRemovableCows);
    }

    [ClientRpc]
    private void RpcStartRemovalPhase(int player, int[] removableNodes)
    {
        currentRemovableCows = new List<int>(removableNodes);
        isRemovalPhase = true;
        playerWhoFormedMill = player;

        HighlightRemovableCows(currentRemovableCows, true);
        UpdateRemovalUI(true);

        if (MyPlayerNumber == player && removalHandler != null)
            removalHandler.ActivateRemovalMode(currentRemovableCows);
    }

    public void RemoveCow(int nodeID)
    {
        if (gameOver) return;
        if (!isRemovalPhase) return;
        if (!currentRemovableCows.Contains(nodeID)) { Debug.Log("That cow is protected."); return; }

        if (!NetworkClient.active && !NetworkServer.active)
        {
            ApplyRemovalLogic(nodeID);
            return;
        }
        CmdRemoveCow(nodeID);
    }

    [Command(requiresAuthority = false)]
    private void CmdRemoveCow(int nodeID)
    {
        if (!occupiedNodes.ContainsKey(nodeID)) return;
        int owner = occupiedNodes[nodeID];
        if (owner == playerWhoFormedMill) return;
        ApplyRemovalLogic(nodeID);
        RpcRemoveCow(nodeID);
    }

    [ClientRpc]
    private void RpcRemoveCow(int nodeID)
    {
        ApplyRemovalVisuals(nodeID);
    }

    private void ApplyRemovalLogic(int nodeID)
    {
        int playerToRemove = occupiedNodes.ContainsKey(nodeID) ? occupiedNodes[nodeID] : -1;
        if (playerToRemove == 1) playerOneTotalCows--;
        else if (playerToRemove == 2) playerTwoTotalCows--;

        UpdateFlyingPhase();
        occupiedNodes.Remove(nodeID);

        isRemovalPhase = false;
        currentRemovableCows.Clear();
        currentPlayer = (currentPlayer == 1) ? 2 : 1;

        CheckPlacementPhaseComplete();

        // In local mode also update visuals directly
        if (!NetworkClient.active && !NetworkServer.active)
            ApplyRemovalVisuals(nodeID);

        // Check win
        if (currentPhase == GamePhase.Movement)
        {
            int winner = 0;
            if (playerOneTotalCows < 3) winner = 2;
            else if (playerTwoTotalCows < 3) winner = 1;
            if (winner != 0)
            {
                string winnerName = (winner == 1)
                    ? PlayerPrefs.GetString("P1", "Player 1")
                    : PlayerPrefs.GetString("P2", "Player 2");

                if (!NetworkClient.active && !NetworkServer.active)
                    ShowWinScreen(winnerName);
                else
                    RpcShowWinScreen(winnerName);
            }
        }
    }

    private void ApplyRemovalVisuals(int nodeID)
    {
        if (cowGameObjects.ContainsKey(nodeID))
        {
            Destroy(cowGameObjects[nodeID]);
            cowGameObjects.Remove(nodeID);
        }

        BoardNode node = GetNodeByID(nodeID);
        if (node != null) node.isOccupied = false;

        HighlightRemovableCows(new List<int>(), false);
        UpdateRemovalUI(false);
        UpdatePhaseUI();

        if (removalHandler != null) removalHandler.DeactivateRemovalMode();
    }

    // ══════════════════════════════════════════════════════════════════════
    // FLYING
    // ══════════════════════════════════════════════════════════════════════
    private void UpdateFlyingPhase()
    {
        player1Flying = (playerOneTotalCows == 3 && currentPhase == GamePhase.Movement);
        player2Flying = (playerTwoTotalCows == 3 && currentPhase == GamePhase.Movement);
    }

    // ══════════════════════════════════════════════════════════════════════
    // WIN
    // ══════════════════════════════════════════════════════════════════════
    public int CheckWinCondition()
    {
        if (gameOver || currentPhase == GamePhase.Placement) return 0;
        int winner = 0;
        if (playerOneTotalCows < 3) winner = 2;
        else if (playerTwoTotalCows < 3) winner = 1;
        if (winner != 0)
        {
            string winnerName = (winner == 1)
                ? PlayerPrefs.GetString("P1", "Player 1")
                : PlayerPrefs.GetString("P2", "Player 2");
            ShowWinScreen(winnerName);
        }
        return winner;
    }

    private void ShowWinScreen(string winnerName)
    {
        gameOver = true;
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            if (winText != null) winText.text = winnerName + " Wins!";
        }
    }

    [ClientRpc]
    private void RpcShowWinScreen(string winnerName)
    {
        ShowWinScreen(winnerName);
    }

    // ══════════════════════════════════════════════════════════════════════
    // UI HELPERS
    // ══════════════════════════════════════════════════════════════════════
    private void UpdatePhaseUI()
    {
        if (phaseText == null) return;
        phaseText.text = (currentPhase == GamePhase.Placement)
            ? "Phase: Placement"
            : "Phase: Movement";
    }

    private void UpdateRemovalUI(bool active)
    {
        if (removalIndicatorText == null) return;
        removalIndicatorText.gameObject.SetActive(active);
        if (!active) return;

        bool isMyTurn = (playerWhoFormedMill == MyPlayerNumber)
                     || (!NetworkClient.active && !NetworkServer.active);
        removalIndicatorText.text = isMyTurn
            ? "Click an opponent's cow to remove it!"
            : "Opponent is removing one of your cows...";
    }

    private void HighlightRemovableCows(List<int> nodeIDs, bool highlight)
    {
        foreach (var kvp in cowGameObjects)
        {
            if (kvp.Value == null) continue;
            SpriteRenderer sr = kvp.Value.GetComponent<SpriteRenderer>();
            if (sr == null) continue;
            sr.color = (highlight && nodeIDs.Contains(kvp.Key)) ? Color.red : Color.white;
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // REMATCH / QUIT
    // ══════════════════════════════════════════════════════════════════════
    private void OnRematch()
    {
        if (NetworkServer.active)
            NetworkManager.singleton.ServerChangeScene("GameScene");
        else
            SceneManager.LoadScene("GameScene");
    }

    private void OnQuit()
    {
        if (NetworkServer.active || NetworkClient.active)
            NetworkManager.singleton.StopHost();
        SceneManager.LoadScene("LobbyScene");
    }

    // ══════════════════════════════════════════════════════════════════════
    // UTILITY
    // ══════════════════════════════════════════════════════════════════════
    private BoardNode GetNodeByID(int nodeID)
    {
        BoardNode[] allNodes = FindObjectsByType<BoardNode>(FindObjectsSortMode.None);
        foreach (BoardNode node in allNodes)
            if (node.nodeID == nodeID) return node;
        return null;
    }

    public void EndTurn()
    {
        if (isRemovalPhase) return;
        currentPlayer = (currentPlayer == 1) ? 2 : 1;
        UpdatePhaseUI();
    }

    public bool IsRemovalPhase() => isRemovalPhase;
    public bool IsPlacementPhase() => currentPhase == GamePhase.Placement;
    public bool IsMovementPhase() => currentPhase == GamePhase.Movement;
    public int GetCurrentPlayer() => currentPlayer;
    public int GetPlayerWhoFormedMill() => playerWhoFormedMill;
    public bool IsNodeOccupied(int id) => occupiedNodes.ContainsKey(id);
    public int GetNodeOwner(int id) => occupiedNodes.ContainsKey(id) ? occupiedNodes[id] : -1;
    public List<int> GetRemovableCows() => new List<int>(currentRemovableCows);
    public int GetPlayerOnePlacedCows() => playerOnePlacedCows;
    public Dictionary<int, int> GetOccupiedNodes() => occupiedNodes;

    public bool IsPlayerFlying(int playerNumber)
    {
        if (playerNumber == 1) return player1Flying;
        if (playerNumber == 2) return player2Flying;
        return false;
    }
}