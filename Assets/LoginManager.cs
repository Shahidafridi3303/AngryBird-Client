using UnityEngine;
using TMPro; // For TextMeshPro
using System.Collections.Generic;
using UnityEngine.UI; // For Button

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
    public TMP_Dropdown accountDropdown;

    // Local storage for account passwords
    private Dictionary<string, string> accountPasswordMap = new Dictionary<string, string>();

    private void Start()
    {
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

        Debug.Log($"Login button pressed. Username: {username}, Password: {password}");

        if (string.IsNullOrEmpty(username))
        {
            ShowFeedback("Please select or enter a username.");
            return;
        }

        // Send login data to the server
        NetworkClientProcessing.SendMessageToServer($"2,{username},{password}", TransportPipeline.ReliableAndInOrder);
        Debug.Log($"Attempting login with Username: {username}, Password: {password}");
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

    public void ShowFeedback(string message)
    {
        feedbackText.text = message;
    }
}
