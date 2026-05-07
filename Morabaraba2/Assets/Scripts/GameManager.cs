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

    // Fix Colour issue
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
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Debug.Log("RemovalHandler found: " + (removalHandler != null));

        millDetector = GetComponent<MillDetector>();
        if (millDetector == null)
            millDetector = gameObject.AddComponent<MillDetector>();

        removalHandler = GetComponent<CowRemovalClickHandler>();
        if (removalHandler == null)
            removalHandler = gameObject.AddComponent<CowRemovalClickHandler>();

        if (winPanel != null)
            winPanel.SetActive(false);

        if (removalIndicatorText != null)
            removalIndicatorText.gameObject.SetActive(false);

        UpdatePhaseUI();

        if (rematchButton != null)
            rematchButton.onClick.AddListener(OnRematch);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuit);
    }

    void OnDisable()
    {
        Debug.Log("Gamemanager disabled\n" + System.Environment.StackTrace);
    }

    // ── Testability ────────────────────────────────────────────────────────
    public void SetPlayerOneCows(int value)
    {
        playerOneTotalCows = value;
    }

    public void SetPlayerTwoCows(int value)
    {
        playerTwoTotalCows = value;
    }

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
            {
                originalColours[cowObject] = sr.color;
            }
        }

        if (!PhotonNetwork.IsConnected)
        {
            ApplyPlacementLogic(player, nodeID);
            return;
        }

        int pvID = cowObject.GetComponent<PhotonView>().ViewID;
        photonView.RPC("RPC_Placement", RpcTarget.Others, player, nodeID,pvID);
        ApplyPlacementLogic(player,nodeID); //run locally immediately 
    }

    [PunRPC]
    private void RPC_Placement(int player, int nodeID,int photonViewID)
    {
        PhotonView pv = PhotonView.Find(photonViewID);
        if (pv != null)
        {
            cowGameObjects[nodeID] = pv.gameObject;
            SpriteRenderer sr = pv.gameObject.GetComponent<SpriteRenderer>();
            if (sr != null && !originalColours.ContainsKey(pv.gameObject))
            {
                originalColours[pv.gameObject] = sr.color;
            }
        }
        ApplyPlacementLogic(player, nodeID);
    }

    private void ApplyPlacementLogic(int player, int nodeID)
    {
        if (millDetector == null)
        {
            millDetector = GetComponent<MillDetector>();

            if (millDetector == null)
                millDetector = gameObject.AddComponent<MillDetector>();
        }

        occupiedNodes[nodeID] = player;

        if (player == 1)
            playerOnePlacedCows++;
        else
            playerTwoPlacedCows++;

        SoundManager.PlayValidMove();

        List<MillDetector.Mill> formedMills =
            millDetector.CheckMillOnPlacement(nodeID, player, occupiedNodes);

        if (formedMills.Count > 0 && !isRemovalPhase)
        {
            isRemovalPhase = true;
            playerWhoFormedMill = player;
            currentMills = formedMills;

            int opponent = (player == 1) ? 2 : 1;

            currentRemovableCows =
                millDetector.GetRemovableOpponentCows(opponent, occupiedNodes);

            StartRemovalPhase(player);
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

        if (!PhotonNetwork.IsConnected)
        {
            ApplyMovementLogic(player, fromNodeID, toNodeID);
            return;
        }

        photonView.RPC("RPC_Movement", RpcTarget.All, player, fromNodeID, toNodeID);
    }

    [PunRPC]
    private void RPC_Movement(int player, int fromNodeID, int toNodeID)
    {
        // Move cow visually on remote client
        BoardNode toNode = GetNodeByID(toNodeID);
        BoardNode fromNode = GetNodeByID(fromNodeID);

        if (fromNode != null)
            fromNode.isOccupied = false;

        if (toNode != null)
            toNode.isOccupied = true;

        // Move actual cow GameObject
        if (cowGameObjects.ContainsKey(fromNodeID))
        {
            GameObject cowGO = cowGameObjects[fromNodeID];

            if (cowGO != null && toNode != null)
            {
                cowGO.transform.position = toNode.transform.position;
            }

            cowGameObjects.Remove(fromNodeID);
            cowGameObjects[toNodeID] = cowGO;
        }

        // IMPORTANT:
        // occupiedNodes is NOT updated here.
        // ApplyMovementLogic already handles it.

        ApplyMovementLogic(player, fromNodeID, toNodeID);
    }

    private void ApplyMovementLogic(int player, int fromNodeID, int toNodeID)
    {
        if (millDetector == null)
        {
            millDetector = GetComponent<MillDetector>();

            if (millDetector == null)
                millDetector = gameObject.AddComponent<MillDetector>();
        }

        // Update occupied nodes
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

            int opponent = (player == 1) ? 2 : 1;

            currentRemovableCows =
                millDetector.GetRemovableOpponentCows(opponent, occupiedNodes);

            StartRemovalPhase(player);
        }
        else if (!isRemovalPhase)
        {
            currentPlayer = (currentPlayer == 1) ? 2 : 1;

            UpdatePhaseUI();

            // Check if next player has no valid moves
            CheckWinCondition();
        }
    }

    private bool PlayerHasValidMoves(int player)
    {
        // Flying players can move anywhere
        if (IsPlayerFlying(player))
        {
            for (int i = 0; i < 24; i++)
            {
                if (!occupiedNodes.ContainsKey(i))
                {
                    return true;
                }
            }

            return false;
        }

        // Normal movement adjacency rules
        Dictionary<int, List<int>> adjacency = new Dictionary<int, List<int>>()
    {
        {0,  new List<int>{1,7}},
        {1,  new List<int>{0,2,9}},
        {2,  new List<int>{1,3}},
        {3,  new List<int>{2,4,11}},
        {4,  new List<int>{3,5}},
        {5,  new List<int>{4,6,13}},
        {6,  new List<int>{5,7}},
        {7,  new List<int>{6,0,15}},

        {8,  new List<int>{9,15}},
        {9,  new List<int>{8,10,1,17}},
        {10, new List<int>{9,11}},
        {11, new List<int>{10,12,3,19}},
        {12, new List<int>{11,13}},
        {13, new List<int>{12,14,5,21}},
        {14, new List<int>{13,15}},
        {15, new List<int>{14,8,7,23}},

        {16, new List<int>{17,23}},
        {17, new List<int>{16,18,9}},
        {18, new List<int>{17,19}},
        {19, new List<int>{18,20,11}},
        {20, new List<int>{19,21}},
        {21, new List<int>{20,22,13}},
        {22, new List<int>{21,23}},
        {23, new List<int>{22,16,15}}
    };

        // Check all cows belonging to player
        foreach (var kvp in occupiedNodes)
        {
            int nodeID = kvp.Key;
            int owner = kvp.Value;

            if (owner != player)
                continue;

            foreach (int adjacent in adjacency[nodeID])
            {
                if (!occupiedNodes.ContainsKey(adjacent))
                {
                    return true;
                }
            }
        }

        return false;
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

    private void StartRemovalPhase(int player)
    {
        if (currentRemovableCows.Count == 0)
        {
            Debug.LogWarning("No removable cows - skipping removal.");

            isRemovalPhase = false;

            currentPlayer = (currentPlayer == 1) ? 2 : 1;

            UpdatePhaseUI();
            return;
        }

        HighlightRemovableCows(currentRemovableCows, true);
        UpdateRemovalUI(true);

        if (MyPlayerNumber == player && removalHandler != null)
        {
            Debug.Log("trying to activate removal node whoop ");
            removalHandler.ActivateRemovalMode(currentRemovableCows);
        }
    }

    public void RemoveCow(int nodeID)
    {
        if (gameOver) return;
        if (!isRemovalPhase) return;

        if (!currentRemovableCows.Contains(nodeID))
        {
            Debug.Log("That cow is protected.");
            SoundManager.PlayInvalidMove();
            return;
        }

        if (!PhotonNetwork.IsConnected)
        {
            ApplyRemovalLogic(nodeID);
            return;
        }

        photonView.RPC("RPC_RemoveCow", RpcTarget.All, nodeID);
    }

    [PunRPC]
    private void RPC_RemoveCow(int nodeID)
    {
        ApplyRemovalLogic(nodeID);
    }

    private void ApplyRemovalLogic(int nodeID)
    {
        List<int> previouslyHighlighted = new List<int>(currentRemovableCows);

        int playerToRemove =
            occupiedNodes.ContainsKey(nodeID)
            ? occupiedNodes[nodeID]
            : -1;

        // Destroy cow visually
        if (cowGameObjects.ContainsKey(nodeID))
        {
            Destroy(cowGameObjects[nodeID]);
            cowGameObjects.Remove(nodeID);
        }

        SoundManager.PlayRemoval();

        BoardNode node = GetNodeByID(nodeID);
        if (node != null)
            node.isOccupied = false;

        if (playerToRemove == 1)
            playerOneTotalCows--;
        else if (playerToRemove == 2)
            playerTwoTotalCows--;

        UpdateFlyingPhase();

        occupiedNodes.Remove(nodeID);

        isRemovalPhase = false;

        HighlightRemovableCows(previouslyHighlighted, false);
        UpdateRemovalUI(false);

        if (removalHandler != null)
            removalHandler.DeactivateRemovalMode();

        currentPlayer = (currentPlayer == 1) ? 2 : 1;

        CheckPlacementPhaseComplete();
        UpdatePhaseUI();

        // Check all win conditions
        CheckWinCondition();

        currentRemovableCows.Clear();
    }

    // ══════════════════════════════════════════════════════════════════════
    // FLYING
    // ══════════════════════════════════════════════════════════════════════
    private void UpdateFlyingPhase()
    {
        player1Flying =
            (playerOneTotalCows == 3 && currentPhase == GamePhase.Movement);

        player2Flying =
            (playerTwoTotalCows == 3 && currentPhase == GamePhase.Movement);

        if (player1Flying)
            Debug.Log("Player 1 is now flying!");

        if (player2Flying)
            Debug.Log("Player 2 is now flying!");
    }

    // ══════════════════════════════════════════════════════════════════════
    // WIN
    // ══════════════════════════════════════════════════════════════════════
    public int CheckWinCondition()
    {
        if (gameOver || currentPhase == GamePhase.Placement)
            return 0;

        int winner = 0;

        // Less than 3 cows
        if (playerOneTotalCows < 3)
            winner = 2;
        else if (playerTwoTotalCows < 3)
            winner = 1;

        // No valid moves
        else if (!PlayerHasValidMoves(1))
            winner = 2;
        else if (!PlayerHasValidMoves(2))
            winner = 1;

        if (winner != 0)
        {
            string winnerName =
                (winner == 1)
                ? PlayerPrefs.GetString("P1", "Player 1")
                : PlayerPrefs.GetString("P2", "Player 2");

            ShowWinScreen(winnerName);

            if (PhotonNetwork.IsConnected)
            {
                photonView.RPC("RPC_ShowWinScreen", RpcTarget.Others, winnerName);
            }
        }

        return winner;
    }

    private void ShowWinScreen(string winnerName)
    {
        gameOver = true;

        if (winPanel != null)
        {
            winPanel.SetActive(true);

            if (winText != null)
                winText.text = winnerName + " Wins!";
        }
    }

    [PunRPC]
    private void RPC_ShowWinScreen(string winnerName)
    {
        ShowWinScreen(winnerName);
    }

    // ══════════════════════════════════════════════════════════════════════
    // UI HELPERS
    // ══════════════════════════════════════════════════════════════════════
    private void UpdatePhaseUI()
    {
        if (phaseText == null) return;

        phaseText.text =
            (currentPhase == GamePhase.Placement)
            ? "Phase: Placement"
            : "Phase: Movement";
    }

    private void UpdateRemovalUI(bool active)
    {
        if (removalIndicatorText == null) return;

        removalIndicatorText.gameObject.SetActive(active);

        if (!active) return;

        bool isMyTurn =
            !PhotonNetwork.IsConnected ||
            (playerWhoFormedMill == MyPlayerNumber);

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
                sr.color =
                    nodeIDs.Contains(kvp.Key)
                    ? Color.red
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
        if (originalColours.ContainsKey(obj))
        {
            return originalColours[obj];
        }

        return Color.white;
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
        if (PhotonNetwork.IsConnected)
            PhotonNetwork.LeaveRoom();
        else
            SceneManager.LoadScene("LobbyScene");
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("LobbyScene");
    }

    // ══════════════════════════════════════════════════════════════════════
    // UTILITY
    // ══════════════════════════════════════════════════════════════════════
    private BoardNode GetNodeByID(int nodeID)
    {
        BoardNode[] allNodes = FindObjectsByType<BoardNode>(FindObjectsSortMode.None);

        foreach (BoardNode node in allNodes)
        {
            if (node.nodeID == nodeID)
                return node;
        }

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

    public int GetNodeOwner(int id)
        => occupiedNodes.ContainsKey(id) ? occupiedNodes[id] : -1;

    public List<int> GetRemovableCows()
        => new List<int>(currentRemovableCows);

    public int GetPlayerOnePlacedCows()
        => playerOnePlacedCows;

    public Dictionary<int, int> GetOccupiedNodes()
        => occupiedNodes;

    public bool IsPlayerFlying(int playerNumber)
    {
        if (playerNumber == 1) return player1Flying;
        if (playerNumber == 2) return player2Flying;

        return false;
    }
}
