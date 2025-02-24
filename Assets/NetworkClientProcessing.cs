using System.Collections.Generic;
using UnityEngine;

static public class NetworkClientProcessing
{
    static public void ReceivedMessageFromServer(string msg, TransportPipeline pipeline)
    {
        string[] csv = msg.Split(',');
        int signifier = int.Parse(csv[0]);

        LoginManager loginManager = Object.FindObjectOfType<LoginManager>();

        if (signifier == ServerToClientSignifiers.AccountCreated)
        {
            loginManager.ShowFeedback("Account created successfully!");
        }
        else if (signifier == ServerToClientSignifiers.AccountCreationFailed)
        {
            loginManager.ShowFeedback("Account creation failed. Username already exists.");
        }
        else if (signifier == ServerToClientSignifiers.LoginSuccessful)
        {
            loginManager.ShowFeedback("Login successful!");
            loginManager.SetUIState(UIState.Lobby);
        }
        else if (signifier == ServerToClientSignifiers.LoginFailed)
        {
            loginManager.ShowFeedback("Login failed. Invalid credentials.");
        }
        else if (signifier == ServerToClientSignifiers.AccountList)
        {
            Debug.Log($"Raw Account List Received: {msg}");

            if (csv.Length > 1)
            {
                string accountListRaw = msg.Substring(msg.IndexOf(',') + 1); // Extract everything after the first comma
                string[] accountEntries = accountListRaw.Split(','); // Split by commas
                List<string> accounts = new List<string>();
                Dictionary<string, string> passwords = new Dictionary<string, string>();

                foreach (string entry in accountEntries)
                {
                    if (entry.Contains(":"))
                    {
                        string[] pair = entry.Split(':'); // Split into username and password
                        if (pair.Length == 2)
                        {
                            string username = pair[0];
                            string password = pair[1];
                            accounts.Add(username); // Add to accounts list
                            passwords[username] = password; // Map username to password
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Invalid account entry: {entry}");
                    }
                }

                Debug.Log($"Processed Accounts: {string.Join(", ", accounts)}");

                if (loginManager != null)
                {
                    loginManager.PopulateAccountDropdown(accounts, passwords);
                }
                else
                {
                    Debug.LogError("LoginManager not found in the scene!");
                }
            }
            else
            {
                Debug.LogError("Account list message is malformed or empty.");
            }
        }

        else if (signifier == ServerToClientSignifiers.AccountDeleted)
        {
            string deletedAccount = csv[1];
            loginManager.ShowFeedback($"Account '{deletedAccount}' deleted successfully.");
        }
        else if (signifier == ServerToClientSignifiers.AccountDeletionFailed)
        {
            string failedAccount = csv[1];
            loginManager.ShowFeedback($"Failed to delete account '{failedAccount}'.");
        }
    }


    static public void SendMessageToServer(string msg, TransportPipeline pipeline)
    {
        networkClient.SendMessageToServer(msg, pipeline);
    }

    #region Connection Management

    static NetworkClient networkClient;

    static public void SetNetworkedClient(NetworkClient NetworkClient)
    {
        networkClient = NetworkClient;
    }

    static public void ConnectionEvent()
    {
        Debug.Log("Network Connection Event!");
    }

    static public void DisconnectionEvent()
    {
        Debug.Log("Network Disconnection Event!");
    }

    static public bool IsConnectedToServer()
    {
        return networkClient.IsConnected();
    }

    static public void ConnectToServer()
    {
        networkClient.Connect();
    }

    static public void DisconnectFromServer()
    {
        networkClient.Disconnect();
    }

    #endregion

    #region Game Logic Management

    static GameLogic gameLogic;

    static public void SetGameLogic(GameLogic GameLogic)
    {
        gameLogic = GameLogic;
    }

    static public NetworkClient GetNetworkedClient()
    {
        return networkClient;
    }

    #endregion
}

public static class ClientToServerSignifiers
{
    public const int CreateAccount = 1;
    public const int Login = 2;
    public const int DeleteAccount = 3; // New signifier for deleting accounts
}

public static class ServerToClientSignifiers
{
    public const int AccountCreated = 1;
    public const int AccountCreationFailed = 2;
    public const int LoginSuccessful = 3;
    public const int LoginFailed = 4;
    public const int AccountList = 5;
    public const int AccountDeleted = 6; // New signifier for successful deletion
    public const int AccountDeletionFailed = 7; // New signifier for failed deletion
}
