using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    void Start()
    {
        // Set custom resolution and windowed mode
        Screen.SetResolution(968, 704, false);

        NetworkClientProcessing.SetGameLogic(this);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
            NetworkClientProcessing.SendMessageToServer("3,Hello server's world, sincerely your network client", TransportPipeline.ReliableAndInOrder);

    }

}
