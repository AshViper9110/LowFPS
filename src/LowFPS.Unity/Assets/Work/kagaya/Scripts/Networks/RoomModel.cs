using Cysharp.Threading.Tasks;
using LowFPS.Shared.Interfaces.Services;
using LowFPS.Shared.Interfaces.StreamingHubs;
using MagicOnion;
using MagicOnion.Client;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class RoomModel : Singleton<RoomModel>, IRoomHubReceiver {
    [SerializeField] private ServerConfigSO serverConfig;

    private GrpcChannelx channelx;
    private IRoomHub roomHub;

    /// <summary>
    /// 　接続ID
    /// </summary>
    public Guid ConnectionId { get; private set; }

    /// <summary>
    /// ユーザー名
    /// </summary>
    public string UserName { get; private set; }

    /// <summary>
    /// MagicOnionに接続しているか
    /// </summary>
    private bool isConnected = false;
    public bool IsConnected { get { return isConnected; } }

    /// <summary>
    /// ロームに入っているか
    /// </summary>
    private bool isJoinRoom = false;
    public bool IsJoinRoom { get { return isJoinRoom; } }

    /// <summary>
    /// サーバーからの受信時間
    /// </summary>
    private TimeSpan receivedSpan = TimeSpan.Zero;

    /*
    * サーバー通知
    */

    /// <summary>
    /// ユーザー接続通知
    /// </summary>
    public Action<JoinedUser> OnJoinedUser { get; set; }
    /// <summary>
    /// ユーザー退出通知
    /// </summary>
    public Action<Guid, int> OnLeavedUser { get; set; }

    /*
     * 処理
     */

    /// <summary>
    /// 　MagicOnion接続処理
    /// </summary>
    public async UniTask ConnectAsync() {
        channelx = GrpcChannelx.ForAddress(
#if DEBUG
            serverConfig.DEBUG.url
#else
            serverConfig.PRODUCTION.url
#endif
            );
        roomHub = await StreamingHubClient.
             ConnectAsync<IRoomHub, IRoomHubReceiver>(channelx, this);
        this.ConnectionId = await roomHub.GetConnectionId();
        isConnected = true;
    }

    /// <summary>
    /// MagicOnion切断処理
    /// </summary>
    public async UniTask DisconnectAsync() {
        isConnected = false;
        if (roomHub != null) await roomHub.DisposeAsync();
        if (channelx != null) await channelx.ShutdownAsync();
        roomHub = null;
        channelx = null;
    }
    /// <summary>
    /// 破棄処理
    /// </summary>
    protected override void OnDestroy() {
        base.OnDestroy();
        DisconnectAsync().Forget();
    }

    /// <summary>
    /// ゲーム終了時
    /// </summary>
    protected override void OnApplicationQuit() {
        base.OnApplicationQuit();
        DisconnectAsync().Forget();
    }

    /// <summary>
    /// 通信速度測定
    /// </summary>
    public async UniTask SpeedTestAsync() {
        if (roomHub == null) {
            throw new Exception("RoomHubがnullです。");
        }

        DateTime sendTime = DateTime.UtcNow;
        DateTime receivedTime = await roomHub.SpeedTestAsync(sendTime, receivedSpan);
        receivedSpan = DateTime.UtcNow - receivedTime;
    }

    /// <summary>
    /// ルームに入室
    /// </summary>
    public async UniTask JoinRoomAsync(string userName, string roomName) {
        if (roomHub == null) {
            throw new Exception("RoomHubがnullです。");
        }

        try {
            JoinedUser[] joinedUsers = await roomHub.JoinRoomAsync(userName, roomName);
            isJoinRoom = true;
            if (joinedUsers != null) {
                foreach (var user in joinedUsers) {
                    // 自分自身はスキップ
                    if (user.ConnectionId != ConnectionId) {
                        OnJoinedUser(user);
                    }
                }
            }
        }
        catch (Exception e) {
            Debug.LogException(e);
        }

    }


    /// <summary>
    /// [サーバー通知]
    /// ロビーの入室通知
    /// </summary>
    public void OnJoinRoom(JoinedUser user) {
        if (OnJoinedUser != null) {
            OnJoinedUser(user);
        }
    }

    /// <summary>
    /// ルームから退室
    /// </summary>
    public async UniTask LeaveRoomAsync() {
        if (roomHub == null) {
            throw new Exception("RoomHubがnullです。");
        }

        isJoinRoom = false;

        await roomHub.LeaveRoomAsync();
    }

    /// <summary>
    /// [サーバー通知]
    /// ロビーの退室通知
    /// </summary>
    public void OnLeaveRoom(Guid connectionId, int joinOrder) {
        if (OnLeavedUser != null) {
            OnLeavedUser(connectionId, joinOrder);
        }
    }
}
