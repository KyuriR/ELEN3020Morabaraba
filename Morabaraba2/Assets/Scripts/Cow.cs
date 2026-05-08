using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Cow : MonoBehaviour
{
    private Vector3 offset;
    private float zCoord;
    private Vector3 startPosition;
    private BoardNode hoveredNode;
    private BoardNode placedNode;
    private bool isPlaced = false;
    public int playerNumber;
    private bool isDragging = false;
    private GameObject highlight;
    private SpriteRenderer highlightRenderer;
    private SpriteRenderer pieceRenderer;
    private Vector3 previousPosition;
    private BoardNode previousNode;

    void Start()
    {
        startPosition = transform.position;
        pieceRenderer = GetComponent<SpriteRenderer>();
        Transform foundHighlight = transform.Find("Highlight");
        if (foundHighlight != null)
        {
            highlight = foundHighlight.gameObject;
            highlightRenderer = foundHighlight.GetComponent<SpriteRenderer>();
            highlight.SetActive(false);
        }
    }

    public void RefreshTheme()
    {
        if (highlightRenderer == null || ThemeManager.Instance == null) return;
        highlightRenderer.color = ThemeManager.Instance.PieceHighlight(playerNumber);
    }

    // Called by AIOpponent to place/move without mouse input
    public void ForcePlace(BoardNode node)
    {
        transform.position = node.transform.position;
        node.isOccupied = true;
        placedNode = node;
        isPlaced = true;
    }

    public void ForceMove(BoardNode from, BoardNode to)
    {
        if (from != null) from.isOccupied = false;
        transform.position = to.transform.position;
        to.isOccupied = true;
        placedNode = to;
    }

    void OnMouseDown()
    {
        if (GameManager.Instance.IsRemovalPhase()) return;
        if (PlayerPrefs.GetInt("IsAIGame", 0) == 1 && playerNumber == 2) return;
        if (PhotonNetwork.IsConnected && playerNumber != GameManager.Instance.MyPlayerNumber) return;
        if (GameManager.Instance.GetCurrentPlayer() != playerNumber) { Debug.Log("Not Player " + playerNumber + "'s turn"); return; }

        if (GameManager.Instance.IsPlacementPhase()) { if (isPlaced) return; }
        else { if (!isPlaced) return; previousPosition = transform.position; previousNode = placedNode; }

        isDragging = true;
        zCoord = Camera.main.WorldToScreenPoint(transform.position).z;
        offset = transform.position - GetMouseWorldPosition();

        if (highlight != null)
        {
            highlight.SetActive(true);
            if (highlightRenderer != null && ThemeManager.Instance != null)
                highlightRenderer.color = ThemeManager.Instance.PieceHighlight(playerNumber);
        }
    }

    void OnMouseDrag() { if (!isDragging) return; transform.position = GetMouseWorldPosition() + offset; }

    void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;
        if (GameManager.Instance.IsPlacementPhase()) HandlePlacement();
        else HandleMovementOrFlying();
        if (highlight != null) highlight.SetActive(false);
    }

    private void HandlePlacement()
    {
        if (hoveredNode != null && !hoveredNode.isOccupied)
        {
            transform.position = hoveredNode.transform.position;
            hoveredNode.isOccupied = true;
            placedNode = hoveredNode;
            isPlaced = true;
            SoundEffectsPlayer.PlayPlacement();
            Debug.Log("Cow placed on node: " + placedNode.nodeID);
            GameManager.Instance.RegisterPlacement(playerNumber, placedNode.nodeID, gameObject);
        }
        else
        {
            transform.position = startPosition;
            SoundEffectsPlayer.PlayInvalidMove();
            Debug.Log("Invalid placement.");
        }
    }

    private void HandleMovementOrFlying()
    {
        if (hoveredNode == null || hoveredNode.isOccupied) { ReturnToPreviousNode(); return; }

        bool validMove = GameManager.Instance.IsPlayerFlying(playerNumber)
                      || (previousNode != null && AreNodesAdjacent(previousNode.nodeID, hoveredNode.nodeID));

        if (validMove)
        {
            if (previousNode != null) previousNode.isOccupied = false;
            transform.position = hoveredNode.transform.position;
            hoveredNode.isOccupied = true;
            int oldNodeID = previousNode != null ? previousNode.nodeID : -1;
            placedNode = hoveredNode;
            SoundEffectsPlayer.PlayValidMove();
            GameManager.Instance.RegisterMovement(playerNumber, oldNodeID, placedNode.nodeID, gameObject);
        }
        else { ReturnToPreviousNode(); SoundEffectsPlayer.PlayInvalidMove(); }
    }

    private void ReturnToPreviousNode()
    {
        transform.position = previousPosition;
        if (previousNode != null) { previousNode.isOccupied = true; placedNode = previousNode; }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = zCoord;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    void OnTriggerEnter2D(Collider2D col) { if (col.CompareTag("BoardNode")) hoveredNode = col.GetComponent<BoardNode>(); }
    void OnTriggerExit2D(Collider2D col) { if (col.CompareTag("BoardNode")) { BoardNode node = col.GetComponent<BoardNode>(); if (hoveredNode == node) hoveredNode = null; } }

    public BoardNode GetPlacedNode() => placedNode;

    private bool AreNodesAdjacent(int from, int to)
    {
        Dictionary<int, List<int>> adj = new Dictionary<int, List<int>>()
        {
            {0,new List<int>{1,7}},{1,new List<int>{0,2,9}},{2,new List<int>{1,3}},{3,new List<int>{2,4,11}},
            {4,new List<int>{3,5}},{5,new List<int>{4,6,13}},{6,new List<int>{5,7}},{7,new List<int>{6,0,15}},
            {8,new List<int>{9,15}},{9,new List<int>{8,10,1,17}},{10,new List<int>{9,11}},{11,new List<int>{10,12,3,19}},
            {12,new List<int>{11,13}},{13,new List<int>{12,14,5,21}},{14,new List<int>{13,15}},{15,new List<int>{14,8,7,23}},
            {16,new List<int>{17,23}},{17,new List<int>{16,18,9}},{18,new List<int>{17,19}},{19,new List<int>{18,20,11}},
            {20,new List<int>{19,21}},{21,new List<int>{20,22,13}},{22,new List<int>{21,23}},{23,new List<int>{22,16,15}}
        };
        if (!adj.ContainsKey(from)) return false;
        return adj[from].Contains(to);
    }
}