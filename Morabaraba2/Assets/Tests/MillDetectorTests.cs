using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class MillDetectorTests
{
    private GameObject gameObject;
    private MillDetector millDetector;

    [SetUp]
    public void Setup()
    {
        gameObject = new GameObject();
        millDetector = gameObject.AddComponent<MillDetector>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(gameObject);
    }
    
    //These Setup and TearDown methods are used for Tests and do not affect actual Game/Prototype

   
    [Test]
    public void CheckMillOnPlacement_FormsOuterMill_ReturnsMill()
    {
        //Test to detect a valid mill
        //Arrange 
        Dictionary<int, int> occupiedNodes = new Dictionary<int, int>()
        {
            {0, 1},
            {1, 1},
            {2, 1}
        };
        
        //Act 
        List<MillDetector.Mill> mills =
            millDetector.CheckMillOnPlacement(2, 1, occupiedNodes);
        //Assert 
        Assert.AreEqual(1, mills.Count);
    }

    [Test]
    public void CheckMillOnPlacement_IncompleteMill_ReturnsNoMill()
    {
        //Test to check that system will not detect incomplete mill
        //Arrange 
        Dictionary<int, int> occupiedNodes = new Dictionary<int, int>()
        {
            {0, 1},
            {1, 1}
        };
        //Act 
        List<MillDetector.Mill> mills =
            millDetector.CheckMillOnPlacement(1, 1, occupiedNodes);
        //Assert 
        Assert.AreEqual(0, mills.Count);
    }

    [Test]
    public void HasAnyMills_ValidMill_ReturnsTrue()
    {
        //Arrange 
        Dictionary<int, int> occupiedNodes = new Dictionary<int, int>()
        {
            {0, 1},
            {1, 1},
            {2, 1}
        }; 
        
        //Act
        bool result = millDetector.HasAnyMills(1, occupiedNodes);
        //Assert 
        Assert.IsTrue(result);
    }

    [Test]
    public void HasAnyMills_NoMill_ReturnsFalse()
    {
        //Arrange 
        Dictionary<int, int> occupiedNodes = new Dictionary<int, int>()
        {
            {0, 1},
            {1, 1}
        };
        //Act
        bool result = millDetector.HasAnyMills(1, occupiedNodes);
         //Assert 
        Assert.IsFalse(result);
    }



    [Test]
    public void GetRemovableOpponentCows_ReturnsOnlyNonMillCows()
    {
        //Test to return removable cows correctly 
        //Arrange 
        Dictionary<int, int> occupiedNodes = new Dictionary<int, int>()
        {
            // Player 2 mill
            {0, 2},
            {1, 2},
            {2, 2},

            // Extra removable cow
            {5, 2}
        };
        //Act
        List<int> removable =
            millDetector.GetRemovableOpponentCows(2, occupiedNodes);
        //Assert 
        Assert.AreEqual(1, removable.Count);
        Assert.Contains(5, removable);
    }

    [Test]
    public void GetRemovableOpponentCows_AllCowsInMill_ReturnsAll()
    {
        Dictionary<int, int> occupiedNodes = new Dictionary<int, int>()
        {
            {0, 2},
            {1, 2},
            {2, 2}
        };

        List<int> removable =
            millDetector.GetRemovableOpponentCows(2, occupiedNodes);

        Assert.AreEqual(3, removable.Count);

        Assert.Contains(0, removable);
        Assert.Contains(1, removable);
        Assert.Contains(2, removable);
    }

 

    [Test]
    public void GetNodesInMills_ReturnsCorrectNodes()
    {
        Dictionary<int, int> occupiedNodes = new Dictionary<int, int>()
        {
            {0, 1},
            {1, 1},
            {2, 1}
        };

        HashSet<int> nodes =
            millDetector.GetNodesInMills(1, occupiedNodes);

        Assert.AreEqual(3, nodes.Count);

        Assert.IsTrue(nodes.Contains(0));
        Assert.IsTrue(nodes.Contains(1));
        Assert.IsTrue(nodes.Contains(2));
    }

  
    [Test]
    public void GetAllPlayerMills_MultipleMills_ReturnsBoth()
    {
        Dictionary<int, int> occupiedNodes = new Dictionary<int, int>()
        {
            // Mill 1
            {0, 1},
            {1, 1},
            {2, 1},

            // Mill 2
            {8, 1},
            {9, 1},
            {10, 1}
        };

        List<MillDetector.Mill> mills =
            millDetector.GetAllPlayerMills(1, occupiedNodes);

        Assert.AreEqual(2, mills.Count);
    }
}
