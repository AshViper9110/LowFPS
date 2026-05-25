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
    public void RemovePlayer(Guid connectionId, int joinOrder) {
        Destroy(playerList[connectionId].playerObj);
        playerList.Remove(connectionId);
        //自身の繰り下げ
        if (mySelf.joinedUser.JoinOrder > joinOrder)
        {
            mySelf.joinedUser.JoinOrder--;
        }
        //全体の繰り下げ
        foreach (PlayerData player in playerList.Values)
        {
            if(player.joinedUser.JoinOrder > joinOrder)
            {
                player.joinedUser.JoinOrder--;
            }
        }
    }
}