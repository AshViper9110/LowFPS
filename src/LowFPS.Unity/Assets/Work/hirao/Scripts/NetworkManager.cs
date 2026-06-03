using LowFPS.Shared.Interfaces.Services;
using System;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : Singleton<NetworkManager>
{
    [SerializeField] private GameObject syncPlayerPrefab;
    [SerializeField] private GameObject playerPrefab;

    private GameObject myPlayer;

    public Transform spawnPoint;

    public PlayerCon MyPlayerCon
    {
        get
        {
            if (myPlayer == null)
                return null;

            return myPlayer.GetComponent<PlayerCon>();
        }
    }

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
            RoomModel.I.OnHitDamaged += OnHitDamaged;
            RoomModel.I.Ondead += Ondead;
            RoomModel.I.OnRespawned += OnRespawned;
        }
    }

    private void OnDisable()
    {
        if (RoomModel.I != null)
        {
            RoomModel.I.OnJoinedUser -= OnJoinedUser;
            RoomModel.I.OnLeavedUser -= OnLeavedUser;
            RoomModel.I.OnHitDamaged -= OnHitDamaged;
            RoomModel.I.Ondead -= Ondead;
            RoomModel.I.OnRespawned -= OnRespawned;
        }
    }

    public async void JoinRoom()
    {
        await RoomModel.I.JoinRoomAsync("User001", "TestRoom");

        myPlayer = Instantiate(
            playerPrefab,
            new Vector3(0, 1, 0),
            Quaternion.identity);

        SyncPlayer syncPlayer = myPlayer.GetComponent<SyncPlayer>();
        syncPlayer.connectionId = RoomModel.I.ConnectionId;
    }

    private void OnJoinedUser(JoinedUser joinedUser)
    {
        if (joinedUser.ConnectionId != RoomModel.I.ConnectionId)
        {
            Debug.Log($"{joinedUser.Name}が入室しました");

            GameObject user = Instantiate(syncPlayerPrefab);

            SyncPlayer syncPlayer = user.GetComponent<SyncPlayer>();
            syncPlayer.connectionId = joinedUser.ConnectionId;

            PlayerData playerData = new PlayerData()
            {
                playerObj = user,
                joinedUser = joinedUser,
            };

            InRoomPlayerData.I.AddPlayer(
                joinedUser.ConnectionId,
                playerData);
        }
        else
        {
            Debug.Log("入室しました");

            PlayerData playerData = new PlayerData()
            {
                playerObj = myPlayer,
                joinedUser = joinedUser,
            };

            InRoomPlayerData.I.SetMySelf(playerData);

            // ←追加
            InRoomPlayerData.I.AddPlayer(
                joinedUser.ConnectionId,
                playerData);
        }
    }

    private void OnLeavedUser(Guid connectionId, int joinOrder)
    {
        InRoomPlayerData.I.RemovePlayer(connectionId, joinOrder);
    }

    private void OnHitDamaged(Guid connectionId, JoinedUser joinedUser)
    {
        InRoomPlayerData.I.PlayerList[connectionId].joinedUser.Hp = joinedUser.Hp;

        if (connectionId == RoomModel.I.ConnectionId) return;
        //TODO プレイヤーの上部に被ダメージを表示

    }

    private void Ondead(Guid myConnectionId, Guid enemyConnectionId)
    {
        if (RoomModel.I.ConnectionId != myConnectionId) return;

        Debug.Log("しぼうしました");
        InRoomPlayerData.I.PlayerList[myConnectionId].joinedUser.Hp = 0;
        MyPlayerCon.Dead(enemyConnectionId);
    }
    
    private void OnRespawned(Guid connectionId, JoinedUser joinedUser)
    {
        if (RoomModel.I.ConnectionId != connectionId)
        {
            return;
        }
        Debug.Log("リスポーンしました");
        InRoomPlayerData.I.PlayerList[connectionId].joinedUser.Hp = joinedUser.Hp;

        MyPlayerCon.Respawn(spawnPoint.position);
    }
}