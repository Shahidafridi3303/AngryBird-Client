using UnityEngine;
using TMPro; // For TextMeshPro
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{
    [SerializeField] private GameObject chatPanel;
    [SerializeField] private TMP_InputField chatInputField;
    [SerializeField] private TextMeshProUGUI chatHistoryText;
    [SerializeField] private Button sendButton;

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
            AppendMessage($"You: {message}");
            chatInputField.text = ""; // Clear input field
        }
    }

    public void DisplayIncomingMessage(string message)
    {
        AppendMessage($"Opponent: {message}");
    }

    private void AppendMessage(string message)
    {
        chatHistoryText.text += $"{message}\n"; // Add message to chat history
    }
}