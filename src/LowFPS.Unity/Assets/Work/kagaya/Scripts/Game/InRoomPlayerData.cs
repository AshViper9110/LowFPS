using LowFPS.Shared.Interfaces.Services;
using System;
using System.Collections.Generic;
using UnityEngine;

public class InRoomPlayerData : Singleton<InRoomPlayerData> {
    // 自分
    private PlayerData mySelf;
    public PlayerData MySelf { get { return mySelf; } }
    // プレイヤーリスト
    private Dictionary<Guid, PlayerData> playerList;
    public Dictionary<Guid, PlayerData> PlayerList { get { return playerList; } }

    protected override void Awake() {
        base.Awake();
        playerList = new Dictionary<Guid, PlayerData>();
    }

    /// <summary>
    /// 初期化
    /// </summary>
    public void Init() {
        mySelf = null;
        playerList.Clear();
    }

    /// <summary>
    /// 自分の情報を追加
    /// </summary>
    public void SetMySelf(PlayerData self) {
        mySelf = self;
    }

    /// <summary>
    /// プレイヤーリストに追加
    /// </summary>
    public void AddPlayer(Guid connectionId, PlayerData playerData) {
        playerList[connectionId] = playerData;
    }

    /// <summary>
    /// プレイヤーリストから削除
    /// </summary>
    public void RemovePlayer(Guid connectionId, int joinOrder)
    {
        if (!playerList.TryGetValue(connectionId, out PlayerData playerData))
        {
            Debug.LogWarning($"Player not found: {connectionId}");
            return;
        }

        Destroy(playerData.playerObj);

        playerList.Remove(connectionId);

        if (mySelf != null &&
            mySelf.joinedUser != null &&
            mySelf.joinedUser.JoinOrder > joinOrder)
        {
            mySelf.joinedUser.JoinOrder--;
        }

        foreach (PlayerData player in playerList.Values)
        {
            if (player == null || player.joinedUser == null)
                continue;

            if (player.joinedUser.JoinOrder > joinOrder)
            {
                player.joinedUser.JoinOrder--;
            }
        }
    }

    public PlayerData GetPlayer(Guid connectionId)
    {
        if (playerList.TryGetValue(connectionId, out PlayerData playerData))
        {
            return playerData;
        }

        return null;
    }
}