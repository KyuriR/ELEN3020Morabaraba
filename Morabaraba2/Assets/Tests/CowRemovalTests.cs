using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class CowRemovalTests
{
    // A Test behaves as an ordinary method
    [Test]
    public void CowRemovalTestsSimplePasses()
    {
        // Use the Assert class to test conditions
    }
    
    

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator RemoveCow_DeletesCowGameObject()
    
    {
        //Arrange 
        GameObject gameObj = new GameObject();
        GameManager gm = gameObj.AddComponent<GameManager>();
        gameObj.AddComponent<MillDetector>();

        CowRemovalClickHandler cowRemoval = gameObj.AddComponent<CowRemovalClickHandler>();

        GameManager.Instance = gm;
        //gm.SendMessage("Start");
        //Create removable cow
        GameObject cowObj = new GameObject("Cow");
        cowObj.tag = "Cow";
        cowObj.AddComponent<SpriteRenderer>();
        Cow cow = cowObj.AddComponent<Cow>();
        cow.playerNumber = 2;
        
        //Create node
        GameObject nodeObj = new GameObject();
        BoardNode node = nodeObj.AddComponent<BoardNode>();
        node.nodeID = 1;
        node.isOccupied = true;
        
        //Register placement 
        gm.RegisterPlacement(2,1,cowObj);
        
        //Force Removal Phase instead of activating GameManager
        typeof(GameManager).GetField("isRemovalPhase",System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance).SetValue(gm, true);
        
        typeof(GameManager).GetField("currentRemovableCows",System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance).SetValue(gm, new List<int> {1});
        
        //Act 
        gm.RemoveCow(1);
        
        //Wait one frame for Destroy() method to activate
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
        
        Assert.IsFalse(gm.IsNodeOccupied(1));
        //If node is no longer occupied, cow removal successful
        Assert.IsTrue(cowObj==null);
    }
}
