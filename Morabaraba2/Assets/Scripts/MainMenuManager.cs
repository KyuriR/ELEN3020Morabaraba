using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Manages the Main Menu scene with Login, Register, and Forgot Password panels.
/// Uses PlayerPrefs to store accounts locally (no backend needed).
/// When Firebase is added later, replace the SaveAccount/ValidateLogin methods.
///
/// SCENE HIERARCHY SETUP:
///
/// Canvas
///   BackgroundImage          (Image component, assign your background sprite)
///   LogoText                 (TextMeshProUGUI - "MORABARABA")
///   SubtitleText             (TextMeshProUGUI - "A Traditional African Strategy Game")
///
///   LoginPanel
///     Title                  (TextMeshProUGUI - "Welcome Back")
///     EmailInput             (TMP_InputField)
///     PasswordInput          (TMP_InputField - content type: Password)
///     LoginButton            (Button - "Login")
///     ForgotPasswordButton   (Button - "Forgot Password?")
///     NoAccountText          (TextMeshProUGUI - "Don't have an account?")
///     RegisterLinkButton     (Button - "Register")
///     StatusText             (TextMeshProUGUI - error/status messages)
///
///   RegisterPanel
///     Title                  (TextMeshProUGUI - "Create Account")
///     UsernameInput          (TMP_InputField - display name)
///     EmailInput             (TMP_InputField)
///     PasswordInput          (TMP_InputField - content type: Password)
///     ConfirmPasswordInput   (TMP_InputField - content type: Password)
///     RegisterButton         (Button - "Create Account")
///     BackButton             (Button - "Back to Login")
///     StatusText             (TextMeshProUGUI)
///
///   ForgotPasswordPanel
///     Title                  (TextMeshProUGUI - "Reset Password")
///     EmailInput             (TMP_InputField)
///     NewPasswordInput       (TMP_InputField - content type: Password)
///     ConfirmNewPasswordInput(TMP_InputField - content type: Password)
///     ResetButton            (Button - "Reset Password")
///     BackButton             (Button - "Back to Login")
///     StatusText             (TextMeshProUGUI)
///
///   GuestPanel
///     Title                  (TextMeshProUGUI - "Play as Guest")
///     GuestNameInput         (TMP_InputField - "Enter a display name")
///     PlayLocalButton        (Button - "Play Local")
///     PlayOnlineButton       (Button - "Play Online")
///     BackButton             (Button - "Back")
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject registerPanel;
    [SerializeField] private GameObject forgotPasswordPanel;
    [SerializeField] private GameObject guestPanel;

    [Header("Login Panel")]
    [SerializeField] private TMP_InputField loginEmailInput;
    [SerializeField] private TMP_InputField loginPasswordInput;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button forgotPasswordButton;
    [SerializeField] private Button goToRegisterButton;
    [SerializeField] private Button guestButton;
    [SerializeField] private TextMeshProUGUI loginStatusText;

    [Header("Register Panel")]
    [SerializeField] private TMP_InputField registerUsernameInput;
    [SerializeField] private TMP_InputField registerEmailInput;
    [SerializeField] private TMP_InputField registerPasswordInput;
    [SerializeField] private TMP_InputField registerConfirmPasswordInput;
    [SerializeField] private Button registerButton;
    [SerializeField] private Button backToLoginFromRegisterButton;
    [SerializeField] private TextMeshProUGUI registerStatusText;

    [Header("Forgot Password Panel")]
    [SerializeField] private TMP_InputField forgotEmailInput;
    [SerializeField] private TMP_InputField newPasswordInput;
    [SerializeField] private TMP_InputField confirmNewPasswordInput;
    [SerializeField] private Button resetPasswordButton;
    [SerializeField] private Button backToLoginFromForgotButton;
    [SerializeField] private TextMeshProUGUI forgotStatusText;

    [Header("Guest Panel")]
    [SerializeField] private TMP_InputField guestNameInput;
    [SerializeField] private Button playLocalButton;
    [SerializeField] private Button playOnlineButton;
    [SerializeField] private Button backFromGuestButton;

    // ── Lifecycle ──────────────────────────────────────────────────────────
    private void Start()
    {
        // Wire up buttons
        loginButton.onClick.AddListener(OnLogin);
        forgotPasswordButton.onClick.AddListener(() => ShowPanel(forgotPasswordPanel));
        goToRegisterButton.onClick.AddListener(() => ShowPanel(registerPanel));
        guestButton.onClick.AddListener(() => ShowPanel(guestPanel));

        registerButton.onClick.AddListener(OnRegister);
        backToLoginFromRegisterButton.onClick.AddListener(() => ShowPanel(loginPanel));

        resetPasswordButton.onClick.AddListener(OnResetPassword);
        backToLoginFromForgotButton.onClick.AddListener(() => ShowPanel(loginPanel));

        playLocalButton.onClick.AddListener(OnPlayLocal);
        playOnlineButton.onClick.AddListener(OnPlayOnline);
        backFromGuestButton.onClick.AddListener(() => ShowPanel(loginPanel));

        // Start on login panel
        ShowPanel(loginPanel);

        // Clear any old status messages
        ClearStatus();
    }

    // ── Panel Navigation ───────────────────────────────────────────────────
    private void ShowPanel(GameObject panel)
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(false);
        forgotPasswordPanel.SetActive(false);
        guestPanel.SetActive(false);

        panel.SetActive(true);
        ClearStatus();
    }

    private void ClearStatus()
    {
        if (loginStatusText != null) loginStatusText.text = "";
        if (registerStatusText != null) registerStatusText.text = "";
        if (forgotStatusText != null) forgotStatusText.text = "";
    }

    // ══════════════════════════════════════════════════════════════════════
    // LOGIN
    // ══════════════════════════════════════════════════════════════════════
    private void OnLogin()
    {
        string email = loginEmailInput.text.Trim().ToLower();
        string password = loginPasswordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            loginStatusText.text = "Please enter your email and password.";
            loginStatusText.color = Color.red;
            return;
        }

        if (!IsValidEmail(email))
        {
            loginStatusText.text = "Please enter a valid email address.";
            loginStatusText.color = Color.red;
            return;
        }

        // Check credentials against stored accounts
        string storedPassword = PlayerPrefs.GetString("account_" + email + "_password", "");
        if (string.IsNullOrEmpty(storedPassword))
        {
            loginStatusText.text = "No account found with that email.";
            loginStatusText.color = Color.red;
            return;
        }

        if (storedPassword != HashPassword(password))
        {
            loginStatusText.text = "Incorrect password.";
            loginStatusText.color = Color.red;
            return;
        }

        // Success
        string username = PlayerPrefs.GetString("account_" + email + "_username", email);
        loginStatusText.text = "Welcome back, " + username + "!";
        loginStatusText.color = Color.green;

        PlayerPrefs.SetString("LoggedInEmail", email);
        PlayerPrefs.SetString("LoggedInUsername", username);
        PlayerPrefs.SetString("P1", username);

        StartCoroutine(LoadSceneAfterDelay("LobbyScene", 1f));
    }

    // ══════════════════════════════════════════════════════════════════════
    // REGISTER
    // ══════════════════════════════════════════════════════════════════════
    private void OnRegister()
    {
        string username = registerUsernameInput.text.Trim();
        string email = registerEmailInput.text.Trim().ToLower();
        string password = registerPasswordInput.text;
        string confirmPassword = registerConfirmPasswordInput.text;

        // Validation
        if (string.IsNullOrEmpty(username))
        {
            SetStatus(registerStatusText, "Please enter a display name.", false);
            return;
        }

        if (!IsValidEmail(email))
        {
            SetStatus(registerStatusText, "Please enter a valid email address.", false);
            return;
        }

        if (password.Length < 6)
        {
            SetStatus(registerStatusText, "Password must be at least 6 characters.", false);
            return;
        }

        if (password != confirmPassword)
        {
            SetStatus(registerStatusText, "Passwords do not match.", false);
            return;
        }

        // Check if email already registered
        string existing = PlayerPrefs.GetString("account_" + email + "_password", "");
        if (!string.IsNullOrEmpty(existing))
        {
            SetStatus(registerStatusText, "An account with that email already exists.", false);
            return;
        }

        // Save account
        PlayerPrefs.SetString("account_" + email + "_password", HashPassword(password));
        PlayerPrefs.SetString("account_" + email + "_username", username);
        PlayerPrefs.Save();

        SetStatus(registerStatusText, "Account created! You can now log in.", true);
        StartCoroutine(SwitchToLoginAfterDelay(1.5f));
    }

    // ══════════════════════════════════════════════════════════════════════
    // FORGOT PASSWORD / RESET
    // ══════════════════════════════════════════════════════════════════════
    private void OnResetPassword()
    {
        string email = forgotEmailInput.text.Trim().ToLower();
        string newPassword = newPasswordInput.text;
        string confirmPassword = confirmNewPasswordInput.text;

        if (!IsValidEmail(email))
        {
            SetStatus(forgotStatusText, "Please enter a valid email address.", false);
            return;
        }

        // Check account exists
        string stored = PlayerPrefs.GetString("account_" + email + "_password", "");
        if (string.IsNullOrEmpty(stored))
        {
            SetStatus(forgotStatusText, "No account found with that email.", false);
            return;
        }

        if (newPassword.Length < 6)
        {
            SetStatus(forgotStatusText, "Password must be at least 6 characters.", false);
            return;
        }

        if (newPassword != confirmPassword)
        {
            SetStatus(forgotStatusText, "Passwords do not match.", false);
            return;
        }

        // Update password
        PlayerPrefs.SetString("account_" + email + "_password", HashPassword(newPassword));
        PlayerPrefs.Save();

        SetStatus(forgotStatusText, "Password reset successfully! You can now log in.", true);
        StartCoroutine(SwitchToLoginAfterDelay(2f));
    }

    // ══════════════════════════════════════════════════════════════════════
    // GUEST MODE
    // ══════════════════════════════════════════════════════════════════════
    private void OnPlayLocal()
    {
        string name = guestNameInput.text.Trim();
        if (string.IsNullOrEmpty(name)) name = "Guest";

        PlayerPrefs.SetString("P1", name);
        PlayerPrefs.SetString("P2", "Player 2");
        PlayerPrefs.SetString("LoggedInUsername", name);
        PlayerPrefs.SetInt("MyPlayerNumber", 0);

        SceneManager.LoadScene("GameScene");
    }

    private void OnPlayOnline()
    {
        string name = guestNameInput.text.Trim();
        if (string.IsNullOrEmpty(name)) name = "Guest";

        PlayerPrefs.SetString("P1", name);
        PlayerPrefs.SetString("LoggedInUsername", name);

        SceneManager.LoadScene("LobbyScene");
    }

    // ══════════════════════════════════════════════════════════════════════
    // HELPERS
    // ══════════════════════════════════════════════════════════════════════
    private void SetStatus(TextMeshProUGUI text, string message, bool success)
    {
        if (text == null) return;
        text.text = message;
        text.color = success ? Color.green : Color.red;
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email)) return false;
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch { return false; }
    }

    /// <summary>
    /// Simple hash so passwords aren't stored as plain text in PlayerPrefs.
    /// Replace this with proper hashing when Firebase is added.
    /// </summary>
    private string HashPassword(string password)
    {
        int hash = 17;
        foreach (char c in password)
            hash = hash * 31 + c;
        return hash.ToString();
    }

    private IEnumerator LoadSceneAfterDelay(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator SwitchToLoginAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowPanel(loginPanel);
    }
}