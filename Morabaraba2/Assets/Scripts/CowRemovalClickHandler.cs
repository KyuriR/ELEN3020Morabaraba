using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles mouse clicks during the removal phase.
/// When active, clicking on an eligible opponent cow removes it.
/// Activated and deactivated by GameManager.
/// </summary>
public class CowRemovalClickHandler : MonoBehaviour
{
    private bool removalModeActive = false;
    private List<int> removableNodes = new List<int>();
    private Camera mainCamera;
    private BoardNode[] allNodes;

    void Start()
    {
        mainCamera = Camera.main;
        allNodes = FindObjectsByType<BoardNode>(FindObjectsSortMode.None);
    }

    public void ActivateRemovalMode(List<int> nodes)
    {
        removalModeActive = true;
        removableNodes = nodes;
        Debug.Log("Removal mode activated. Eligible nodes: " + string.Join(", ", nodes));
    }

    public void DeactivateRemovalMode()
    {
        removalModeActive = false;
        removableNodes.Clear();
    }

    void Update()
    {
        if (!removalModeActive)
        {
            Debug.Log("Removal mode NOT ACTIVE ");
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            worldPosition.z = 0;

            Collider2D hit = Physics2D.OverlapPoint(worldPosition);

            if (hit != null && hit.CompareTag("Cow"))
            {
                Cow cow = hit.GetComponent<Cow>();
                BoardNode node = cow.GetPlacedNode();

                if (node != null && removableNodes.Contains(node.nodeID))
                {
                    GameManager.Instance.RemoveCow(node.nodeID);
                }
                else
                {
                    Debug.Log("That cow cannot be removed.");
                }
            }
            else
            {
                // Fallback: manual proximity check
                CheckAllCowsManually(worldPosition);
            }
        }
    }

    private void CheckAllCowsManually(Vector3 clickPosition)
    {
        Cow[] allCows = FindObjectsByType<Cow>(FindObjectsSortMode.None);
        float closestDistance = 0.5f;
        Cow closestCow = null;

        foreach (Cow cow in allCows)
        {
            float distance = Vector3.Distance(clickPosition, cow.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestCow = cow;
            }
        }

        if (closestCow != null)
        {
            BoardNode node = closestCow.GetPlacedNode();
            if (node != null && removableNodes.Contains(node.nodeID))
                GameManager.Instance.RemoveCow(node.nodeID);
        }
    }
}
