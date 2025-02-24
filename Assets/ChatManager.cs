using UnityEngine;
using TMPro; // For TextMeshPro
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{
    [SerializeField] private GameObject chatPanel;
    [SerializeField] private TMP_InputField chatInputField;
    [SerializeField] private TextMeshProUGUI chatHistoryText;
    [SerializeField] private Button sendButton;

    [SerializeField] private Color youColor = Color.green; // Default green
    [SerializeField] private Color opponentColor = Color.red; // Default red

    private string roomName;

    private void Start()
    {
        sendButton.onClick.AddListener(OnSendButtonPressed);
    }

    public void InitializeChat(string room)
    {
        roomName = room;
        chatHistoryText.text = ""; // Clear chat history
        chatPanel.SetActive(true);
    }

    private void OnSendButtonPressed()
    {
        string message = chatInputField.text;
        if (!string.IsNullOrEmpty(message))
        {
            // Send message to server
            NetworkClientProcessing.SendMessageToServer($"6,{roomName},{message}", TransportPipeline.ReliableAndInOrder);
            AppendMessage($"<color=#F4FF00>You:</color> {message}"); // yellow for "you"
    
            chatInputField.text = ""; // Clear input field
        }
    }

    public void ResetChat()
    {
        chatHistoryText.text = ""; // Clear chat history
        chatInputField.text = ""; // Clear input field
        chatPanel.SetActive(false); // Deactivate chat panel
    }

    public void DisplayIncomingMessage(string message)
    {
        Debug.Log($"Displaying incoming message: {message}"); // Debug for verification
        AppendMessage($"<color=#06ECF3>Rival:</color> {message}"); // light blue for "Opponent:"
    }

    private void AppendMessage(string message)
    {
        if (chatHistoryText != null)
        {
            chatHistoryText.text += $"{message}\n"; // Add message to chat history
            Debug.Log($"Appended message to chat history: {message}"); // Debug
        }
        else
        {
            Debug.LogError("ChatHistoryText reference is missing in ChatManager.");
        }
    }

}