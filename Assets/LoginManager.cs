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
    public GameObject gameRoomPanel;
    public GameObject ticTacToePanel; 
    public GameObject chatPanel;
    public TMP_InputField usernameField;
    public TMP_InputField passwordField;
    public TextMeshProUGUI feedbackText;
    public TMP_Dropdown accountDropdown;
    public TMP_InputField roomNameField; 
    public TextMeshProUGUI roomStatusText;
    public GameObject resultPanel; 
    public TextMeshProUGUI resultPanelMessage; 

    private string currentRoomName = "";

    private Dictionary<string, string> accountPasswordMap = new Dictionary<string, string>();

    private void Start()
    {
        SetUIState(UIState.Login);
    }

    public void SetUIState(UIState state)
    {
        loginPanel.SetActive(state == UIState.Login);
        gameRoomPanel.SetActive(state == UIState.GameRoomWaiting || state == UIState.GameRoomPlaying);
        ticTacToePanel.SetActive(state == UIState.GameRoomPlaying); 
        chatPanel.SetActive(state == UIState.GameRoomPlaying);
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

            // Reset panels
            if (ticTacToePanel != null)
            {
                ticTacToePanel.SetActive(false); 
            }

            if (chatPanel != null)
            {
                ChatManager chatManager = FindObjectOfType<ChatManager>();
                if (chatManager != null)
                {
                    chatManager.ResetChat();
                }
            }

            if (resultPanel != null)
            {
                resultPanel.SetActive(false); 
            }

            // Return to GameRoomPanel
            gameRoomPanel.SetActive(true);
            roomStatusText.text = "You are now in the Game Room.";
            SetUIState(UIState.GameRoomWaiting);
        }
        else
        {
            Debug.Log("No room to leave.");
        }
    }


    public void OnLeaveGameRoomPressed()
    {
        Debug.Log("Leaving the GameRoom panel...");

        // Reset the room name (if set)
        currentRoomName = "";

        loginPanel.SetActive(true);
        gameRoomPanel.SetActive(false);

        feedbackText.text = "Create or login to an account";

        SetUIState(UIState.Login);

        Debug.Log("Returned to the LoginPanel from the GameRoomPanel.");
    }

    public void OnPlayAgainButtonPressed()
    {
        Debug.Log("Play Again button pressed. Returning to GameRoom...");

        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        SetUIState(UIState.GameRoomWaiting);
    }

    public void OnQuitGameButtonPressed()
    {
        Debug.Log("Quit Game button pressed.");

        // Ensure proper cleanup before quitting
        NetworkClientProcessing.OnApplicationQuit();

#if UNITY_EDITOR
        // Stop play mode in the Unity Editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
    // Quit the application in the built version
    Application.Quit();
#endif
    }

    private void OnApplicationQuit()
    {
        NetworkClientProcessing.OnApplicationQuit();
    }

    public void StartGame()
    {
        roomStatusText.text = "Game Started! You can now play with your opponent.";
        SetUIState(UIState.GameRoomPlaying);
        gameRoomPanel.SetActive(false); 
        ticTacToePanel.SetActive(true); 
        chatPanel.SetActive(true);

        ChatManager chatManager = FindObjectOfType<ChatManager>();
        if (chatManager != null)
        {
            chatManager.InitializeChat(currentRoomName);
        }

        Debug.Log("TicTacToe panel and ChatManager panel activated");
    }

    public void OnDeleteAccountButtonPressed()
    {
        int selectedIndex = accountDropdown.value;

        if (selectedIndex == 0) 
        {
            ShowFeedback("Please select an account to delete.");
            return;
        }

        string selectedAccount = accountDropdown.options[selectedIndex].text;

        if (accountPasswordMap.TryGetValue(selectedAccount, out string selectedPassword))
        {
            // Send delete request to the server
            NetworkClientProcessing.SendMessageToServer($"3,{selectedAccount},{selectedPassword}", TransportPipeline.ReliableAndInOrder);
            ShowFeedback($"Delete request sent for account: {selectedAccount}");

            // Remove the account locally to reflect the deletion immediately
            accountPasswordMap.Remove(selectedAccount);
            RefreshAccountDropdown(new List<string>(accountPasswordMap.Keys));
        }
        else
        {
            ShowFeedback($"Password for account '{selectedAccount}' not found. Cannot delete.");
        }
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
            accountNames = new List<string> { "Select Account" };
        }

        accountDropdown.ClearOptions(); 
        accountNames.Insert(0, "Select Account"); 
        accountDropdown.AddOptions(accountNames);
        accountDropdown.value = 0;
    }

    public void SetObserverUI(string roomName)
    {
        gameRoomPanel.SetActive(false); 
        ticTacToePanel.SetActive(true); 
        roomStatusText.text = $"Observing game in room: {roomName}"; 
    }

    public void OnAccountSelected(int index)
    {
        // Fetch the actual selected index directly from the dropdown
        int selectedIndex = accountDropdown.value;

        if (selectedIndex > 0) 
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
