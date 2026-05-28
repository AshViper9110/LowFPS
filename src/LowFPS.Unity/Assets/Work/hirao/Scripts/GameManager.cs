using LowFPS.Shared.Interfaces.Services;
using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Awake()
    {
        NetworkManager.I.joinRoom();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
