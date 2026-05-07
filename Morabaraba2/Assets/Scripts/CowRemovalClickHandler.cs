using System.Collections.Generic;
using UnityEngine;

public class CowRemovalClickHandler : MonoBehaviour
{
    private bool removalModeActive = false;
    private List<int> removableNodes = new List<int>();
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    public void ActivateRemovalMode(List<int> nodes)
    {
        removalModeActive = true;
        removableNodes = new List<int>(nodes);
        Debug.Log("[Removal] Activated. Eligible nodes: " + string.Join(", ", nodes));

        // Log each removable node's world position so we can verify they match the board
        foreach (int id in nodes)
        {
            BoardNode n = GetNodeByID(id);
            Debug.Log($"[Removal] Node {id} world pos: {(n != null ? n.transform.position.ToString() : "NOT FOUND")}");
        }
    }

    public void DeactivateRemovalMode()
    {
        removalModeActive = false;
        removableNodes.Clear();
    }

    void Update()
    {
        if (!removalModeActive) return;
        if (!Input.GetMouseButtonDown(0)) return;

        Vector3 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0;

        Debug.Log($"[Removal] Click at {worldPos}");

        // Log distance to every removable node so we can see which one wins
        int bestNode = -1;
        float bestDist = 0.6f;   // threshold in world units

        foreach (int nodeID in removableNodes)
        {
            BoardNode node = GetNodeByID(nodeID);
            if (node == null) continue;

            float dist = Vector3.Distance(worldPos, node.transform.position);
            Debug.Log($"[Removal] Distance to node {nodeID} at {node.transform.position}: {dist:F3}");

            if (dist < bestDist)
            {
                bestDist = dist;
                bestNode = nodeID;
            }
        }

        if (bestNode >= 0)
        {
            Debug.Log($"[Removal] Removing cow on node {bestNode} (dist {bestDist:F3})");
            GameManager.Instance.RemoveCow(bestNode);
        }
        else
        {
            Debug.Log("[Removal] No removable node within threshold of click.");
        }
    }

    private BoardNode GetNodeByID(int id)
    {
        foreach (BoardNode n in FindObjectsByType<BoardNode>(FindObjectsSortMode.None))
            if (n.nodeID == id) return n;
        return null;
    }
}