using NUnit.Framework;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class NetWorkUITests
{
    private GameObject networkUIObject;
    private NetworkUI networkUI;

    private TMP_InputField playerNameInput;
    private TMP_InputField roomCodeInput;

    private Button quickJoinButton;
    private Button hostButton;
    private Button joinButton;

    private TextMeshProUGUI statusText;

    [SetUp]
    public void Setup()
    {
        networkUIObject = new GameObject();

        networkUI = networkUIObject.AddComponent<NetworkUI>();

        // Create UI components
        playerNameInput = new GameObject().AddComponent<TMP_InputField>();
        roomCodeInput = new GameObject().AddComponent<TMP_InputField>();

        quickJoinButton = new GameObject().AddComponent<Button>();
        hostButton = new GameObject().AddComponent<Button>();
        joinButton = new GameObject().AddComponent<Button>();

        statusText = new GameObject().AddComponent<TextMeshProUGUI>();

        // Assign using reflection because fields are private
        typeof(NetworkUI)
            .GetField("playerNameInput",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
            .SetValue(networkUI, playerNameInput);

        typeof(NetworkUI)
            .GetField("roomCodeInput",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
            .SetValue(networkUI, roomCodeInput);

        typeof(NetworkUI)
            .GetField("quickJoinButton",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
            .SetValue(networkUI, quickJoinButton);

        typeof(NetworkUI)
            .GetField("hostButton",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
            .SetValue(networkUI, hostButton);

        typeof(NetworkUI)
            .GetField("joinButton",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
            .SetValue(networkUI, joinButton);

        typeof(NetworkUI)
            .GetField("statusText",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
            .SetValue(networkUI, statusText);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(networkUIObject);
    }

    // =========================================================
    // PLAYER NAME VALIDATION
    // =========================================================

    [Test]
    public void EmptyPlayerName_ShouldFailValidation()
    {
        playerNameInput.text = "";

        bool result =
            (bool)typeof(NetworkUI)
            .GetMethod("SetName",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
            .Invoke(networkUI, null);

        Assert.IsFalse(result);
    }

    [Test]
    public void ValidPlayerName_ShouldPassValidation()
    {
        playerNameInput.text = "Player1";

        bool result =
            (bool)typeof(NetworkUI)
            .GetMethod("SetName",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
            .Invoke(networkUI, null);

        Assert.IsTrue(result);
    }

    // =========================================================
    // ROOM CODE VALIDATION
    // =========================================================

    [UnityTest]
    public IEnumerator EmptyRoomCode_ShowsErrorMessage()
    {
        //Test for room code validation 
        //Arrange 
        roomCodeInput.text = "";

        playerNameInput.text = "Tester";

        //Act 
        typeof(NetworkUI)
            .GetMethod("JoinByCode",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
            .Invoke(networkUI, null);

        yield return null;
        //Assert 
        Assert.AreEqual(
            "Please enter a room code.",
            statusText.text
        );
    }

    // =========================================================
    // STATUS MESSAGE TESTING
    // =========================================================

    [Test]
    public void StatusText_UpdatesCorrectly()
    {
        typeof(NetworkUI)
            .GetMethod("SetStatus",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
            .Invoke(networkUI,
                new object[] { "Connected", true });

        Assert.AreEqual("Connected", statusText.text);
    }

    // =========================================================
    // BUTTON INTERACTABILITY
    // =========================================================

    [Test]
    public void Buttons_DisabledCorrectly()
    {
        typeof(NetworkUI)
            .GetMethod("SetButtonsInteractable",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
            .Invoke(networkUI,
                new object[] { false });

        Assert.IsFalse(quickJoinButton.interactable);
        Assert.IsFalse(hostButton.interactable);
        Assert.IsFalse(joinButton.interactable);
    }

    [Test]
    public void Buttons_EnabledCorrectly()
    {
        typeof(NetworkUI)
            .GetMethod("SetButtonsInteractable",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
            .Invoke(networkUI,
                new object[] { true });

        Assert.IsTrue(quickJoinButton.interactable);
        Assert.IsTrue(hostButton.interactable);
        Assert.IsTrue(joinButton.interactable);
    }
}