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
    public TMP_InputField usernameField;
    public TMP_InputField passwordField;
    public TextMeshProUGUI feedbackText;
    public TMP_Dropdown accountDropdown;
    public TMP_InputField roomNameField; // Input for room name
    public TextMeshProUGUI roomStatusText; // Text for room status

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

        Debug.Log($"Login button pressed. Username: {username}, Password: {password}");
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
            NetworkClientProcessing.SendMessageToServer($"5,{currentRoomName}", TransportPipeline.ReliableAndInOrder);
            currentRoomName = "";
            SetUIState(UIState.Login); // Go back to login state
        }
    }

    public void StartGame()
    {
        roomStatusText.text = "Game Started! You can now play with your opponent.";
        SetUIState(UIState.GameRoomPlaying);
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

        Debug.Log("Dropdown populated successfully.");
    }

    public void OnAccountSelected(int index)
    {
        Debug.Log($"Dropdown selection changed. Passed index: {index}");

        // Fetch the actual selected index directly from the dropdown
        int selectedIndex = accountDropdown.value;
        Debug.Log($"Dropdown actual selected index: {selectedIndex}");

        if (selectedIndex > 0) // Skip the default "Select Account" option
        {
            string selectedUsername = accountDropdown.options[selectedIndex].text;
            Debug.Log($"Selected Username: {selectedUsername}");

            // Autofill username field
            usernameField.text = selectedUsername;
            Debug.Log($"Username field updated with: {usernameField.text}");

            // Autofill password if available
            if (accountPasswordMap.ContainsKey(selectedUsername))
            {
                passwordField.text = accountPasswordMap[selectedUsername];
                Debug.Log($"Password field updated with: {passwordField.text}");
            }
            else
            {
                passwordField.text = "";
                Debug.Log($"Password field cleared for: {selectedUsername}");
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
