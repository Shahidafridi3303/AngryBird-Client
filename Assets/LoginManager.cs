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
    public Button deleteAccountButton; // Reference to the delete account button

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
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowFeedback("Username and password cannot be empty.");
            return;
        }
        NetworkClientProcessing.SendMessageToServer($"2,{username},{password}", TransportPipeline.ReliableAndInOrder);
    }

    public void RefreshAccountDropdown(List<string> accountNames)
    {
        accountDropdown.ClearOptions();
        accountNames.Insert(0, "Select Account"); // Add a default "Select Account" option
        accountDropdown.AddOptions(accountNames);
        accountDropdown.value = 0; // Reset dropdown to the default option
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
        NetworkClientProcessing.SendMessageToServer($"3,{selectedAccount}", TransportPipeline.ReliableAndInOrder);
    }

    public void PopulateAccountDropdown(List<string> accountNames)
    {
        if (accountNames == null || accountNames.Count == 0)
        {
            Debug.LogWarning("Account list is null or empty. Adding default option.");
            accountNames = new List<string> { "Select Account" }; // Add default option
        }

        Debug.Log($"Populating dropdown with accounts: {string.Join(", ", accountNames)}");

        if (accountDropdown == null)
        {
            Debug.LogError("AccountDropdown is not assigned in LoginManager!");
            return;
        }

        accountDropdown.ClearOptions(); // Clear existing options
        accountNames.Insert(0, "Select Account"); // Add default "Select Account" option
        accountDropdown.AddOptions(accountNames); // Add new options
        accountDropdown.value = 0; // Reset dropdown to the default option
    }


    public void OnAccountSelected(int index)
    {
        if (index > 0) // Skip the default "Select Account" option
        {
            usernameField.text = accountDropdown.options[index].text; // Autofill the username field
        }
    }

    public void ShowFeedback(string message)
    {
        feedbackText.text = message;
    }
}
