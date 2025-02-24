using UnityEngine;
using TMPro; // For TextMeshPro
using UnityEngine.UI;

public enum UIState
{
    Login,
    Lobby
}

public class LoginManager : MonoBehaviour
{
    public GameObject loginPanel;
    public GameObject lobbyPanel;
    public TMP_InputField usernameField;
    public TMP_InputField passwordField;
    public TextMeshProUGUI feedbackText;

    private void Start()
        SetUIState(UIState.Login);
    }

    public void SetUIState(UIState state)
    {
        if (state == UIState.Login)
        {
            loginPanel.SetActive(true);
            lobbyPanel.SetActive(false);
        }
        else if (state == UIState.Lobby)
        {
            loginPanel.SetActive(false);
            lobbyPanel.SetActive(true);
        }
    }

    public void OnLoginButtonPressed()
    {
        string username = usernameField.text;
        string password = passwordField.text;
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowFeedback("Username and password cannot be empty.");
            return;
        }
        NetworkClientProcessing.SendMessageToServer($"2,{username},{password}", TransportPipeline.ReliableAndInOrder);
    }

    public void OnCreateAccountButtonPressed()
    {
        string username = usernameField.text;
        string password = passwordField.text;
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowFeedback("Username and password cannot be empty.");
            return;
        }
        NetworkClientProcessing.SendMessageToServer($"1,{username},{password}", TransportPipeline.ReliableAndInOrder);
    }

    public void ShowFeedback(string message)
    {
        feedbackText.text = message;
    }
}