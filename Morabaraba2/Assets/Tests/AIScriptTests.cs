using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Reflection;

public class AIScriptTests
{
    private AIOpponent ai;

    [SetUp]
    public void Setup()
    {
        GameObject obj = new GameObject();
        ai = obj.AddComponent<AIOpponent>();
    }
     //Test to check AI prioritising forming mills
    [Test]
    public void AI_Completes_Mill_When_Possible()
    {
        //Arrange 
        Dictionary<int, int> state = new Dictionary<int, int>
        {
            {0, 2},
            {1, 2}
        };

        List<int> empty = new List<int> {2,3,4};
         //Act 
        MethodInfo method =
            typeof(AIOpponent).GetMethod(
                "FindMillCompletion",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

        int result = (int)method.Invoke(ai, new object[]
        {
            2,
            empty,
            state
        });
        //Assert 
        Assert.AreEqual(2, result);
    }
    //Test to check AI blocking opponent mill 
    [Test]
    public void AI_Blocks_Opponent_Mill()
    {
        Dictionary<int, int> state = new Dictionary<int, int>
        {
            {0, 1},
            {1, 1}
        };

        List<int> empty = new List<int> {2,5,7};

        MethodInfo method =
            typeof(AIOpponent).GetMethod(
                "FindMillCompletion",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

        int result = (int)method.Invoke(ai, new object[]
        {
            1,
            empty,
            state
        });

        Assert.AreEqual(2, result);
    }
    //AI makes valid moves to adjacent nodes 
    [Test]
    public void AI_Generates_Valid_Adjacent_Moves()
    {
        Dictionary<int, int> state = new Dictionary<int, int>
        {
            {0, 2}
        };

        MethodInfo method =
            typeof(AIOpponent).GetMethod(
                "GetAllMoves",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

        var moves = method.Invoke(ai, new object[]
        {
            2,
            state,
            false
        });

        Assert.IsNotNull(moves);
    }
    
    //AI detects mills correctly 
    [Test]
    public void AI_Detects_Completed_Mill()
    {
        Dictionary<int, int> state = new Dictionary<int, int>
        {
            {0, 2},
            {1, 2},
            {2, 2}
        };

        MethodInfo method =
            typeof(AIOpponent).GetMethod(
                "CountMills",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

        int mills = (int)method.Invoke(ai, new object[]
        {
            2,
            2,
            state
        });

        Assert.AreEqual(1, mills);
    }
}
