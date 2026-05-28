using LowFPS.Shared.Interfaces.Services;
using System;
using UnityEngine;

public class NetworkManager : Singleton<NetworkManager>
{
    [SerializeField] private GameObject syncPlayerPrefab;
    [SerializeField] private GameObject playerPrefab;
    private GameObject myPlayer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        await RoomModel.I.ConnectAsync();
    }
    private void OnEnable()
    {
        if (RoomModel.I != null)
        {
            RoomModel.I.OnJoinedUser += OnJoinedUser;
            RoomModel.I.OnLeavedUser += OnLeavedUser;
        }
    }

    private void OnDisable()
    {
        if (RoomModel.I != null)
        {
            RoomModel.I.OnJoinedUser -= OnJoinedUser;
            RoomModel.I.OnLeavedUser -= OnLeavedUser;
        }
    }

    async public void joinRoom()
    {
        await RoomModel.I.JoinRoomAsync("User001", "TestRoom");

        myPlayer = Instantiate(playerPrefab);
        SyncPlayer syncPlayer = myPlayer.GetComponent<SyncPlayer>();
        syncPlayer.connectionId = RoomModel.I.ConnectionId;
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
        }
        else
        {
            Debug.Log($"“üŽº‚µ‚Ü‚µ‚½");

            PlayerData playerData = new PlayerData()
            {
                playerObj = myPlayer,
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
