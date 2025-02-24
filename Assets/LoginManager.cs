using UnityEngine;
using TMPro; // For TextMeshPro
using System.Collections.Generic;
using UnityEngine.UI; // For Button

public enum UIState
{
    Login,
    GameRoomWaiting,
    GameRoomPlaying
}

public class LoginManager : MonoBehaviour
{
    public GameObject loginPanel;
    public GameObject gameRoomPanel; // Replace lobbyPanel with gameRoomPanel
    public GameObject ticTacToePanel; // Add a reference to the TicTacToePanel
    public GameObject chatPanel; // Reference to ChatManager panel
    public TMP_InputField usernameField;
    public TMP_InputField passwordField;
    public TextMeshProUGUI feedbackText;
    public TMP_Dropdown accountDropdown;
    public TMP_InputField roomNameField; // Input for room name
    public TextMeshProUGUI roomStatusText; // Text for room status
    public GameObject resultPanel; 
    public TextMeshProUGUI resultPanelMessage; 

    private string currentRoomName = "";

    // Local storage for account passwords
    private Dictionary<string, string> accountPasswordMap = new Dictionary<string, string>();

    private void Start()
    {
        SetUIState(UIState.Login);
    }

    public void SetUIState(UIState state)
    {
        loginPanel.SetActive(state == UIState.Login);
        gameRoomPanel.SetActive(state == UIState.GameRoomWaiting || state == UIState.GameRoomPlaying);
        ticTacToePanel.SetActive(state == UIState.GameRoomPlaying); // Show TicTacToePanel only when playing
        chatPanel.SetActive(state == UIState.GameRoomPlaying); // Show ChatPanel only when playing
    }

    public void OnLoginButtonPressed()
    {
        string username = usernameField.text;
        string password = passwordField.text;

        if (string.IsNullOrEmpty(username))
        {
            ShowFeedback("Please enter a username.");
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

        Debug.Log($"Create account button pressed. Username: {username}");
        NetworkClientProcessing.SendMessageToServer($"1,{username},{password}", TransportPipeline.ReliableAndInOrder);
    }

    public void ShowFeedback(string message)
    {
        feedbackText.text = message;
    }

    public void OnCreateOrJoinRoomPressed()
    {
        string roomName = roomNameField.text;

        if (string.IsNullOrEmpty(roomName))
        {
            roomStatusText.text = "Room name cannot be empty.";
            return;
        }

        currentRoomName = roomName;
        Debug.Log($"Attempting to join or create room: {roomName}");
        NetworkClientProcessing.SendMessageToServer($"4,{roomName}", TransportPipeline.ReliableAndInOrder);

        roomStatusText.text = $"Attempting to join room: {roomName}";
        SetUIState(UIState.GameRoomWaiting);
    }

    public void OnLeaveRoomPressed()
    {
        if (!string.IsNullOrEmpty(currentRoomName))
        {
            Debug.Log($"Leaving room: {currentRoomName}");

            // Notify the server about leaving the room
            NetworkClientProcessing.SendMessageToServer($"5,{currentRoomName}", TransportPipeline.ReliableAndInOrder);
            currentRoomName = ""; // Clear the current room locally

            // Reset TicTacToePanel, ResultPanel, and ChatManager
            if (ticTacToePanel != null)
            {
                ticTacToePanel.SetActive(false); // Deactivate the game panel
            }

            if (chatPanel != null)
            {
                ChatManager chatManager = FindObjectOfType<ChatManager>();
                if (chatManager != null)
                {
                    chatManager.ResetChat(); // Reset and deactivate chat
                }
            }

            if (resultPanel != null)
            {
                resultPanel.SetActive(false); // Deactivate the result panel
            }

            // Return to GameRoomPanel
            SetUIState(UIState.GameRoomWaiting);
        }
    }


    public void OnPlayAgainButtonPressed()
    {
        Debug.Log("Play Again button pressed. Returning to GameRoom...");

        // Deactivate the ResultPanel
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        // Return to the GameRoomPanel
        SetUIState(UIState.GameRoomWaiting);
    }

    public void OnQuitGameButtonPressed()
    {
        Debug.Log("Quit Game button pressed. Exiting application...");
        Application.Quit(); // Quit the application
    }

    public void StartGame()
    {
        roomStatusText.text = "Game Started! You can now play with your opponent.";
        SetUIState(UIState.GameRoomPlaying);
        gameRoomPanel.SetActive(false); // Deactivate GameRoomPanel
        ticTacToePanel.SetActive(true); // Activate TicTacToePanel
        chatPanel.SetActive(true); // Activate ChatManager panel

        ChatManager chatManager = FindObjectOfType<ChatManager>();
        if (chatManager != null)
        {
            chatManager.InitializeChat(currentRoomName); // Initialize chat with room name
        }

        Debug.Log("TicTacToe panel and ChatManager panel activated");
    }


    public void OnDeleteAccountButtonPressed()
    {
        int selectedIndex = accountDropdown.value;

        if (selectedIndex == 0) // Ensure a valid account is selected
        {
            ShowFeedback("Please select an account to delete.");
            return;
        }

        string selectedAccount = accountDropdown.options[selectedIndex].text;

        // Send delete request to the server
        NetworkClientProcessing.SendMessageToServer($"3,{selectedAccount}", TransportPipeline.ReliableAndInOrder);
        Debug.Log($"Delete request sent for account: {selectedAccount}");
    }

    public void RefreshAccountDropdown(List<string> accountNames)
    {
        accountDropdown.ClearOptions();
        accountNames.Insert(0, "Select Account"); // Add a default "Select Account" option
        accountDropdown.AddOptions(accountNames);
        accountDropdown.value = 0; // Reset dropdown to the default option
    }

    public void PopulateAccountDropdown(List<string> accountNames, Dictionary<string, string> passwords)
    {
        accountPasswordMap = passwords; // Save the username-password mapping

        if (accountNames == null || accountNames.Count == 0)
        {
            Debug.LogWarning("Account list is null or empty. Adding default option.");
            accountNames = new List<string> { "Select Account" }; // Add default option
        }

        accountDropdown.ClearOptions(); // Clear existing options
        accountNames.Insert(0, "Select Account"); // Add default "Select Account" option
        accountDropdown.AddOptions(accountNames); // Add new options
        accountDropdown.value = 0; // Reset dropdown to the default option
    }

    public void OnAccountSelected(int index)
    {
        // Fetch the actual selected index directly from the dropdown
        int selectedIndex = accountDropdown.value;

        if (selectedIndex > 0) // Skip the default "Select Account" option
        {
            string selectedUsername = accountDropdown.options[selectedIndex].text;
            Debug.Log($"Selected Username: {selectedUsername}");

            // Autofill username field
            usernameField.text = selectedUsername;

            // Autofill password if available
            if (accountPasswordMap.ContainsKey(selectedUsername))
            {
                passwordField.text = accountPasswordMap[selectedUsername];
            }
            else
            {
                passwordField.text = "";
            }
        }
        else
        {
            usernameField.text = "";
            passwordField.text = "";
            Debug.Log("Resetting fields as 'Select Account' was chosen.");
        }
    }
}
