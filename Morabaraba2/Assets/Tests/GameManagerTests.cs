using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class GameManagerTests
{
    private GameManager gm;
    private GameObject obj;

    [SetUp]
    public void SetUp()
    {
        obj = new GameObject();

        gm = obj.AddComponent<GameManager>();

        obj.AddComponent<MillDetector>();
        obj.AddComponent<CowRemovalClickHandler>();

        GameManager.Instance = gm;

        gm.SendMessage("Start");
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(obj);
    }

    [Test]
    public void PlayerOneLoses_ReturnPlayerTwoAsWinner()
    {
        // Arrange
        gm.SetPlayerOneCows(2);
        gm.SetPlayerTwoCows(5);

        // Force movement phase
        typeof(GameManager)
            .GetField("currentPhase",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
            .SetValue(gm, GameManager.GamePhase.Movement);

        // Act
        int winner = gm.CheckWinCondition();

        // Assert
        Assert.AreEqual(2, winner);
    }

    [Test]
    public void PlayerTwoLoses_ReturnPlayerOneAsWinner()
    {
        // Arrange
        gm.SetPlayerOneCows(5);
        gm.SetPlayerTwoCows(2);

        // Force movement phase
        typeof(GameManager)
            .GetField("currentPhase",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
            .SetValue(gm, GameManager.GamePhase.Movement);

        // Act
        int winner = gm.CheckWinCondition();

        // Assert
        Assert.AreEqual(1, winner);
    }

    [Test]
    public void PlayerOnesTurn_SwitchToPlayerTwo()
    {
        // Arrange
        gm.currentPlayer = 1;

        // Act
        gm.EndTurn();

        // Assert
        Assert.AreEqual(2, gm.currentPlayer);
    }

    [Test]
    public void PlayerTwosTurn_SwitchToPlayerOne()
    {
        // Arrange
        gm.currentPlayer = 2;

        // Act
        gm.EndTurn();

        // Assert
        Assert.AreEqual(1, gm.currentPlayer);
    }

    [UnityTest]
    public IEnumerator GameManagerTestsWithEnumeratorPasses()
    {
        yield return null;
    }

    [Test]
    public void RegisterPlacement_PlayerOneCountIncreases()
    {
        // Arrange
        GameObject cow = new GameObject();

        // Act
        gm.RegisterPlacement(1, 0, cow);

        // Assert
        Assert.AreEqual(1, gm.GetPlayerOnePlacedCows());
    }

    [Test]
    public void RemoveCow_ValidNode_RemovesCow()
    {
        // Arrange
        GameObject cow1 = new GameObject();
        GameObject cow2 = new GameObject();

        gm.RegisterPlacement(1, 0, cow1);
        gm.RegisterPlacement(2, 1, cow2);

        // Force removal phase manually
        typeof(GameManager)
            .GetField("isRemovalPhase",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
            .SetValue(gm, true);

        typeof(GameManager)
            .GetField("currentRemovableCows",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
            .SetValue(gm, new List<int> { 1 });

        typeof(GameManager)
            .GetField("playerWhoFormedMill",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
            .SetValue(gm, 1);

        // Act
        gm.RemoveCow(1);

        // Assert
        Assert.IsFalse(gm.GetOccupiedNodes().ContainsKey(1));
    }
    
    [Test]
    public void AllCowsPlaced_SwitchesToMovementPhase()
    {
        // Arrange
        GameObject cow;

        // Simulate all placements-->Act 0
        for (int i = 0; i < 12; i++)
        {
            cow = new GameObject();
            gm.RegisterPlacement(1, i, cow);
        }

        for (int i = 12; i < 24; i++)
        {
            cow = new GameObject();
            gm.RegisterPlacement(2, i, cow);
        }

        // Assert
        Assert.IsTrue(gm.IsMovementPhase());
    }
    
    [Test]
    public void PlayerWithThreeCows_EntersFlyingPhase()
    {
        // Arrange

        // Force movement phase
        typeof(GameManager)
            .GetField("currentPhase",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
            .SetValue(gm, GameManager.GamePhase.Movement);

        gm.SetPlayerOneCows(3);

        // Act
        typeof(GameManager)
            .GetMethod("UpdateFlyingPhase",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
            .Invoke(gm, null);

        // Assert
        Assert.IsTrue(gm.IsPlayerFlying(1));
    }
}