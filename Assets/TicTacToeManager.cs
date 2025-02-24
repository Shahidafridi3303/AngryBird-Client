using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TicTacToeManager : MonoBehaviour
{
    [SerializeField] private Button[] buttons;
    [SerializeField] private Sprite xSprite, oSprite;
    public TextMeshProUGUI turnText, resultText, gameRoomNameText;

    private string roomName;
    private bool isPlayerTurn = false;
    private int playerID; // 1 for X, 2 for O

    private bool isObserver = false;

    [SerializeField] private TextMeshProUGUI totalTimeText; 
    private float totalTimeElapsed = 0f;
    private bool isGameTimerRunning = false;


    void Start()
    {
        NetworkClientProcessing.SetTicTacToeManager(this);

        // Bind button clicks dynamically
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            buttons[i].onClick.AddListener(() => OnCellClicked(index));
        }
    }

    public void StartGameTimer()
    {
        totalTimeElapsed = 0f;
        isGameTimerRunning = true;
        UpdateTotalTimeText();
    }

    void Update()
    {
        if (isGameTimerRunning)
        {
            totalTimeElapsed += Time.deltaTime;
            UpdateTotalTimeText();
        }
    }

    public void StopGameTimer()
    {
        isGameTimerRunning = false;
    }


    private void UpdateTotalTimeText()
    {
        int minutes = Mathf.FloorToInt(totalTimeElapsed / 60);
        int seconds = Mathf.FloorToInt(totalTimeElapsed % 60);
        totalTimeText.text = $"Time: {minutes:D2}:{seconds:D2}";
    }


    public void StartNewGame()
    {
        roomName = ""; // Clear the previous room name
        turnText.text = "Waiting for opponent..."; // Reset TurnText for all
        resultText.text = ""; // Clear the result text

        foreach (Button button in buttons)
        {
            button.image.sprite = null; // Clear button sprites
            button.interactable = true; // Enable buttons for new game
        }

        StartGameTimer(); // Reset and start the timer
    }


    public void InitializePlayer(string role, int turn, string room)
    {
        playerID = (role == "X") ? 1 : 2;
        isPlayerTurn = (turn == 1); // First player's turn
        roomName = room; // Assign room name
        UpdateTurnText();

        // Reset and activate ChatManager
        ChatManager chatManager = FindObjectOfType<ChatManager>();
        if (chatManager != null)
        {
            chatManager.ResetChat(); 
            chatManager.SetChatActive(true); 
        }
        if (isObserver)
        {
            turnText.text = "Observing game..."; // Observer sees this
        }
        else
        {
            UpdateTurnText(); // Players see turn-related text
        }

        gameRoomNameText.text = $"Room: {roomName}";

        StartGameTimer(); // Start the game timer
        isObserver = false;
    }


    public void UpdateCell(int x, int y, int player)
    {
        int index = x * 3 + y;
        Debug.Log($"Updating cell at ({x}, {y}) for player {player}. Button index: {index}");

        buttons[index].image.sprite = (player == 1) ? xSprite : oSprite; // Update sprite
        buttons[index].interactable = false;

        // Toggle turn indicator
        isPlayerTurn = (player != playerID);
        UpdateTurnText();
    }

    private void UpdateTurnText()
    {
        if (isObserver)
        {
            turnText.text = "Observing game...";
        }
        else if (isPlayerTurn)
        {
            turnText.text = "Your Turn";
        }
        else
        {
            turnText.text = "Opponent's Turn";
        }
    }

    public void OnCellClicked(int index)
    {
        if (isPlayerTurn)
        {
            int x = index / 3;
            int y = index % 3;

            // Debug to confirm room and move details
            Debug.Log($"Sending move to server: Room {roomName}, x: {x}, y: {y}, playerID: {playerID}");

            // Send the room name along with the move
            NetworkClientProcessing.SendMessageToServer($"11,{roomName},{x},{y},{playerID}", TransportPipeline.ReliableAndInOrder);

            buttons[index].image.sprite = (playerID == 1) ? xSprite : oSprite;
            buttons[index].interactable = false;

            isPlayerTurn = false; // Wait for the server to confirm the move
            UpdateTurnText();
        }
        else
        {
            SoundManager.instance.PlayNotYourMoveSound();
            Debug.Log("Not your turn!");
        }
    }

    public void SetPlayerTurn(bool isTurn)
    {
        if (isObserver)
        {
            turnText.text = "Observing game..."; // Observer sees this
        }
        else
        {
            isPlayerTurn = isTurn;

            if (isTurn)
            {
                turnText.text = "Your Turn"; // Player's turn
            }
            else
            {
                turnText.text = "Opponent's Turn"; // Opponent's turn
            }
        }
        UpdateTurnText();
    }

    public void ShowGameResult(int result)
    {
        StopGameTimer(); // Stop the timer

        if (result == 1)
            resultText.text = "Player 1 (X) Wins!";
        else if (result == 2)
            resultText.text = "Player 2 (O) Wins!";
        else
            resultText.text = "It's a Draw!";

        LoginManager loginManager = UnityEngine.Object.FindObjectOfType<LoginManager>();
        if (loginManager != null)
        {
            loginManager.resultPanel.SetActive(true);
            loginManager.resultPanelMessage.text = resultText.text;
        }

        // Reset UI elements for all clients (players and observers)
        if (isObserver)
        {
            turnText.text = "Observing game...";
        }
        else
        {
            turnText.text = ""; // Clear turn text for players
        }
    }

    public void InitializeObserver(string room)
    {
        roomName = room;
        Debug.Log($"Observer initialized for room: {roomName}");

        turnText.gameObject.SetActive(true);
        turnText.text = "Observing game...";

        gameRoomNameText.text = $"Observing Room: {roomName}";

        // Disable interaction for all buttons
        foreach (Button button in buttons)
        {
            button.interactable = false;
        }

        isObserver = true;

        // Reset timer for the observer
        totalTimeElapsed = 0f; 
        UpdateTotalTimeText(); 
        isGameTimerRunning = true; 

        // Disable ChatManager for observers
        ChatManager chatManager = UnityEngine.Object.FindObjectOfType<ChatManager>();
        if (chatManager != null)
        {
            chatManager.SetChatActive(false); // Disable for observers
        }
        else
        {
            Debug.LogError("ChatManager instance not found for observer.");
        }
    }

    public void UpdateBoardForObserver(string serializedBoardState)
    {
        string[] cells = serializedBoardState.Split(',');
        for (int i = 0; i < cells.Length; i++)
        {
            int x = i / 3;
            int y = i % 3;
            int playerMark = int.Parse(cells[i]);

            if (playerMark == 1 || playerMark == 2) // If the cell is occupied
            {
                Sprite sprite = (playerMark == 1) ? xSprite : oSprite;
                buttons[i].image.sprite = sprite;
                buttons[i].interactable = false; // Disable interaction for observers
            }
        }
    }

    public void UpdateBoardState(int[] flattenedBoard)
    {
        for (int i = 0; i < flattenedBoard.Length; i++)
        {
            int value = flattenedBoard[i]; // 0 = empty, 1 = Player 1 (X), 2 = Player 2 (O)
            if (value != 0)
            {
                buttons[i].image.sprite = (value == 1) ? xSprite : oSprite;
                buttons[i].interactable = false; // Ensure buttons are disabled for the observer
            }
        }
    }

}
