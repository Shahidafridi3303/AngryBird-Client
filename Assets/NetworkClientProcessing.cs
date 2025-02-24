using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class NetworkClientProcessing
{

    #region Send and Receive Data Functions
    public static void ReceivedMessageFromServer(string msg, TransportPipeline pipeline)
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
    }


    static public void SendMessageToServer(string msg, TransportPipeline pipeline)
    {
        networkClient.SendMessageToServer(msg, pipeline);
    }

    #endregion

    #region Connection Related Functions and Events
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

    #region Setup
    static NetworkClient networkClient;
    static GameLogic gameLogic;

    static public void SetNetworkedClient(NetworkClient NetworkClient)
    {
        networkClient = NetworkClient;
    }
    static public NetworkClient GetNetworkedClient()
    {
        return networkClient;
    }
    static public void SetGameLogic(GameLogic GameLogic)
    {
        gameLogic = GameLogic;
    }

    #endregion

}

public static class ClientToServerSignifiers
{
    public const int CreateAccount = 1; // For account creation requests
    public const int Login = 2;        // For login requests
}

public static class ServerToClientSignifiers
{
    public const int AccountCreated = 1;           // Response for successful account creation
    public const int AccountCreationFailed = 2;    // Response for failed account creation
    public const int LoginSuccessful = 3;          // Response for successful login
    public const int LoginFailed = 4;              // Response for failed login
}