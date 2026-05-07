using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

public class CowTests
{
    private GameObject gameManagerObject;
    private GameManager gameManager;

    private GameObject cowObject;
    private Cow cow;

    private GameObject nodeObject;
    private BoardNode mockNode;

    [SetUp]
    public void SetUp()
    {
        // ======================
        // GameManager setup
        // ======================
        gameManagerObject = new GameObject("GameManager");

        gameManager = gameManagerObject.AddComponent<GameManager>();

        gameManagerObject.AddComponent<MillDetector>();
        gameManagerObject.AddComponent<CowRemovalClickHandler>();

        GameManager.Instance = gameManager;

        gameManager.SendMessage("Start");

        // ======================
        // Cow setup
        // ======================
        cowObject = new GameObject("Cow");

        cow = cowObject.AddComponent<Cow>();

        cow.playerNumber = 1;

        cowObject.AddComponent<SpriteRenderer>();

        // ======================
        // BoardNode setup
        // ======================
        nodeObject = new GameObject("BoardNode");

        mockNode = nodeObject.AddComponent<BoardNode>();

        mockNode.nodeID = 1;
        mockNode.isOccupied = false;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(cowObject);
        Object.DestroyImmediate(gameManagerObject);
        Object.DestroyImmediate(nodeObject);
    }

    // =========================================================
    // PLACEMENT TESTS
    // =========================================================

    [UnityTest]
    public IEnumerator Cow_Placement_RegistersCorrectly()
    {
        // Arrange
        gameManager.currentPlayer = 1;

        GameObject placedCow = new GameObject();

        // Act
        gameManager.RegisterPlacement(1, mockNode.nodeID, placedCow);

        yield return null;

        // Assert
        Assert.IsTrue(gameManager.IsNodeOccupied(mockNode.nodeID));
    }

    [UnityTest]
    public IEnumerator Cow_Placement_AssignsCorrectOwner()
    {
        // Arrange
        gameManager.currentPlayer = 1;

        GameObject placedCow = new GameObject();

        // Act
        gameManager.RegisterPlacement(1, mockNode.nodeID, placedCow);

        yield return null;

        // Assert
        Assert.AreEqual(1, gameManager.GetNodeOwner(mockNode.nodeID));
    }

    // =========================================================
    // MOVEMENT TESTS
    // =========================================================

    [UnityTest]
    public IEnumerator Cow_Movement_ValidMove_UpdatesBoardState()
    {
        // Arrange
        int fromNode = 1;
        int toNode = 2;

        SetupNode(fromNode, 1);
        SetupNode(toNode, 0);

        gameManager.currentPlayer = 1;

        GameObject movingCow = new GameObject();

        // Act
        gameManager.RegisterMovement(1, fromNode, toNode, movingCow);

        yield return null;

        // Assert
        Assert.IsFalse(gameManager.GetOccupiedNodes().ContainsKey(fromNode));

        Assert.IsTrue(gameManager.IsNodeOccupied(toNode));

        Assert.AreEqual(1, gameManager.GetNodeOwner(toNode));
    }

    [UnityTest]
    public IEnumerator Cow_Movement_ReplacesOldNodeOwnership()
    {
        // Arrange
        int fromNode = 1;
        int toNode = 2;

        SetupNode(fromNode, 1);

        gameManager.currentPlayer = 1;

        GameObject movingCow = new GameObject();

        // Act
        gameManager.RegisterMovement(1, fromNode, toNode, movingCow);

        yield return null;

        // Assert
        Assert.AreEqual(-1, gameManager.GetNodeOwner(fromNode));

        Assert.AreEqual(1, gameManager.GetNodeOwner(toNode));
    }

    // =========================================================
    // TURN SYSTEM TESTS
    // =========================================================

    [Test]
    public void EndTurn_PlayerOne_BecomesPlayerTwo()
    {
        // Arrange
        gameManager.currentPlayer = 1;

        // Act
        gameManager.EndTurn();

        // Assert
        Assert.AreEqual(2, gameManager.GetCurrentPlayer());
    }

    [Test]
    public void EndTurn_PlayerTwo_BecomesPlayerOne()
    {
        // Arrange
        gameManager.currentPlayer = 2;

        // Act
        gameManager.EndTurn();

        // Assert
        Assert.AreEqual(1, gameManager.GetCurrentPlayer());
    }

    // =========================================================
    // HELPER METHODS
    // =========================================================

    private void SetupNode(int nodeID, int owner)
    {
        if (owner == 0)
        {
            gameManager.GetOccupiedNodes().Remove(nodeID);
        }
        else
        {
            gameManager.GetOccupiedNodes()[nodeID] = owner;
        }
    }
}