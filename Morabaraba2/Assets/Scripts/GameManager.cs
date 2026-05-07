using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    // ── Game State ─────────────────────────────────────────────────────────
    public int currentPlayer = 1;

    private Dictionary<int, int> occupiedNodes = new Dictionary<int, int>();
    private Dictionary<int, GameObject> cowGameObjects = new Dictionary<int, GameObject>();
    private Dictionary<GameObject, Color> originalColours = new Dictionary<GameObject, Color>();

    private List<int> currentRemovableCows = new List<int>();

    private MillDetector millDetector;
    private CowRemovalClickHandler removalHandler;

    private bool isRemovalPhase = false;
    private int playerWhoFormedMill = 0;
    private List<MillDetector.Mill> currentMills = new List<MillDetector.Mill>();

    private int playerOneTotalCows = 12;
    private int playerTwoTotalCows = 12;
    private bool player1Flying = false;
    private bool player2Flying = false;
    private int playerOnePlacedCows = 0;
    private int playerTwoPlacedCows = 0;
    private bool gameOver = false;

    public enum GamePhase { Placement, Movement }
    private GamePhase currentPhase = GamePhase.Placement;

    // ── Prefabs for remote spawning ────────────────────────────────────────
    [Header("Token Prefabs for Remote Spawning")]
    [Tooltip("Drag the Player 1 cow prefab that matches the active theme. " +
             "ThemeManager will update this automatically at runtime.")]
    public GameObject player1CowPrefab;

    [Tooltip("Drag the Player 2 cow prefab that matches the active theme.")]
    public GameObject player2CowPrefab;

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
            if (!PhotonNetwork.IsConnected) return currentPlayer;
            return PhotonNetwork.IsMasterClient ? 1 : 2;
        }
    }

    // ── Unity Lifecycle ────────────────────────────────────────────────────
    private void Awake() { Instance = this; }

    private void Start()
    {
        millDetector = GetComponent<MillDetector>()
                    ?? gameObject.AddComponent<MillDetector>();

        removalHandler = GetComponent<CowRemovalClickHandler>()
                      ?? gameObject.AddComponent<CowRemovalClickHandler>();

        if (winPanel != null) winPanel.SetActive(false);
        if (removalIndicatorText != null) removalIndicatorText.gameObject.SetActive(false);

        UpdatePhaseUI();

        if (rematchButton != null) rematchButton.onClick.AddListener(OnRematch);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuit);

        // Keep prefabs in sync with the active theme
        SyncPrefabsWithTheme();
    }

    /// Call this whenever the theme changes so remote spawns use the right sprite.
    public void SyncPrefabsWithTheme()
    {
        if (ThemeManager.Instance == null) return;
        int idx = ThemeManager.Instance.CurrentThemeIndex;
        if (idx < 0) return;

        if (ThemeManager.Instance.player1TokenPrefabs != null &&
            idx < ThemeManager.Instance.player1TokenPrefabs.Length)
            player1CowPrefab = ThemeManager.Instance.player1TokenPrefabs[idx];

        if (ThemeManager.Instance.player2TokenPrefabs != null &&
            idx < ThemeManager.Instance.player2TokenPrefabs.Length)
            player2CowPrefab = ThemeManager.Instance.player2TokenPrefabs[idx];
    }

    override public void OnDisable()
    {
        Debug.Log("GameManager disabled\n" + System.Environment.StackTrace);
    }

    // ── Testability ────────────────────────────────────────────────────────
    public void SetPlayerOneCows(int v) { playerOneTotalCows = v; }
    public void SetPlayerTwoCows(int v) { playerTwoTotalCows = v; }

    // ══════════════════════════════════════════════════════════════════════
    // PLACEMENT
    // ══════════════════════════════════════════════════════════════════════
    public void RegisterPlacement(int player, int nodeID, GameObject cowObject)
    {
        if (gameOver) return;

        if (cowObject != null)
        {
            cowGameObjects[nodeID] = cowObject;
            SpriteRenderer sr = cowObject.GetComponent<SpriteRenderer>();
            if (sr != null && !originalColours.ContainsKey(cowObject))
                originalColours[cowObject] = sr.color;
        }

        if (!PhotonNetwork.IsConnected)
        {
            ApplyPlacementLogic(player, nodeID);
            return;
        }

        // Send node position so remote client knows where to spawn the visual
        BoardNode node = GetNodeByID(nodeID);
        Vector3 worldPos = node != null ? node.transform.position : Vector3.zero;

        photonView.RPC("RPC_Placement", RpcTarget.Others, player, nodeID,
                       worldPos.x, worldPos.y, worldPos.z);

        ApplyPlacementLogic(player, nodeID);
    }

    [PunRPC]
    private void RPC_Placement(int player, int nodeID, float wx, float wy, float wz)
    {
        // Spawn the visual on the remote client
        SpawnRemoteCow(player, nodeID, new Vector3(wx, wy, wz));
        ApplyPlacementLogic(player, nodeID);
    }

    // ══════════════════════════════════════════════════════════════════════
    // MOVEMENT
    // ══════════════════════════════════════════════════════════════════════
    public void RegisterMovement(int player, int fromNodeID, int toNodeID, GameObject cowObject)
    {
        if (gameOver) return;

        // Keep cowGameObjects in sync so removal can find the cow after it moves
        if (cowObject != null)
        {
            if (fromNodeID >= 0 && cowGameObjects.ContainsKey(fromNodeID))
                cowGameObjects.Remove(fromNodeID);
            cowGameObjects[toNodeID] = cowObject;
        }

        if (!PhotonNetwork.IsConnected)
        {
            ApplyMovementLogic(player, fromNodeID, toNodeID);
            return;
        }

        BoardNode toNode = GetNodeByID(toNodeID);
        Vector3 worldPos = toNode != null ? toNode.transform.position : Vector3.zero;

        photonView.RPC("RPC_Movement", RpcTarget.Others, player, fromNodeID, toNodeID,
                       worldPos.x, worldPos.y, worldPos.z);

        ApplyMovementLogic(player, fromNodeID, toNodeID);
    }

    [PunRPC]
    private void RPC_Movement(int player, int fromNodeID, int toNodeID,
                               float wx, float wy, float wz)
    {
        Vector3 toPos = new Vector3(wx, wy, wz);

        // Move the existing cow GO on this client
        if (cowGameObjects.ContainsKey(fromNodeID))
        {
            GameObject cowGO = cowGameObjects[fromNodeID];
            if (cowGO != null) cowGO.transform.position = toPos;
            cowGameObjects.Remove(fromNodeID);
            cowGameObjects[toNodeID] = cowGO;
        }
        else
        {
            // Fallback: no GO found locally for this cow — spawn one
            SpawnRemoteCow(player, toNodeID, toPos);
        }

        BoardNode fromNode = GetNodeByID(fromNodeID);
        BoardNode toNode = GetNodeByID(toNodeID);
        if (fromNode != null) fromNode.isOccupied = false;
        if (toNode != null) toNode.isOccupied = true;

        ApplyMovementLogic(player, fromNodeID, toNodeID);
    }

    // ── Spawn a cow visually on the remote client ──────────────────────────
    private void SpawnRemoteCow(int player, int nodeID, Vector3 worldPos)
    {
        GameObject prefab = player == 1 ? player1CowPrefab : player2CowPrefab;

        if (prefab == null)
        {
            Debug.LogWarning($"[GameManager] No prefab set for player {player}. " +
                              "Assign player1CowPrefab / player2CowPrefab in the Inspector, " +
                              "or call SyncPrefabsWithTheme() after theme loads.");
            return;
        }

        GameObject cow = Instantiate(prefab, worldPos, Quaternion.identity);

        // Disable the Cow drag script on the remote client — this machine doesn't own it
        Cow cowScript = cow.GetComponent<Cow>();
        if (cowScript != null) cowScript.enabled = false;

        // Disable colliders so it doesn't interfere with local input
        Collider2D col = cow.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        cowGameObjects[nodeID] = cow;

        SpriteRenderer sr = cow.GetComponent<SpriteRenderer>();
        if (sr != null && !originalColours.ContainsKey(cow))
            originalColours[cow] = sr.color;

        BoardNode node = GetNodeByID(nodeID);
        if (node != null) node.isOccupied = true;
    }

    // ══════════════════════════════════════════════════════════════════════
    // PLACEMENT LOGIC (unchanged)
    // ══════════════════════════════════════════════════════════════════════
    private void ApplyPlacementLogic(int player, int nodeID)
    {
        if (millDetector == null)
            millDetector = GetComponent<MillDetector>() ?? gameObject.AddComponent<MillDetector>();

        occupiedNodes[nodeID] = player;

        if (player == 1) playerOnePlacedCows++;
        else playerTwoPlacedCows++;

        SoundManager.PlayValidMove();

        List<MillDetector.Mill> formedMills =
            millDetector.CheckMillOnPlacement(nodeID, player, occupiedNodes);

        if (formedMills.Count > 0 && !isRemovalPhase)
        {
            isRemovalPhase = true;
            playerWhoFormedMill = player;
            currentMills = formedMills;
            int opponent = player == 1 ? 2 : 1;
            currentRemovableCows = millDetector.GetRemovableOpponentCows(opponent, occupiedNodes);
            StartRemovalPhase(player);
        }
        else if (!isRemovalPhase)
        {
            CheckPlacementPhaseComplete();
            currentPlayer = currentPlayer == 1 ? 2 : 1;
            UpdatePhaseUI();
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // MOVEMENT LOGIC (unchanged)
    // ══════════════════════════════════════════════════════════════════════
    private void ApplyMovementLogic(int player, int fromNodeID, int toNodeID)
    {
        if (millDetector == null)
            millDetector = GetComponent<MillDetector>() ?? gameObject.AddComponent<MillDetector>();

        if (occupiedNodes.ContainsKey(fromNodeID))
            occupiedNodes.Remove(fromNodeID);
        occupiedNodes[toNodeID] = player;

        SoundManager.PlayValidMove();

        List<MillDetector.Mill> formedMills =
            millDetector.CheckMillOnPlacement(toNodeID, player, occupiedNodes);

        if (formedMills.Count > 0 && !isRemovalPhase)
        {
            isRemovalPhase = true;
            playerWhoFormedMill = player;
            currentMills = formedMills;
            int opponent = player == 1 ? 2 : 1;
            currentRemovableCows = millDetector.GetRemovableOpponentCows(opponent, occupiedNodes);
            StartRemovalPhase(player);
        }
        else if (!isRemovalPhase)
        {
            currentPlayer = currentPlayer == 1 ? 2 : 1;
            UpdatePhaseUI();
            CheckWinCondition();
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // REMOVAL PHASE (unchanged except HighlightRemovableCows colour fix)
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

    private void StartRemovalPhase(int player)
    {
        if (currentRemovableCows.Count == 0)
        {
            Debug.LogWarning("No removable cows — skipping removal.");
            isRemovalPhase = false;
            currentPlayer = currentPlayer == 1 ? 2 : 1;
            UpdatePhaseUI();
            return;
        }

        HighlightRemovableCows(currentRemovableCows, true);
        UpdateRemovalUI(true);

        if (MyPlayerNumber == player && removalHandler != null)
            removalHandler.ActivateRemovalMode(currentRemovableCows);
    }

    public void RemoveCow(int nodeID)
    {
        if (gameOver || !isRemovalPhase) return;

        if (!currentRemovableCows.Contains(nodeID))
        {
            SoundManager.PlayInvalidMove();
            return;
        }

        if (!PhotonNetwork.IsConnected) { ApplyRemovalLogic(nodeID); return; }

        photonView.RPC("RPC_RemoveCow", RpcTarget.All, nodeID);
    }

    [PunRPC]
    private void RPC_RemoveCow(int nodeID) { ApplyRemovalLogic(nodeID); }

    private void ApplyRemovalLogic(int nodeID)
    {
        List<int> previouslyHighlighted = new List<int>(currentRemovableCows);

        int playerToRemove = occupiedNodes.ContainsKey(nodeID) ? occupiedNodes[nodeID] : -1;

        // Primary: dictionary lookup
        if (cowGameObjects.ContainsKey(nodeID))
        {
            Destroy(cowGameObjects[nodeID]);
            cowGameObjects.Remove(nodeID);
        }
        else
        {
            // Fallback: cow was moved here but cowGameObjects wasn't updated.
            // Try placedNode match first, then proximity.
            BoardNode targetNode = GetNodeByID(nodeID);
            bool destroyed = false;

            foreach (Cow cow in FindObjectsByType<Cow>(FindObjectsSortMode.None))
            {
                if (cow.GetPlacedNode() != null && cow.GetPlacedNode().nodeID == nodeID)
                {
                    cowGameObjects.Remove(nodeID); // clean up if stale
                    Destroy(cow.gameObject);
                    destroyed = true;
                    break;
                }
            }

            if (!destroyed && targetNode != null)
            {
                foreach (Cow cow in FindObjectsByType<Cow>(FindObjectsSortMode.None))
                {
                    if (Vector3.Distance(cow.transform.position, targetNode.transform.position) < 0.3f)
                    {
                        Destroy(cow.gameObject);
                        break;
                    }
                }
            }
        }

        SoundManager.PlayRemoval();

        BoardNode node = GetNodeByID(nodeID);
        if (node != null) node.isOccupied = false;

        if (playerToRemove == 1) playerOneTotalCows--;
        else if (playerToRemove == 2) playerTwoTotalCows--;

        UpdateFlyingPhase();
        occupiedNodes.Remove(nodeID);
        isRemovalPhase = false;

        HighlightRemovableCows(previouslyHighlighted, false);
        UpdateRemovalUI(false);

        if (removalHandler != null) removalHandler.DeactivateRemovalMode();

        currentPlayer = currentPlayer == 1 ? 2 : 1;
        CheckPlacementPhaseComplete();
        UpdatePhaseUI();
        CheckWinCondition();
        currentRemovableCows.Clear();
    }

    // ══════════════════════════════════════════════════════════════════════
    // FLYING
    // ══════════════════════════════════════════════════════════════════════
    private void UpdateFlyingPhase()
    {
        player1Flying = playerOneTotalCows == 3 && currentPhase == GamePhase.Movement;
        player2Flying = playerTwoTotalCows == 3 && currentPhase == GamePhase.Movement;
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
        else if (!PlayerHasValidMoves(1)) winner = 2;
        else if (!PlayerHasValidMoves(2)) winner = 1;

        if (winner != 0)
        {
            string winnerName = winner == 1
                ? PlayerPrefs.GetString("P1", "Player 1")
                : PlayerPrefs.GetString("P2", "Player 2");

            ShowWinScreen(winnerName);

            if (PhotonNetwork.IsConnected)
                photonView.RPC("RPC_ShowWinScreen", RpcTarget.Others, winnerName);
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

    [PunRPC]
    private void RPC_ShowWinScreen(string winnerName) { ShowWinScreen(winnerName); }

    private bool PlayerHasValidMoves(int player)
    {
        if (IsPlayerFlying(player))
        {
            for (int i = 0; i < 24; i++)
                if (!occupiedNodes.ContainsKey(i)) return true;
            return false;
        }

        Dictionary<int, List<int>> adjacency = new Dictionary<int, List<int>>()
        {
            {0,new List<int>{1,7}},   {1,new List<int>{0,2,9}},
            {2,new List<int>{1,3}},   {3,new List<int>{2,4,11}},
            {4,new List<int>{3,5}},   {5,new List<int>{4,6,13}},
            {6,new List<int>{5,7}},   {7,new List<int>{6,0,15}},
            {8,new List<int>{9,15}},  {9,new List<int>{8,10,1,17}},
            {10,new List<int>{9,11}}, {11,new List<int>{10,12,3,19}},
            {12,new List<int>{11,13}},{13,new List<int>{12,14,5,21}},
            {14,new List<int>{13,15}},{15,new List<int>{14,8,7,23}},
            {16,new List<int>{17,23}},{17,new List<int>{16,18,9}},
            {18,new List<int>{17,19}},{19,new List<int>{18,20,11}},
            {20,new List<int>{19,21}},{21,new List<int>{20,22,13}},
            {22,new List<int>{21,23}},{23,new List<int>{22,16,15}}
        };

        foreach (var kvp in occupiedNodes)
        {
            if (kvp.Value != player) continue;
            foreach (int adj in adjacency[kvp.Key])
                if (!occupiedNodes.ContainsKey(adj)) return true;
        }
        return false;
    }

    // ══════════════════════════════════════════════════════════════════════
    // UI HELPERS
    // ══════════════════════════════════════════════════════════════════════
    private void UpdatePhaseUI()
    {
        if (phaseText == null) return;
        phaseText.text = currentPhase == GamePhase.Placement ? "Phase: Placement" : "Phase: Movement";
    }

    private void UpdateRemovalUI(bool active)
    {
        if (removalIndicatorText == null) return;
        removalIndicatorText.gameObject.SetActive(active);
        if (!active) return;
        bool isMyTurn = !PhotonNetwork.IsConnected || playerWhoFormedMill == MyPlayerNumber;
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

            if (highlight)
            {
                sr.color = nodeIDs.Contains(kvp.Key)
                    ? (ThemeManager.Instance != null
                        ? ThemeManager.Instance.MillFlashColor()
                        : Color.red)
                    : GetOriginalColour(kvp.Value);
            }
            else
            {
                sr.color = GetOriginalColour(kvp.Value);
            }
        }
    }

    private Color GetOriginalColour(GameObject obj)
    {
        return originalColours.ContainsKey(obj) ? originalColours[obj] : Color.white;
    }

    // ══════════════════════════════════════════════════════════════════════
    // REMATCH / QUIT
    // ══════════════════════════════════════════════════════════════════════
    private void OnRematch()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel("GameScene");
        else if (!PhotonNetwork.IsConnected)
            SceneManager.LoadScene("GameScene");
    }

    private void OnQuit()
    {
        if (PhotonNetwork.IsConnected) PhotonNetwork.LeaveRoom();
        else SceneManager.LoadScene("LobbyScene");
    }

    public override void OnLeftRoom() { SceneManager.LoadScene("LobbyScene"); }

    // ══════════════════════════════════════════════════════════════════════
    // UTILITY
    // ══════════════════════════════════════════════════════════════════════
    private BoardNode GetNodeByID(int nodeID)
    {
        foreach (BoardNode node in FindObjectsByType<BoardNode>(FindObjectsSortMode.None))
            if (node.nodeID == nodeID) return node;
        return null;
    }

    public void EndTurn()
    {
        if (isRemovalPhase) return;
        currentPlayer = currentPlayer == 1 ? 2 : 1;
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
    public bool IsPlayerFlying(int p) => p == 1 ? player1Flying : p == 2 && player2Flying;
    public int GetPlayerOneTotalCows() => playerOneTotalCows;
    public int GetPlayerTwoTotalCows() => playerTwoTotalCows;
}