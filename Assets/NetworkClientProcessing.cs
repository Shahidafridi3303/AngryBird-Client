using System;
using System.Collections.Generic;
using UnityEngine;

static public class NetworkClientProcessing
{
    static TicTacToeManager ticTacToeManager;

    public static void SetTicTacToeManager(TicTacToeManager manager)
    {
        ticTacToeManager = manager;
    }

    static public void ReceivedMessageFromServer(string msg, TransportPipeline pipeline)
    {
        string[] csv = msg.Split(',');
        int signifier = int.Parse(csv[0]);

        LoginManager loginManager = UnityEngine.Object.FindObjectOfType<LoginManager>();

        if (signifier == ServerToClientSignifiers.GameRoomCreatedOrJoined)
        {
            string roomName = csv[1];
            int playerCount = int.Parse(csv[2]);
            loginManager.roomStatusText.text = $"Joined room: {roomName}. Waiting for players... ({playerCount}/2)";

            // Reactivate ChatManager
            ChatManager chatManager = UnityEngine.Object.FindObjectOfType<ChatManager>();
            if (chatManager != null)
            {
                chatManager.ResetChat(); // Reset chat state
                chatManager.gameObject.SetActive(true);
            }
        }
        else if (signifier == ServerToClientSignifiers.StartGame)
        {
            string roomName = csv[1];
            string role = csv[2]; // "X" or "O"
            int turn = int.Parse(csv[3]); // 1 for turn, 0 otherwise

            if (ticTacToeManager != null)
            {
                ticTacToeManager.InitializePlayer(role, turn, roomName); // Include roomName as the third argument
            }

            if (loginManager != null)
            {
                loginManager.StartGame(); 
            }
        }

        else if (signifier == ServerToClientSignifiers.OpponentMessage)
        {
            string message = csv[1];
            Debug.Log($"Message from opponent: {message}");

            ChatManager chatManager = UnityEngine.Object.FindObjectOfType<ChatManager>();
            if (chatManager != null)
            {
                chatManager.DisplayIncomingMessage(message);
            }
        }
        else if (signifier == ServerToClientSignifiers.AccountCreated)
        {
            loginManager.ShowFeedback("Account created successfully!");

            SendMessageToServer($"{ClientToServerSignifiers.RequestAccountList}", TransportPipeline.ReliableAndInOrder);
        }

        else if (signifier == ServerToClientSignifiers.AccountCreationFailed)
        {
            loginManager.ShowFeedback("Account creation failed. Username already exists.");
        }
        else if (signifier == ServerToClientSignifiers.LoginSuccessful)
        {
            loginManager.ShowFeedback("Login successful!");
            loginManager.SetUIState(UIState.GameRoomWaiting);
        }
        else if (signifier == ServerToClientSignifiers.LoginFailed)
        {
            loginManager.ShowFeedback("Login failed. Invalid credentials.");
        }
        else if (signifier == ServerToClientSignifiers.AccountDeleted)
        {
            if (csv.Length > 1)
            {
                string deletedAccount = csv[1];
                loginManager.ShowFeedback($"Account '{deletedAccount}' deleted successfully.");
            }
            else
            {
                loginManager.ShowFeedback("Account deleted successfully, but no account name was provided.");
            }
        }
        else if (signifier == ServerToClientSignifiers.AccountDeletionFailed)
        {
            if (csv.Length > 1)
            {
                string failedAccount = csv[1];
                loginManager.ShowFeedback($"Failed to delete account '{failedAccount}'.");
            }
            else
            {
                loginManager.ShowFeedback("Account deletion failed. No additional details provided.");
            }
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
        else if (signifier == ServerToClientSignifiers.PlayerMove)
        {
            int x = int.Parse(csv[1]);
            int y = int.Parse(csv[2]);
            int player = int.Parse(csv[3]);

            Debug.Log($"Processing PlayerMove: x = {x}, y = {y}, player = {player}");

            if (ticTacToeManager != null)
            {
                ticTacToeManager.UpdateCell(x, y, player); // Update UI for all clients (players and observers)
            }
        }
        else if (signifier == ServerToClientSignifiers.TurnUpdate)
        {
            int isPlayerTurn = int.Parse(csv[1]);
            Debug.Log($"Processing TurnUpdate: IsPlayerTurn = {isPlayerTurn}");

            if (ticTacToeManager != null)
            {
                ticTacToeManager.SetPlayerTurn(isPlayerTurn == 1);
            }
        }
        else if (signifier == ServerToClientSignifiers.GameResult)
        {
            int result = int.Parse(csv[1]);
            ticTacToeManager.ShowGameResult(result);
        }

        else if (signifier == ServerToClientSignifiers.OpponentMessage)
        {
            string message = csv[1];
            Debug.Log($"Message from opponent: {message}");

            ChatManager chatManager = UnityEngine.Object.FindObjectOfType<ChatManager>();
            if (chatManager != null)
            {
                chatManager.DisplayIncomingMessage(message);
            }
        }
        else if (signifier == ServerToClientSignifiers.ObserverJoined)
        {
            string roomName = csv[1];
            Debug.Log($"Joined room {roomName} as an observer.");

            if (loginManager != null)
            {
                loginManager.SetUIState(UIState.GameRoomPlaying); 
                loginManager.SetObserverUI(roomName);
                Debug.Log("TicTacToe panel activated for observer.");
            }

            // Deactivate ChatManager panel
            ChatManager chatManager = UnityEngine.Object.FindObjectOfType<ChatManager>();
            if (chatManager != null)
            {
                chatManager.ResetChat(); 
                chatManager.gameObject.SetActive(false); 
            }

            TicTacToeManager ticTacToeManager = UnityEngine.Object.FindObjectOfType<TicTacToeManager>();
            if (ticTacToeManager != null)
            {
                ticTacToeManager.InitializeObserver(roomName);
            }
        }

        else if (signifier == ServerToClientSignifiers.BoardStateUpdate)
        {
            int x = int.Parse(csv[1]);
            int y = int.Parse(csv[2]);
            int playerMark = int.Parse(csv[3]);

            Debug.Log($"Received board state: Player {playerMark} at ({x}, {y})");

            if (ticTacToeManager != null)
            {
                ticTacToeManager.UpdateCell(x, y, playerMark);
            }
        }

        else if (signifier == ServerToClientSignifiers.GameRoomDestroyed)
        {
            Debug.Log("Received GameRoomDestroyed signal. Returning to GameRoom panel...");

            if (loginManager != null)
            {
                loginManager.SetUIState(UIState.GameRoomWaiting);
                loginManager.roomStatusText.text = "Game has ended. Please join or create a new room.";
            }

            if (ticTacToeManager != null)
            {
                ticTacToeManager.StartNewGame();
            }

            ChatManager chatManager = UnityEngine.Object.FindObjectOfType<ChatManager>();
            if (chatManager != null)
            {
                chatManager.ResetChat();
            }
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

    static public void OnApplicationQuit()
    {
        DisconnectFromServer();
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
    public const int DeleteAccount = 3;

    public const int CreateOrJoinGameRoom = 4;
    public const int LeaveGameRoom = 5;
    public const int SendMessageToOpponent = 6;
    public const int PlayerMove = 11;
    public const int RequestAccountList = 13;
}

public static class ServerToClientSignifiers
{
    public const int AccountCreated = 1;
    public const int AccountCreationFailed = 2;
    public const int LoginSuccessful = 3;
    public const int LoginFailed = 4;
    public const int AccountList = 5;
    public const int AccountDeleted = 6; 
    public const int AccountDeletionFailed = 7; 

    public const int GameRoomCreatedOrJoined = 8;
    public const int StartGame = 9;
    public const int OpponentMessage = 10;
    public const int ObserverJoined = 14; 
    public const int PlayerMove = 11; 
    public const int GameResult = 12; 
    public const int TurnUpdate = 13;

    public const int BoardStateUpdate = 15; 
    public const int GameRoomDestroyed = 16; 
}
