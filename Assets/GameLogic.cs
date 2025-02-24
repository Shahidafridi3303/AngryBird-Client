using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    void Start()
    {
        NetworkClientProcessing.SetGameLogic(this);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
            NetworkClientProcessing.SendMessageToServer("3,Hello server's world, sincerely your network client", TransportPipeline.ReliableAndInOrder);

    }

}
