using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Attach to your GameManager GameObject in GameScene.
/// Reads IsAIGame and AIDifficulty from PlayerPrefs on Start.
/// Always plays as Player 2.
/// </summary>
public class AIOpponent : MonoBehaviour
{
    [Header("Seconds before AI makes its move (feels more natural)")]
    public float thinkDelay = 0.8f;

    private bool _isAIGame;
    private int _difficulty;  // 0=Easy 1=Medium 2=Hard
    private bool _thinking;

    // ── Adjacency map ──────────────────────────────────────────────────────
    private static readonly Dictionary<int, List<int>> Adjacency =
        new Dictionary<int, List<int>>
        {
            {0,new List<int>{1,7}},    {1,new List<int>{0,2,9}},
            {2,new List<int>{1,3}},    {3,new List<int>{2,4,11}},
            {4,new List<int>{3,5}},    {5,new List<int>{4,6,13}},
            {6,new List<int>{5,7}},    {7,new List<int>{6,0,15}},
            {8,new List<int>{9,15}},   {9,new List<int>{8,10,1,17}},
            {10,new List<int>{9,11}},  {11,new List<int>{10,12,3,19}},
            {12,new List<int>{11,13}}, {13,new List<int>{12,14,5,21}},
            {14,new List<int>{13,15}}, {15,new List<int>{14,8,7,23}},
            {16,new List<int>{17,23}}, {17,new List<int>{16,18,9}},
            {18,new List<int>{17,19}}, {19,new List<int>{18,20,11}},
            {20,new List<int>{19,21}}, {21,new List<int>{20,22,13}},
            {22,new List<int>{21,23}}, {23,new List<int>{22,16,15}}
        };

    private static readonly List<int[]> Mills = new List<int[]>
    {
        new[]{0,1,2},   new[]{2,3,4},   new[]{4,5,6},   new[]{6,7,0},
        new[]{8,9,10},  new[]{10,11,12},new[]{12,13,14},new[]{14,15,8},
        new[]{16,17,18},new[]{18,19,20},new[]{20,21,22},new[]{22,23,16},
        new[]{1,9,17},  new[]{3,11,19}, new[]{5,13,21}, new[]{7,15,23}
    };

    void Start()
    {
        _isAIGame = PlayerPrefs.GetInt("IsAIGame", 0) == 1;
        _difficulty = PlayerPrefs.GetInt("AIDifficulty", 0);
    }

    void Update()
    {
        if (!_isAIGame || _thinking) return;
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.GetCurrentPlayer() != 2) return;

        if (GameManager.Instance.IsRemovalPhase())
        {
            if (GameManager.Instance.GetPlayerWhoFormedMill() == 2)
                StartCoroutine(DoRemoval());
            return;
        }

        if (GameManager.Instance.IsPlacementPhase()) { StartCoroutine(DoPlacement()); return; }
        if (GameManager.Instance.IsMovementPhase()) { StartCoroutine(DoMovement()); return; }
    }

    // ══════════════════════════════════════════════════════════════════════
    // PLACEMENT
    // ══════════════════════════════════════════════════════════════════════
    private IEnumerator DoPlacement()
    {
        _thinking = true;
        yield return new WaitForSeconds(thinkDelay);

        var state = GameManager.Instance.GetOccupiedNodes();
        var empty = GetEmptyNodes(state);
        if (empty.Count == 0) { _thinking = false; yield break; }

        int chosen = _difficulty switch
        {
            0 => Easy_Placement(empty),
            1 => Medium_Placement(empty, state),
            _ => Medium_Placement(empty, state)
        };

        PlaceCowAt(chosen);
        _thinking = false;
    }

    private int Easy_Placement(List<int> empty)
        => empty[Random.Range(0, empty.Count)];

    private int Medium_Placement(List<int> empty, Dictionary<int, int> state)
    {
        int win = FindMillCompletion(2, empty, state);
        if (win >= 0) return win;

        int block = FindMillCompletion(1, empty, state);
        if (block >= 0) return block;

        return BestStrategicNode(empty);
    }

    // ══════════════════════════════════════════════════════════════════════
    // MOVEMENT
    // ══════════════════════════════════════════════════════════════════════
    private IEnumerator DoMovement()
    {
        _thinking = true;
        yield return new WaitForSeconds(thinkDelay);

        var state = GameManager.Instance.GetOccupiedNodes();
        bool flying = GameManager.Instance.IsPlayerFlying(2);

        (int from, int to) move = _difficulty switch
        {
            0 => Easy_Move(state, flying),
            1 => Medium_Move(state, flying),
            _ => Hard_Move(state, flying)
        };

        if (move.from >= 0 && move.to >= 0)
            MoveCowFromTo(move.from, move.to);

        _thinking = false;
    }

    private (int, int) Easy_Move(Dictionary<int, int> state, bool flying)
    {
        var moves = GetAllMoves(2, state, flying);
        if (moves.Count == 0) return (-1, -1);
        var m = moves[Random.Range(0, moves.Count)];
        return (m.from, m.to);
    }

    private (int, int) Medium_Move(Dictionary<int, int> state, bool flying)
    {
        var moves = GetAllMoves(2, state, flying);
        if (moves.Count == 0) return (-1, -1);

        foreach (var m in moves)
        {
            var sim = Simulate(state, 2, m.from, m.to);
            if (CountMills(2, m.to, sim) > 0) return (m.from, m.to);
        }

        foreach (var m in moves)
        {
            var sim = Simulate(state, 2, m.from, m.to);
            if (FindMillCompletion(1, GetEmptyNodes(sim), sim) < 0) return (m.from, m.to);
        }

        var pick = moves[Random.Range(0, moves.Count)];
        return (pick.from, pick.to);
    }

    private (int, int) Hard_Move(Dictionary<int, int> state, bool flying)
    {
        var moves = GetAllMoves(2, state, flying);
        if (moves.Count == 0) return (-1, -1);

        int bestScore = int.MinValue;
        (int from, int to) best = (moves[0].from, moves[0].to);

        foreach (var m in moves)
        {
            var sim = Simulate(state, 2, m.from, m.to);
            int score = Minimax(sim, depth: 3, maximising: false,
                                alpha: int.MinValue, beta: int.MaxValue,
                                aiFlying: flying,
                                humanFlying: GameManager.Instance.IsPlayerFlying(1));
            if (score > bestScore) { bestScore = score; best = (m.from, m.to); }
        }

        return best;
    }

    // ══════════════════════════════════════════════════════════════════════
    // REMOVAL
    // ══════════════════════════════════════════════════════════════════════
    private IEnumerator DoRemoval()
    {
        _thinking = true;
        yield return new WaitForSeconds(thinkDelay);

        var removable = GameManager.Instance.GetRemovableCows();
        if (removable.Count == 0) { _thinking = false; yield break; }

        int chosen = _difficulty == 0
            ? removable[Random.Range(0, removable.Count)]
            : removable.OrderByDescending(n => Mills.Count(m => m.Contains(n))).First();

        GameManager.Instance.RemoveCow(chosen);
        _thinking = false;
    }

    // ══════════════════════════════════════════════════════════════════════
    // MINIMAX
    // ══════════════════════════════════════════════════════════════════════
    private int Minimax(Dictionary<int, int> state, int depth, bool maximising,
                        int alpha, int beta, bool aiFlying, bool humanFlying)
    {
        int score = Evaluate(state);
        if (depth == 0 || Mathf.Abs(score) >= 10000) return score;

        if (maximising)
        {
            int best = int.MinValue;
            foreach (var m in GetAllMoves(2, state, aiFlying))
            {
                int val = Minimax(Simulate(state, 2, m.from, m.to), depth - 1, false,
                                  alpha, beta, aiFlying, humanFlying);
                best = Mathf.Max(best, val);
                alpha = Mathf.Max(alpha, best);
                if (beta <= alpha) break;
            }
            return best;
        }
        else
        {
            int best = int.MaxValue;
            foreach (var m in GetAllMoves(1, state, humanFlying))
            {
                int val = Minimax(Simulate(state, 1, m.from, m.to), depth - 1, true,
                                  alpha, beta, aiFlying, humanFlying);
                best = Mathf.Min(best, val);
                beta = Mathf.Min(beta, best);
                if (beta <= alpha) break;
            }
            return best;
        }
    }

    private int Evaluate(Dictionary<int, int> state)
    {
        int ai = state.Values.Count(v => v == 2);
        int human = state.Values.Count(v => v == 1);

        if (ai < 3) return -10000;
        if (human < 3) return 10000;

        int score = (ai - human) * 10;

        foreach (var mill in Mills)
        {
            int aC = mill.Count(n => state.ContainsKey(n) && state[n] == 2);
            int hC = mill.Count(n => state.ContainsKey(n) && state[n] == 1);

            if (aC == 3) score += 100;
            if (hC == 3) score -= 100;
            if (aC == 2 && hC == 0) score += 10;
            if (hC == 2 && aC == 0) score -= 10;
        }

        return score;
    }

    // ══════════════════════════════════════════════════════════════════════
    // HELPERS
    // ══════════════════════════════════════════════════════════════════════
    private List<int> GetEmptyNodes(Dictionary<int, int> state)
    {
        var list = new List<int>();
        for (int i = 0; i < 24; i++)
            if (!state.ContainsKey(i)) list.Add(i);
        return list;
    }

    private int FindMillCompletion(int player, List<int> empty, Dictionary<int, int> state)
    {
        foreach (int node in empty)
            foreach (var mill in Mills)
                if (mill.Contains(node))
                {
                    int owned = mill.Count(n => n != node && state.ContainsKey(n) && state[n] == player);
                    if (owned == 2) return node;
                }
        return -1;
    }

    private int BestStrategicNode(List<int> empty)
    {
        int best = empty[0], bestCount = -1;
        foreach (int n in empty)
        {
            int count = Mills.Count(m => m.Contains(n));
            if (count > bestCount) { bestCount = count; best = n; }
        }
        return best;
    }

    private struct Move { public int from, to; }

    private List<Move> GetAllMoves(int player, Dictionary<int, int> state, bool flying)
    {
        var moves = new List<Move>();
        foreach (var kvp in state)
        {
            if (kvp.Value != player) continue;
            if (flying)
            {
                for (int to = 0; to < 24; to++)
                    if (!state.ContainsKey(to))
                        moves.Add(new Move { from = kvp.Key, to = to });
            }
            else
            {
                foreach (int to in Adjacency[kvp.Key])
                    if (!state.ContainsKey(to))
                        moves.Add(new Move { from = kvp.Key, to = to });
            }
        }
        return moves;
    }

    private Dictionary<int, int> Simulate(Dictionary<int, int> state, int player, int from, int to)
    {
        var sim = new Dictionary<int, int>(state);
        if (from >= 0) sim.Remove(from);
        sim[to] = player;
        return sim;
    }

    private int CountMills(int player, int placedNode, Dictionary<int, int> state)
        => Mills.Count(m => m.Contains(placedNode) &&
                            m.All(n => state.ContainsKey(n) && state[n] == player));

    // ── Execute moves on actual GameObjects ────────────────────────────────

    private void PlaceCowAt(int nodeID)
    {
        BoardNode targetNode = GetNodeByID(nodeID);
        if (targetNode == null) return;

        foreach (Cow cow in FindObjectsByType<Cow>(FindObjectsSortMode.None))
        {
            if (cow.playerNumber != 2) continue;
            if (cow.GetPlacedNode() != null) continue;

            cow.ForcePlace(targetNode);
            GameManager.Instance.RegisterPlacement(2, nodeID, cow.gameObject);
            return;
        }

        Debug.LogWarning("[AI] No unplaced P2 cow found.");
    }

    private void MoveCowFromTo(int fromID, int toID)
    {
        BoardNode fromNode = GetNodeByID(fromID);
        BoardNode toNode = GetNodeByID(toID);
        if (fromNode == null || toNode == null) return;

        foreach (Cow cow in FindObjectsByType<Cow>(FindObjectsSortMode.None))
        {
            if (cow.playerNumber != 2) continue;
            if (cow.GetPlacedNode() != fromNode) continue;

            cow.ForceMove(fromNode, toNode);
            GameManager.Instance.RegisterMovement(2, fromID, toID, cow.gameObject);
            return;
        }

        Debug.LogWarning($"[AI] No P2 cow found on node {fromID}.");
    }

    private BoardNode GetNodeByID(int id)
    {
        foreach (BoardNode n in FindObjectsByType<BoardNode>(FindObjectsSortMode.None))
            if (n.nodeID == id) return n;
        return null;
    }
}