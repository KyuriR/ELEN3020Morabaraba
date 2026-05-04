using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains all 18 possible mills and logic to detect them.
/// Attach to the GameManager GameObject.
/// </summary>
public class MillDetector : MonoBehaviour
{
    [System.Serializable]
    public class Mill
    {
        public int[] nodeIDs;
        public Mill(int a, int b, int c) { nodeIDs = new int[] { a, b, c }; }
    }

    private List<Mill> allMills = new List<Mill>();

    private void Awake()
    {
        InitializeMills();
    }

    private void InitializeMills()
    {
        // Outer square
        allMills.Add(new Mill(0, 1, 2));
        allMills.Add(new Mill(2, 3, 4));
        allMills.Add(new Mill(4, 5, 6));
        allMills.Add(new Mill(6, 7, 0));
        // Middle square
        allMills.Add(new Mill(8, 9, 10));
        allMills.Add(new Mill(10, 11, 12));
        allMills.Add(new Mill(12, 13, 14));
        allMills.Add(new Mill(14, 15, 8));
        // Inner square
        allMills.Add(new Mill(16, 17, 18));
        allMills.Add(new Mill(18, 19, 20));
        allMills.Add(new Mill(20, 21, 22));
        allMills.Add(new Mill(22, 23, 16));
        // Cross lines
        allMills.Add(new Mill(1, 9, 17));
        allMills.Add(new Mill(3, 11, 19));
        allMills.Add(new Mill(5, 13, 21));
        allMills.Add(new Mill(7, 15, 23));
        allMills.Add(new Mill(0, 8, 16));
        allMills.Add(new Mill(2, 10, 18));
        allMills.Add(new Mill(4, 12, 20));
        allMills.Add(new Mill(6, 14, 22));
    }

    public List<Mill> CheckMillOnPlacement(int placedNodeID, int playerNumber,
        Dictionary<int, int> occupiedNodes)
    {
        List<Mill> formedMills = new List<Mill>();

        foreach (Mill mill in allMills)
        {
            if (ContainsNode(mill, placedNodeID) && IsMillFormed(mill, playerNumber, occupiedNodes))
            {
                if (!IsMillInList(formedMills, mill))
                    formedMills.Add(mill);
            }
        }
        return formedMills;
    }

    public HashSet<int> GetNodesInMills(int playerNumber, Dictionary<int, int> occupiedNodes)
    {
        HashSet<int> nodesInMills = new HashSet<int>();
        foreach (Mill mill in allMills)
        {
            if (IsMillFormed(mill, playerNumber, occupiedNodes))
                foreach (int id in mill.nodeIDs)
                    nodesInMills.Add(id);
        }
        return nodesInMills;
    }

    public List<int> GetRemovableOpponentCows(int opponentPlayerNumber,
        Dictionary<int, int> occupiedNodes)
    {
        List<int> opponentNodes = new List<int>();
        foreach (var kvp in occupiedNodes)
            if (kvp.Value == opponentPlayerNumber)
                opponentNodes.Add(kvp.Key);

        if (opponentNodes.Count == 0) return new List<int>();

        HashSet<int> inMills = GetNodesInMills(opponentPlayerNumber, occupiedNodes);
        List<int> removable = new List<int>();

        foreach (int id in opponentNodes)
            if (!inMills.Contains(id))
                removable.Add(id);

        // If all opponent cows are in mills, any can be removed
        if (removable.Count == 0)
            removable.AddRange(opponentNodes);

        return removable;
    }

    private bool IsMillFormed(Mill mill, int playerNumber, Dictionary<int, int> occupiedNodes)
    {
        foreach (int id in mill.nodeIDs)
            if (!occupiedNodes.ContainsKey(id) || occupiedNodes[id] != playerNumber)
                return false;
        return true;
    }

    private bool ContainsNode(Mill mill, int nodeID)
    {
        foreach (int id in mill.nodeIDs)
            if (id == nodeID) return true;
        return false;
    }

    private bool IsMillInList(List<Mill> mills, Mill target)
    {
        foreach (Mill m in mills)
            if (AreMillsEqual(m, target)) return true;
        return false;
    }

    private bool AreMillsEqual(Mill m1, Mill m2)
    {
        List<int> n1 = new List<int>(m1.nodeIDs);
        List<int> n2 = new List<int>(m2.nodeIDs);
        n1.Sort(); n2.Sort();
        for (int i = 0; i < n1.Count; i++)
            if (n1[i] != n2[i]) return false;
        return true;
    }

    public List<Mill> GetAllPlayerMills(int playerNumber, Dictionary<int, int> occupiedNodes)
    {
        List<Mill> playerMills = new List<Mill>();
        foreach (Mill mill in allMills)
            if (IsMillFormed(mill, playerNumber, occupiedNodes) && !IsMillInList(playerMills, mill))
                playerMills.Add(mill);
        return playerMills;
    }

    public bool HasAnyMills(int playerNumber, Dictionary<int, int> occupiedNodes)
    {
        foreach (Mill mill in allMills)
            if (IsMillFormed(mill, playerNumber, occupiedNodes)) return true;
        return false;
    }
}
