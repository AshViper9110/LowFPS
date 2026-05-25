using LowFPS.Shared.Interfaces.Services;
using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject syncPlayerPrefab;
    [SerializeField] private GameObject player;
    async void Start()
    {
        await RoomModel.I.ConnectAsync();
        await RoomModel.I.JoinRoomAsync("User001","TestRoom");
        SyncPlayer syncPlayer = player.GetComponent<SyncPlayer>();
        syncPlayer.connectionId = RoomModel.I.ConnectionId;
    }

    private void Awake()
    {
        RoomModel.I.OnJoinedUser += OnJoinedUser;
        RoomModel.I.OnLeavedUser += OnLeavedUser;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnJoinedUser(JoinedUser joinedUser)
    {
        if (joinedUser.ConnectionId != RoomModel.I.ConnectionId)
        {
            Debug.Log($"{joinedUser.Name}‚ª“üŽº‚µ‚Ü‚µ‚½");
            GameObject user = Instantiate(syncPlayerPrefab);
            SyncPlayer syncPlayer = user.GetComponent<SyncPlayer>();
            syncPlayer.connectionId = joinedUser.ConnectionId;
            PlayerData playerData = new PlayerData()
            {
                playerObj = user,
                joinedUser = joinedUser,
            };
            InRoomPlayerData.I.AddPlayer(joinedUser.ConnectionId, playerData);
        } else
        {
            Debug.Log($"“üŽº‚µ‚Ü‚µ‚½");
            PlayerData playerData = new PlayerData()
            {
                playerObj = player,
                joinedUser = joinedUser,
            };
            InRoomPlayerData.I.SetMySelf(playerData);
        }
    }

    private void OnLeavedUser(Guid connectionId, int joinOrder)
    {
        InRoomPlayerData.I.RemovePlayer(connectionId, joinOrder);
    }
}
