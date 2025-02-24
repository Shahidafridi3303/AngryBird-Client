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
}
